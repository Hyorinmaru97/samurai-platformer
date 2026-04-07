using UnityEngine;

// Повесь на каждый PlayerHitbox1/2/3
public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 10;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Попали в босса
        BossHealth bossHealth = other.GetComponent<BossHealth>();
        if (bossHealth != null)
        {
            bossHealth.TakeDamage(damage);
            return;
        }

        // Попали в обычного врага
        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }
    }
}
