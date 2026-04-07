using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 10;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Попали в босса
        BossAI bossAI = other.GetComponentInParent<BossAI>();
        if (bossAI != null)
        {
            Vector2 knockback = (other.transform.position - transform.position).normalized;
            bossAI.TakeDamage(damage, knockback);
            return;
        }

        // Попали в обычного врага
        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }
    }
}