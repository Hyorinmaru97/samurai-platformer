using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Урон")]
    public int damage = 1;
    public float attackCooldown = 1f;

    float cooldownTimer;

    void Update()
    {
        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;
    }

    void OnCollisionStay2D(Collision2D col)
    {
        TryDamagePlayer(col.gameObject);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        TryDamagePlayer(col.gameObject);
    }

    void TryDamagePlayer(GameObject other)
    {
        if (cooldownTimer > 0) return;

        var health = other.GetComponent<PlayerHealth>();
        if (health == null)
            health = other.GetComponentInParent<PlayerHealth>();

        if (health != null)
        {
            health.TakeDamage(damage);
            cooldownTimer = attackCooldown;
        }
    }
}
