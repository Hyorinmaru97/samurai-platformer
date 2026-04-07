using UnityEngine;
using System.Collections;

public class PlayerDash2D : MonoBehaviour
{
    [Header("Ground Dash")]
    public float dashDistance = 2.5f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 0.8f;

    [Header("Air Dash (1й прыжок)")]
    public float airDashDistance = 3f;
    public float airDashDuration = 0.3f;

    [Header("Special Dash (2й прыжок)")]
    public float specialDashDistance = 5f;
    public float specialDashDuration = 0.3f;

    [Header("Visual")]
    public Transform visualRoot;
    public float dashLeanAngle = 35f;

    [Header("Trail — пыль")]
    public GameObject dustPrefab;
    public Transform dustSpawnPoint;
    public float dustInterval = 0.05f;

    [Header("I-frames")]
    public Collider2D playerCollider;
    public LayerMask enemyLayer;

    Rigidbody2D rb;
    Animator animator;
    PlayerController2D controller;

    bool isDashing;
    bool canDash = true;
    bool airDashUsed = false;
    int lastJumpCount = 0;
    int facing = 1;
    float lockedY;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        controller = GetComponent<PlayerController2D>();

        if (!playerCollider)
            playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        if (x > 0.01f) facing = 1;
        else if (x < -0.01f) facing = -1;

        if (controller != null)
        {
            if (controller.IsGrounded)
            {
                airDashUsed = false;
                lastJumpCount = 0;
            }
            else if (controller.JumpCount != lastJumpCount)
            {
                airDashUsed = false;
                lastJumpCount = controller.JumpCount;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
        {
            if (controller != null && controller.IsGrounded && canDash)
            {
                StartCoroutine(DashRoutine(DashType.Ground));
            }
            else if (controller != null && !controller.IsGrounded && !airDashUsed)
            {
                airDashUsed = true;
                if (controller.JumpCount >= 2)
                    StartCoroutine(DashRoutine(DashType.Special));
                else
                    StartCoroutine(DashRoutine(DashType.Air));
            }
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            transform.position = new Vector3(
                transform.position.x,
                lockedY,
                transform.position.z);
        }
    }

    enum DashType { Ground, Air, Special }

    IEnumerator DashRoutine(DashType type)
    {
        isDashing = true;

        // Сбрасываем комбо при старте даша
        if (controller != null) controller.OnDashStarted();

        if (type == DashType.Ground) canDash = false;

        lockedY = transform.position.y;

        if (animator)
        {
            switch (type)
            {
                case DashType.Ground:
                    animator.SetBool("IsDashing", true);
                    break;
                case DashType.Air:
                    animator.SetBool("IsAirDashing", true);
                    break;
                case DashType.Special:
                    animator.SetBool("IsSpecialDashing", true);
                    break;
            }
        }

        SetEnemyCollision(false);
        StartCoroutine(SpawnDust());

        if (visualRoot)
            StartCoroutine(LeanRoutine());

        float origGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float dist = type == DashType.Ground  ? dashDistance        :
                     type == DashType.Air     ? airDashDistance     : specialDashDistance;
        float dur  = type == DashType.Ground  ? dashDuration        :
                     type == DashType.Air     ? airDashDuration     : specialDashDuration;

        float speed = dist / dur;
        rb.linearVelocity = new Vector2(facing * speed, 0f);

        yield return new WaitForSeconds(dur);

        rb.gravityScale = origGravity;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        SetEnemyCollision(true);

        // Сбрасываем все dash-состояния
        if (animator)
        {
            animator.SetBool("IsDashing",        false);
            animator.SetBool("IsAirDashing",     false);
            animator.SetBool("IsSpecialDashing", false);
        }

        isDashing = false;

        if (type == DashType.Ground)
        {
            yield return new WaitForSeconds(dashCooldown);
            canDash = true;
        }
    }

    IEnumerator LeanRoutine()
    {
        float targetAngle = facing * dashLeanAngle;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / 0.03f;
            float angle = Mathf.LerpAngle(0f, targetAngle, t);
            visualRoot.localRotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        yield return new WaitForSeconds(dashDuration - 0.05f);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.08f;
            float angle = Mathf.LerpAngle(targetAngle, 0f, t);
            visualRoot.localRotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }
    }

    IEnumerator SpawnDust()
    {
        if (!dustPrefab || !dustSpawnPoint) yield break;

        float elapsed = 0f;
        float dur = dashDuration;
        while (elapsed < dur)
        {
            Instantiate(dustPrefab, dustSpawnPoint.position, Quaternion.identity);
            yield return new WaitForSeconds(dustInterval);
            elapsed += dustInterval;
        }
    }

    void SetEnemyCollision(bool enable)
    {
        var enemies = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
        foreach (var e in enemies)
        {
            if (((1 << e.gameObject.layer) & enemyLayer) != 0)
                Physics2D.IgnoreCollision(playerCollider, e, !enable);
        }
    }

    public bool IsDashing => isDashing;
}
