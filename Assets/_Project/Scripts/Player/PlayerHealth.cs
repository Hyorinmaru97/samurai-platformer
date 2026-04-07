using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;
    int currentHealth;
    bool isDead;

    [Header("UI")]
    public Slider hpBar;
    public TextMeshProUGUI youDiedText;
    public Image deathOverlay;
    public Button restartButton;

    [Header("Invincibility")]
    public float invincibleTime = 0.5f;
    float invincibleTimer;

    [Header("Knockback")]
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.25f;

    [Header("Feedback")]
    public SpriteRenderer spriteRenderer;
    public float flashDuration = 0.08f;
    public float hitstopDuration = 0.05f;

    PlayerDash2D dash;
    Rigidbody2D rb;
    PlayerController2D controller;
    Coroutine hitstopRoutine;

    void Start()
    {
        currentHealth = maxHealth;
        dash = GetComponent<PlayerDash2D>();
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        UpdateUI();

        if (youDiedText) youDiedText.gameObject.SetActive(false);
        if (deathOverlay) deathOverlay.gameObject.SetActive(false);
        if (restartButton) restartButton.gameObject.SetActive(false);

        if (restartButton)
            restartButton.onClick.AddListener(Restart);
    }

    void Update()
    {
        if (invincibleTimer > 0)
            invincibleTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.R))
            Restart();
    }

    public void TakeDamage(int damage, Vector2 knockbackDir = default)
    {
        if (isDead) return;
        if (damage <= 0) return;
        if (dash != null && dash.IsDashing) return;
        if (invincibleTimer > 0) return;

        currentHealth -= damage;
        invincibleTimer = invincibleTime;
        UpdateUI();

        if (controller) controller.OnHit();

        StartCoroutine(FlashRoutine());

        if (hitstopRoutine == null)
            hitstopRoutine = StartCoroutine(HitstopRoutine());

        if (knockbackDir != Vector2.zero && rb != null)
            StartCoroutine(ApplyKnockback(knockbackDir));
        else
            StartCoroutine(ResetHitstunAfterDelay());

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator FlashRoutine()
    {
        if (spriteRenderer == null) yield break;
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.white;
        yield return new WaitForSecondsRealtime(flashDuration);
        spriteRenderer.color = originalColor;
    }

    IEnumerator HitstopRoutine()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitstopDuration);
        Time.timeScale = 1f;
        hitstopRoutine = null;
    }

    IEnumerator ApplyKnockback(Vector2 dir)
    {
        if (controller) controller.enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir.normalized * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSecondsRealtime(knockbackDuration);

        if (controller) controller.enabled = true;
        if (controller) controller.OnHitstunEnd();
    }

    IEnumerator ResetHitstunAfterDelay()
    {
        yield return new WaitForSecondsRealtime(knockbackDuration);
        if (controller) controller.OnHitstunEnd();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        StartCoroutine(DieRoutine());
    }

    IEnumerator DieRoutine()
    {
        if (controller) controller.enabled = false;
        if (rb) rb.linearVelocity = Vector2.zero;

        if (deathOverlay)
        {
            deathOverlay.gameObject.SetActive(true);
            var c = deathOverlay.color;
            c.a = 0f;
            deathOverlay.color = c;
        }

        if (youDiedText)
        {
            youDiedText.gameObject.SetActive(true);
            var c = youDiedText.color;
            c.a = 0f;
            youDiedText.color = c;
        }

        float elapsed = 0f;
        float fadeDuration = 1.5f;
        float overlayTargetAlpha = 0.7f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            if (deathOverlay)
            {
                var c = deathOverlay.color;
                c.a = t * overlayTargetAlpha;
                deathOverlay.color = c;
            }

            if (youDiedText)
            {
                var c = youDiedText.color;
                c.a = t;
                youDiedText.color = c;
            }

            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.3f);
        if (restartButton) restartButton.gameObject.SetActive(true);
    }

    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void UpdateUI()
    {
        if (hpBar)
        {
            hpBar.maxValue = maxHealth;
            hpBar.value = currentHealth;
        }
    }
}