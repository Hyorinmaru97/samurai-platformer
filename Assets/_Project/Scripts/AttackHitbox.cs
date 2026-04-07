using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public int damage = 1;

    // кого уже ударили в текущей атаке
    private readonly HashSet<EnemyHealth> hitThisSwing = new HashSet<EnemyHealth>();

    void OnEnable()
    {
        hitThisSwing.Clear();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryHit(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // важно: если мы уже стоим внутри врага и нажали атаку — тоже нанесёт урон
        TryHit(other);
    }

    void TryHit(Collider2D other)
    {
        var enemy = other.GetComponentInParent<EnemyHealth>();
        if (!enemy) return;

        if (hitThisSwing.Contains(enemy)) return;

        hitThisSwing.Add(enemy);
        enemy.TakeDamage(damage);
    }
}
