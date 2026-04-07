using UnityEngine;
using System.Collections.Generic;

public class BossAttackHitbox : MonoBehaviour
{
    [Header("Settings")]
    public int damage = 1;

    Collider2D col;
    HashSet<Collider2D> hit = new HashSet<Collider2D>();
    int playerLayer;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError("No Collider2D on BossAttackHitbox");
            return;
        }
        col.enabled = false;
        playerLayer = LayerMask.NameToLayer("Player");
    }

    public void Activate()
    {
        hit.Clear();
        col.enabled = true;
    }

    public void Deactivate()
    {
        col.enabled = false;
        hit.Clear();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryHit(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryHit(other);
    }

    void TryHit(Collider2D other)
    {
        if (other.gameObject.layer != playerLayer) return;
        if (hit.Contains(other)) return;

        PlayerHealth hp = other.GetComponentInParent<PlayerHealth>();
        if (hp == null || hp.IsDead) return; // <- вот и весь фикс

        hit.Add(other);
        hp.TakeDamage(damage);
    }
}