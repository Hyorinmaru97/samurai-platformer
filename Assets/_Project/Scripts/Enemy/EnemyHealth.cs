using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHp = 3;
    int hp;

    [Header("UI")]
    public EnemyHpBar hpBar;

    [Header("Soul Reward")]
    public int minReward = 10;
    public int maxReward = 20;
    public GameObject soulVFXPrefab;

    EnemyController controller;
    EnemyDeath death;

    void Awake()
    {
        hp = maxHp;
        controller = GetComponent<EnemyController>();
        death = GetComponent<EnemyDeath>();
        if (hpBar) hpBar.SetPercent(1f);
    }

    public void TakeDamage(int dmg, Vector2 knockbackDir)
    {
        hp -= dmg;

        float p = Mathf.Clamp01((float)hp / maxHp);
        if (hpBar) hpBar.SetPercent(p);

        if (hp <= 0)
        {
            if (hpBar) hpBar.Hide();

            // Души
            if (SoulManager.Instance != null)
                SoulManager.Instance.AddSouls(Random.Range(minReward, maxReward + 1));

            // VFX
            if (soulVFXPrefab != null)
                Instantiate(soulVFXPrefab, transform.position, Quaternion.identity);

            if (death) death.PlayDeath();
            else Destroy(gameObject);
            return;
        }

        if (controller) controller.TakeKnockback(knockbackDir);
    }

    public void TakeDamage(int dmg)
    {
        TakeDamage(dmg, Vector2.right);
    }

    public void ResetHealth()
    {
        hp = maxHp;
        if (hpBar) hpBar.SetPercent(1f);
    }
}