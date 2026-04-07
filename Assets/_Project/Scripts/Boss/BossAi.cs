using UnityEngine;
using System.Collections;

public class BossAI : MonoBehaviour
{
    public enum BossState { Idle, Walk, Death }

        [Header("AI")]
        public float detectionRange = 10f;
        public float meleeRange = 1.8f;
        public float stopDistance = 0.3f;
        public float moveSpeed = 2f;
        public float farDistance = 6f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Refs")]
    public Animator animator;
    public BossCombat combat;
    public Collider2D bossHurtBox;

    Rigidbody2D rb;
    Collider2D bodyCollider;
    Transform player;
    BossHealth bossHealth;

    BossState state = BossState.Idle;
    bool isDead;
    bool isGrounded;
    bool isHurt;
    int comboStep = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        bossHealth = GetComponent<BossHealth>();

        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!combat) combat = GetComponent<BossCombat>();

        if (animator == null)
            Debug.LogError("BossAI: Animator not found", this);
        if (rb == null)
            Debug.LogError("BossAI: Rigidbody2D not found", this);
        if (combat == null)
            Debug.LogError("BossAI: BossCombat not found", this);
    }

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p) player = p.transform;
    }

    void Update()
    {
        if (isDead) return;
        if (player == null) return;

        UpdateGrounded();

        float dist = Vector2.Distance(transform.position, player.position);

        switch (state)
        {
            case BossState.Idle:
                animator.SetBool("IsWalking", false);
                StopHorizontal();
                if (dist <= detectionRange)
                    state = BossState.Walk;
                break;

            case BossState.Walk:
                if (combat == null) break;

                if (dist > detectionRange)
                {
                    animator.SetBool("IsWalking", false);
                    StopHorizontal();
                    comboStep = 0;
                    state = BossState.Idle;
                    break;
                }

                if (combat.IsAttacking || isHurt)
                {
                    animator.SetBool("IsWalking", false);
                    StopHorizontal();
                    break;
                }

                FlipToPlayer();

            if (combat.CanAttack)
            {
                if (dist <= meleeRange)
                {
                    if (comboStep == 0 || comboStep == 1)
                    {
                        combat.TryAttack(1);
                        comboStep++;
                    }
                    else if (comboStep == 2)
                    {
                        combat.TryAttack(2);
                        comboStep = 0;
                    }
                    break;
                }
                else if (dist > farDistance && isGrounded)
                {
                    combat.TryAttack(3);
                    comboStep = 0;
                    break;
                }
            }

                animator.SetBool("IsWalking", true);
                MoveToPlayer();
                break;

            case BossState.Death:
                StopHorizontal();
                break;
        }
    }

    void UpdateGrounded()
    {
        if (!groundCheck) return;
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    void MoveToPlayer()
    {
        if (!player) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist < stopDistance)
        {
            StopHorizontal();
            return;
        }

        if (dist < meleeRange)
        {
            StopHorizontal();
            return;
        }

        float dir = player.position.x > transform.position.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
    }

    void StopHorizontal()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    public void TakeDamage(int dmg, Vector2 knockbackDir)
    {
        if (isDead) return;

        if (bossHealth) bossHealth.TakeDamage(dmg);

        if (!isHurt)
            StartCoroutine(HurtRoutine());

        if (knockbackDir != Vector2.zero && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockbackDir.normalized * 3f, ForceMode2D.Impulse);
        }
    }

    IEnumerator HurtRoutine()
    {
        isHurt = true;
        animator.SetTrigger("Hurt");
        yield return new WaitForSecondsRealtime(0.4f);
        isHurt = false;
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        isHurt = false;
        state = BossState.Death;
        StopAllCoroutines();

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        if (bodyCollider) bodyCollider.enabled = false;
        if (bossHurtBox) bossHurtBox.enabled = false;
        if (combat != null) combat.DisableHitbox();

        animator.SetBool("IsWalking", false);
        animator.SetTrigger("Death");

        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        yield return new WaitForSecondsRealtime(5f);
        Destroy(gameObject);
    }

    void FlipToPlayer()
    {
        if (player == null) return;
        float diff = player.position.x - transform.position.x;
        if (Mathf.Abs(diff) < 0.1f) return;
        float dir = diff > 0 ? 1f : -1f;
        Vector3 scale = transform.localScale;
        scale.x = dir > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        if (groundCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}