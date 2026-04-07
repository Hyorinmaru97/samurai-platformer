using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum State { Idle, Chase, Attack, Return }

    [Header("Параметры")]
    public float aggroDistance = 5f;
    public float attackDistance = 1.2f;
    public float deAggroDistance = 10f;
    public float moveSpeed = 2f;
    public float returnSpeed = 3f;
    public float attackCooldown = 1f;
    public int damage = 2;

    [Header("Прыжок")]
    public float jumpForce = 8f;
    public float wallCheckDistance = 0.3f;
    public LayerMask groundLayer;

    Rigidbody2D rb;
    EnemyController controller;
    Transform player;
    float attackTimer;
    State state = State.Idle;
    Vector3 startPosition;
    bool isGrounded;

    public bool IsMoving => state == State.Chase || state == State.Return;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<EnemyController>();
    }

    void Start()
    {
        startPosition = transform.position;
        var p = GameObject.FindWithTag("Player");
        if (p) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        attackTimer -= Time.deltaTime;
        float dist = Vector2.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Idle:
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                if (dist <= aggroDistance)
                    state = State.Chase;
                break;

            case State.Chase:
                if (dist <= attackDistance)
                    state = State.Attack;
                else if (dist >= deAggroDistance)
                    state = State.Return;
                else
                    MoveTowards(player.position);
                break;

            case State.Attack:
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                if (dist > attackDistance)
                {
                    state = State.Chase;
                    break;
                }
                if (attackTimer <= 0f)
                {
                    DoAttack();
                    attackTimer = attackCooldown;
                }
                break;

            case State.Return:
                float distToStart = Vector2.Distance(transform.position, startPosition);
                if (distToStart < 0.2f)
                {
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                    state = State.Idle;
                    var health = GetComponent<EnemyHealth>();
                    if (health) health.ResetHealth();
                }
                else
                {
                    MoveTowards(startPosition, returnSpeed);
                }
                if (dist <= aggroDistance)
                    state = State.Chase;
                break;
        }
    }

    void MoveTowards(Vector3 target, float speed = -1f)
    {
        if (speed < 0) speed = moveSpeed;
        float dir = target.x > transform.position.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

        CheckAndJump(dir);

        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr)
        {
            Vector3 scale = sr.transform.localScale;
            scale.x = dir > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            sr.transform.localScale = scale;
        }

        var hpBar = GetComponentInChildren<EnemyHpBar>();
        if (hpBar)
        {
            Vector3 s = hpBar.transform.localScale;
            s.x = dir > 0 ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
            hpBar.transform.localScale = s;
        }
    }

        void CheckAndJump(float dir)
        {
            isGrounded = Physics2D.OverlapBox(
                new Vector2(transform.position.x, transform.position.y - 1.5f),
                new Vector2(0.4f, 0.1f), 0f, groundLayer
            );

            bool wallAhead = Physics2D.Raycast(
                transform.position,
                new Vector2(dir, 0),
                wallCheckDistance,
                groundLayer
            );

            bool groundAhead = Physics2D.Raycast(
                new Vector2(transform.position.x + dir * 0.5f, transform.position.y),
                Vector2.down,
                1.5f,
                groundLayer
            );

            Debug.Log($"isGrounded={isGrounded}, wallAhead={wallAhead}, groundAhead={groundAhead}");

            if ((wallAhead || !groundAhead) && isGrounded)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    void DoAttack()
    {
        var health = player.GetComponent<PlayerHealth>();
        if (health == null)
            health = player.GetComponentInParent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(damage);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            new Vector3(transform.position.x, transform.position.y - 1.5f, 0),
            new Vector3(0.4f, 0.1f, 0)
        );
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, deAggroDistance);
    }
}
