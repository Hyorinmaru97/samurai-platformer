using UnityEngine;
using System.Collections;

public class PlayerAttack2D : MonoBehaviour
{
    public enum AttackType { Slash, Lunge, Low }

    [Header("Refs")]
    public SlashPivotSwing swing;
    public Transform visualRoot;
    public Transform attackRoot;
    public GameObject attackVfx;
    public Transform weaponRig;

    [Header("Animator")]
    public Animator animator;

    [Header("Hitboxes")]
    public Transform hitboxSlash;
    public Transform hitboxLunge;
    public Transform hitboxLow;

    [Header("Размеры хитбоксов")]
    public Vector2 sizeSlash = new Vector2(1.2f, 0.8f);
    public Vector2 sizeLunge = new Vector2(2f,   0.5f);
    public Vector2 sizeLow   = new Vector2(1f,   0.6f);

    [Header("Урон")]
    public int damageSlash = 1;
    public int damageLunge = 2;
    public int damageLow   = 1;

    [Header("Timing")]
    public float cooldown    = 0.3f;
    public float hitDelay    = 0.07f;

    [Header("Facing")]
    public LayerMask enemyLayer;

    float cd;
    int facing = 1;
    Vector3 stanceDefault;

    void Start()
    {
        if (!swing) swing = GetComponentInChildren<SlashPivotSwing>(true);
        if (!visualRoot) visualRoot = transform.Find("VisualRoot");
        if (visualRoot) stanceDefault = visualRoot.localPosition;

        // Если animator не назначен — ищем на SamuraiSprite
        if (!animator) animator = GetComponentInChildren<Animator>();

        SetHitboxActive(hitboxSlash, false);
        SetHitboxActive(hitboxLunge, false);
        SetHitboxActive(hitboxLow,   false);
    }

    void Update()
    {
        cd -= Time.deltaTime;
        UpdateFacing();
        ApplyFacing();

        if (swing && visualRoot)
            swing.transform.localRotation = Quaternion.Inverse(visualRoot.localRotation);

        if (cd > 0f) return;

        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift))
        {
            StartCoroutine(DoAttack(AttackType.Lunge));
            cd = cooldown;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(DoAttack(AttackType.Slash));
            cd = cooldown;
        }
        else if (Input.GetMouseButtonDown(1))
        {
            StartCoroutine(DoAttack(AttackType.Low));
            cd = cooldown;
        }
    }

    IEnumerator DoAttack(AttackType type)
    {
        // Запускаем анимацию атаки
        if (animator)
        {
            switch (type)
            {
                case AttackType.Slash:
                    animator.SetTrigger("Attack1");
                    break;
                case AttackType.Lunge:
                    animator.SetTrigger("Attack2");
                    break;
                case AttackType.Low:
                    animator.SetTrigger("Attack3");
                    break;
            }
        }

        if (swing) swing.PlaySwing(GetSwingType(type));
        if (attackVfx) StartCoroutine(FlashVfx());

        yield return new WaitForSeconds(hitDelay);

        Transform hitbox = GetHitbox(type);
        SetHitboxActive(hitbox, true);

        var hits = Physics2D.OverlapBoxAll(
            hitbox.position, GetSize(type), 0f, enemyLayer);

        foreach (var h in hits)
        {
            Vector2 dir = (h.transform.position - transform.position).normalized;

            var enemyHp = h.GetComponentInParent<EnemyHealth>();
            if (enemyHp) enemyHp.TakeDamage(GetDamage(type), dir);

            var bossAI = h.GetComponentInParent<BossAI>();
            if (bossAI) bossAI.TakeDamage(GetDamage(type), dir);
        }

        yield return new WaitForSeconds(0.1f);
        SetHitboxActive(hitbox, false);

        yield return new WaitForSeconds(0.1f);
        if (visualRoot) visualRoot.localPosition = stanceDefault;
    }

    void SetHitboxActive(Transform t, bool active)
    {
        if (!t) return;
        var vis = t.Find("_HitboxVisual");
        if (vis) vis.gameObject.SetActive(active);

        var col = t.GetComponent<BoxCollider2D>();
        if (col) col.enabled = active;
    }

    SlashPivotSwing.SwingType GetSwingType(AttackType type) => type switch
    {
        AttackType.Lunge => SlashPivotSwing.SwingType.Lunge,
        AttackType.Low   => SlashPivotSwing.SwingType.Low,
        _                => SlashPivotSwing.SwingType.Slash
    };

    Transform GetHitbox(AttackType type) => type switch
    {
        AttackType.Slash => hitboxSlash,
        AttackType.Lunge => hitboxLunge,
        AttackType.Low   => hitboxLow,
        _                => hitboxSlash
    };

    Vector2 GetSize(AttackType type) => type switch
    {
        AttackType.Slash => sizeSlash,
        AttackType.Lunge => sizeLunge,
        AttackType.Low   => sizeLow,
        _                => sizeSlash
    };

    int GetDamage(AttackType type) => type switch
    {
        AttackType.Slash => damageSlash,
        AttackType.Lunge => damageLunge,
        AttackType.Low   => damageLow,
        _                => 1
    };

    void UpdateFacing()
    {
        float x = Input.GetAxisRaw("Horizontal");
        if (x > 0.01f) facing = 1;
        else if (x < -0.01f) facing = -1;
    }

    void ApplyFacing()
    {
        if (visualRoot)
        {
            Vector3 s = visualRoot.localScale;
            s.x = Mathf.Abs(s.x) * facing;
            visualRoot.localScale = s;
        }

        if (weaponRig)
        {
            Vector3 s = weaponRig.localScale;
            s.x = Mathf.Abs(s.x) * facing;
            weaponRig.localScale = s;
        }
    }

    IEnumerator FlashVfx()
    {
        attackVfx.SetActive(true);
        yield return new WaitForSeconds(0.08f);
        attackVfx.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        void Draw(Transform t, Vector2 s, Color c)
        {
            if (!t) return;
            Gizmos.color = c;
            Gizmos.DrawWireCube(t.position, s);
        }
        Draw(hitboxSlash, sizeSlash, Color.red);
        Draw(hitboxLunge, sizeLunge, Color.blue);
        Draw(hitboxLow,   sizeLow,   Color.green);
    }
}
