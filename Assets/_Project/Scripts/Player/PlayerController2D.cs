using UnityEngine;
using System.Collections;

public class PlayerController2D : MonoBehaviour
{
    [Header("Move")]
    public float walkSpeed = 4f;
    public float runSpeed = 8f;
    public float maxSpeed = 12f;
    public float airControlMultiplier = 0.6f;

    [Header("Sprint (AA/DD)")]
    public float doubleTapWindow = 1.0f;
    public float sprintGraceTime = 0.15f;
    bool isSprinting;
    float lastForwardTap = -1f;
    float lastBackTap = -1f;
    int sprintDirection = 0;
    float sprintGraceTimer = 0f;

    [Header("Turn")]
    bool isTurning;

    [Header("Jump")]
    public float jumpVelocity = 14f;
    public float coyoteTime = 0.12f;
    public float jumpBuffer = 0.12f;
    public float jumpCutMultiplier = 0.5f;

    [Header("Double Jump")]
    public int maxJumps = 2;
    int jumpCount;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.6f, 0.2f);
    public LayerMask groundLayer;

    [Header("Lean")]
    public Transform visualRoot;
    public float maxLean = 12f;
    public float leanSpeed = 8f;

    [Header("Animation")]
    public Animator animator;

    [Header("Combo")]
    public float comboWindow = 0.5f;

    [Header("Attack Hitboxes")]
    public GameObject hitbox1;
    public GameObject hitbox2;
    public GameObject hitbox3;

    Rigidbody2D rb;
    float xInput;
    float coyoteTimer;
    float jumpBufferTimer;
    bool isGrounded;
    bool wasGrounded;

    int currentAttack = 0;          // 0 = нет атаки, 1/2/3 = текущий удар
    bool animPlaying = false;       // жесткий лок на время анимации
    bool inComboWindow = false;     // открыто ли окно для следующего удара
    bool comboConsumed = false;     // использован ли клик в текущем окне
    Coroutine comboWindowRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        SetAllHitboxes(false);
    }

    void Update()
    {
        HandleFlip();
        HandleSprintDetection();
        HandleTurnDetection();

        xInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferTimer = jumpBuffer;
        else
            jumpBufferTimer -= Time.deltaTime;

        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        if (isGrounded && !wasGrounded)
        {
            jumpCount = 0;
            isSprinting = false;
            sprintDirection = 0;
            if (animator != null) animator.ResetTrigger("JumpTrigger");
        }

        if (isGrounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.deltaTime;

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            DoJump();
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }
        else if (Input.GetKeyDown(KeyCode.Space) && !isGrounded && jumpCount < maxJumps)
        {
            DoJump();
        }

        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);

        if (Mathf.Abs(xInput) < 0.1f)
        {
            sprintGraceTimer -= Time.deltaTime;
            if (sprintGraceTimer <= 0f && !isTurning)
            {
                isSprinting = false;
                sprintDirection = 0;
            }
        }
        else
        {
            sprintGraceTimer = sprintGraceTime;
        }

        HandleAttackInput();

        if (animator != null)
        {
            animator.SetBool("IsRunning", Mathf.Abs(xInput) > 0.1f);
            animator.SetBool("IsSprinting", isSprinting);
            animator.SetBool("IsTurning", isTurning);
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("IsFalling", rb.linearVelocity.y < -1f && !isGrounded);
        }
    }

    void HandleAttackInput()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (animator == null) return;
        if (animator.GetBool("InHitstun")) return;

        // Жесткий лок: пока анимация идет, любые клики игнор
        if (animPlaying) return;

        // Если окно не открыто — стартуем только первую атаку
        if (!inComboWindow)
        {
            StartAttack(1);
            return;
        }

        // Если окно открыто — ровно один клик на следующий удар
        if (inComboWindow && !comboConsumed && currentAttack > 0 && currentAttack < 3)
        {
            comboConsumed = true;
            inComboWindow = false;
            StartAttack(currentAttack + 1);
        }
    }

    void StartAttack(int step)
    {
        if (step < 1 || step > 3) return;
        if (animator == null) return;

        if (comboWindowRoutine != null)
        {
            StopCoroutine(comboWindowRoutine);
            comboWindowRoutine = null;
        }

        if (visualRoot) visualRoot.localPosition = Vector3.zero;

        currentAttack = step;
        animPlaying = true;
        inComboWindow = false;
        comboConsumed = false;

        animator.SetInteger("ActionType", step - 1); // 0,1,2
        animator.ResetTrigger("DoAction");
        animator.SetTrigger("DoAction");
    }

    IEnumerator ComboWindowRoutine()
    {
        yield return new WaitForSeconds(comboWindow);

        if (inComboWindow && !animPlaying)
            ResetCombo();

        comboWindowRoutine = null;
    }

    // ===== Animation Events =====

    public void OnAttack1Enter()
    {
        currentAttack = 1;
        animPlaying = true;
    }

    public void OnAttack2Enter()
    {
        currentAttack = 2;
        animPlaying = true;
    }

    public void OnAttack3Enter()
    {
        currentAttack = 3;
        animPlaying = true;
    }

    public void EnableHitbox1()
    {
        SetAllHitboxes(false);
        if (hitbox1) hitbox1.SetActive(true);
    }

    public void EnableHitbox2()
    {
        SetAllHitboxes(false);
        if (hitbox2) hitbox2.SetActive(true);
    }

    public void EnableHitbox3()
    {
        SetAllHitboxes(false);
        if (hitbox3) hitbox3.SetActive(true);
    }

    public void OnHitboxOff()
    {
        SetAllHitboxes(false);
    }

    public void OnAttackEnd()
    {
        if (currentAttack <= 0) return;

        animPlaying = false;
        SetAllHitboxes(false);

        if (currentAttack < 3)
        {
            inComboWindow = true;
            comboConsumed = false;

            if (comboWindowRoutine != null)
                StopCoroutine(comboWindowRoutine);

            comboWindowRoutine = StartCoroutine(ComboWindowRoutine());
        }
        else
        {
            ResetCombo();
        }
    }

    void ResetCombo()
    {
        if (comboWindowRoutine != null)
        {
            StopCoroutine(comboWindowRoutine);
            comboWindowRoutine = null;
        }

        currentAttack = 0;
        animPlaying = false;
        inComboWindow = false;
        comboConsumed = false;
        SetAllHitboxes(false);
    }

    void SetAllHitboxes(bool active)
    {
        if (hitbox1) hitbox1.SetActive(active);
        if (hitbox2) hitbox2.SetActive(active);
        if (hitbox3) hitbox3.SetActive(active);
    }

    public void OnDashStarted()
    {
        ResetCombo();
    }

    public void OnHit()
    {
        if (animator == null) return;

        ResetCombo();
        animator.SetBool("InHitstun", true);
        animator.SetBool("CanAct", false);
    }

    public void OnHitstunEnd()
    {
        if (animator == null) return;

        animator.ResetTrigger("DoAction");
        animator.SetBool("InHitstun", false);
        animator.SetBool("CanAct", true);
    }

    public int JumpCount => jumpCount;
    public bool IsGrounded => isGrounded;
    public bool IsSprinting => isSprinting;

    void HandleFlip()
    {
        if (!visualRoot) return;

        if (xInput > 0.1f)
            visualRoot.localScale = new Vector3(
                Mathf.Abs(visualRoot.localScale.x),
                visualRoot.localScale.y,
                visualRoot.localScale.z
            );
        else if (xInput < -0.1f)
            visualRoot.localScale = new Vector3(
                -Mathf.Abs(visualRoot.localScale.x),
                visualRoot.localScale.y,
                visualRoot.localScale.z
            );
    }

    void HandleSprintDetection()
    {
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (Time.time - lastForwardTap < doubleTapWindow)
            {
                isSprinting = true;
                sprintDirection = 1;
                sprintGraceTimer = sprintGraceTime;
            }
            lastForwardTap = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (Time.time - lastBackTap < doubleTapWindow)
            {
                isSprinting = true;
                sprintDirection = -1;
                sprintGraceTimer = sprintGraceTime;
            }
            lastBackTap = Time.time;
        }
    }

    void HandleTurnDetection()
    {
        if (!isSprinting || Mathf.Abs(xInput) < 0.1f)
        {
            isTurning = false;
            return;
        }

        int currentDir = xInput > 0 ? 1 : -1;
        isTurning = (currentDir != sprintDirection);

        if (isTurning)
            sprintDirection = currentDir;
    }

    void DoJump()
    {
        if (animator != null)
            animator.ResetTrigger("JumpTrigger");

        jumpCount++;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);

        if (animator != null)
        {
            if (jumpCount >= 2) animator.SetTrigger("DoubleJumpTrigger");
            else animator.SetTrigger("JumpTrigger");
        }
    }

    void FixedUpdate()
    {
        var dash = GetComponent<PlayerDash2D>();
        if (dash != null && dash.IsDashing) return;

        float targetSpeed = Mathf.Min(isSprinting ? runSpeed : walkSpeed, maxSpeed);
        float control = isGrounded ? 1f : airControlMultiplier;
        rb.linearVelocity = new Vector2(xInput * targetSpeed * control, rb.linearVelocity.y);
    }

    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
}