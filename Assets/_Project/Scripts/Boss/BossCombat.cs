using UnityEngine;
using System.Collections;

public class BossCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackCooldown = 2f;

    [Header("Jump Attack")]
    public float jumpDuration = 0.6f;
    public float jumpArcHeight = 2f;
    public float jumpHitboxDuration = 0.15f;
    public float jumpRecoveryDelay = 0.1f;

    [Header("Projectile")]
    public float projectileSpeed = 8f;

    [Header("References")]
    public BossAttackHitbox attack1Hitbox;
    public BossAttackHitbox attack2Hitbox;
    public BossAttackHitbox attack3Hitbox;
    public Transform projectileSpawnPoint;
    public GameObject magicProjectilePrefab;

    Animator animator;
    Rigidbody2D rb;
    Transform player;

    float originalGravityScale;
    bool canAttack = true;
    bool isAttacking = false;
    int currentAttackIndex = 0;
    Coroutine cooldownRoutine;
    Coroutine jumpRoutine;

    public bool CanAttack => canAttack;
    public bool IsAttacking => isAttacking;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        originalGravityScale = rb != null ? rb.gravityScale : 1f;

        canAttack = true;
        isAttacking = false;
        currentAttackIndex = 0;

        if (animator == null)
            Debug.LogError("BossCombat: Animator not found", this);
        if (rb == null)
            Debug.LogError("BossCombat: Rigidbody2D not found", this);
    }

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        if (p) player = p.transform;
    }

    public void TryAttack(int attackIndex)
    {
        if (!canAttack) return;
        if (isAttacking) return;
        if (attackIndex < 1 || attackIndex > 3) return;

        currentAttackIndex = attackIndex;
        StartAttack();
    }

    void StartAttack()
    {
        if (animator == null || rb == null)
        {
            currentAttackIndex = 0;
            return;
        }

        if (currentAttackIndex < 1 || currentAttackIndex > 3)
        {
            isAttacking = false;
            canAttack = true;
            currentAttackIndex = 0;
            return;
        }

        isAttacking = true;
        canAttack = false;

        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
        animator.ResetTrigger("Attack3");

        switch (currentAttackIndex)
        {
            case 1:
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                animator.SetTrigger("Attack1");
                break;
            case 2:
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                animator.SetTrigger("Attack2");
                break;
            case 3:
                animator.SetTrigger("Attack3");
                if (player != null)
                {
                    Vector2 target = new Vector2(player.position.x, transform.position.y);
                    if (jumpRoutine != null) StopCoroutine(jumpRoutine);
                    jumpRoutine = StartCoroutine(JumpToPlayer(target));
                }
                else
                {
                    isAttacking = false;
                    canAttack = true;
                    currentAttackIndex = 0;
                }
                break;
        }
    }

    IEnumerator JumpToPlayer(Vector2 target)
    {
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        Vector2 start = rb.position;
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            if (!isAttacking)
            {
                rb.gravityScale = originalGravityScale;
                jumpRoutine = null;
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / jumpDuration);
            float arc = Mathf.Sin(t * Mathf.PI) * jumpArcHeight;
            Vector2 pos = Vector2.Lerp(start, target, t);
            pos.y += arc;
            rb.MovePosition(pos);
            yield return null;
        }

        rb.gravityScale = originalGravityScale;
        rb.linearVelocity = Vector2.zero;

        EnableAttack3Hitbox();
        yield return new WaitForSecondsRealtime(jumpHitboxDuration);
        DisableHitbox();
        yield return new WaitForSecondsRealtime(jumpRecoveryDelay);

        jumpRoutine = null;
        EndAttack();
    }

    public void EnableAttack1Hitbox()
    {
        if (attack1Hitbox) attack1Hitbox.Activate();
    }

    public void EnableAttack2Hitbox()
    {
        if (attack2Hitbox) attack2Hitbox.Activate();
    }

    public void EnableAttack3Hitbox()
    {
        if (attack3Hitbox) attack3Hitbox.Activate();
    }

    public void DisableHitbox()
    {
        if (attack1Hitbox) attack1Hitbox.Deactivate();
        if (attack2Hitbox) attack2Hitbox.Deactivate();
        if (attack3Hitbox) attack3Hitbox.Deactivate();
    }

    public void SpawnProjectile()
    {
        if (magicProjectilePrefab == null || projectileSpawnPoint == null) return;

        GameObject proj = Instantiate(
            magicProjectilePrefab,
            projectileSpawnPoint.position,
            Quaternion.identity
        );

        float dir = transform.localScale.x > 0 ? 1f : -1f;
        Rigidbody2D rbProj = proj.GetComponent<Rigidbody2D>();
        if (rbProj) rbProj.linearVelocity = new Vector2(dir * projectileSpeed, 0f);
    }

    public void EndAttack()
    {
        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
        animator.ResetTrigger("Attack3");

        if (!isAttacking) return;

        if (jumpRoutine != null)
        {
            StopCoroutine(jumpRoutine);
            jumpRoutine = null;
            rb.gravityScale = originalGravityScale;
        }

        DisableHitbox();

        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        isAttacking = false;

        if (cooldownRoutine != null) StopCoroutine(cooldownRoutine);
        cooldownRoutine = StartCoroutine(CooldownRoutine());
    }

    IEnumerator CooldownRoutine()
        {
            yield return new WaitForSecondsRealtime(attackCooldown);
            canAttack = true;
            cooldownRoutine = null;
        }
}