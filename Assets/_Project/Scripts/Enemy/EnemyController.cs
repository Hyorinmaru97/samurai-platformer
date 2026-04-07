using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Knockback")]
    public float knockbackForceX = 5f;
    public float knockbackForceY = 3f;
    public float knockbackDuration = 0.3f;

    [Header("Flash при ударе")]
    public SpriteRenderer spriteRenderer;
    public Color hitColor = Color.red;
    public float flashDuration = 0.1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
    public LayerMask groundLayer;

    Rigidbody2D rb;
    bool isKnockedBack;
    Color originalColor;
    float lockedX;

    public bool IsKnockedBack => isKnockedBack;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!spriteRenderer)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer)
            originalColor = spriteRenderer.color;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Start()
    {
        lockedX = transform.position.x;
    }

        void FixedUpdate()
        {
            if (!isKnockedBack)
            {
                var ai = GetComponent<EnemyAI>();
                if (ai != null && ai.IsMoving)
                {
                    lockedX = transform.position.x;
                    return;
                }

                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                transform.position = new Vector3(lockedX, transform.position.y, 0f);
            }
            else
            {
                lockedX = transform.position.x;
            }
        }

    public void TakeKnockback(Vector2 direction)
    {
        if (isKnockedBack) return;
        StartCoroutine(KnockbackRoutine(direction));
    }

    IEnumerator KnockbackRoutine(Vector2 direction)
    {
        isKnockedBack = true;

        if (spriteRenderer)
            spriteRenderer.color = hitColor;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(
            direction.x * knockbackForceX,
            knockbackForceY
        ), ForceMode2D.Impulse);

        yield return new WaitForSeconds(flashDuration);

        if (spriteRenderer)
            spriteRenderer.color = originalColor;

        yield return new WaitForSeconds(knockbackDuration);

        yield return new WaitUntil(() => IsGrounded());

        rb.linearVelocity = Vector2.zero;
        lockedX = transform.position.x;
        isKnockedBack = false;
    }

    bool IsGrounded()
    {
        if (!groundCheck) return true;
        return Physics2D.OverlapBox(
            groundCheck.position, groundCheckSize, 0f, groundLayer);
    }

    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
}
