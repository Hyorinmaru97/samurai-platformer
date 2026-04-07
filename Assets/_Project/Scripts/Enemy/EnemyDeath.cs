using UnityEngine;
using System.Collections;

public class EnemyDeath : MonoBehaviour
{
    [Header("Осколки")]
    public int pieceCount = 6;
    public float pieceSize = 0.2f;
    public float explodeForce = 4f;
    public float lifetime = 0.8f;

    public void PlayDeath()
    {
        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        // скрываем оригинал
        var sr = GetComponent<SpriteRenderer>();
        var col = GetComponent<Collider2D>();
        var rb = GetComponent<Rigidbody2D>();

        //if (sr) sr.enabled = false;
        if (col) col.enabled = false;
        if (rb) rb.simulated = false;

        // спавним осколки
        for (int i = 0; i < pieceCount; i++)
        {
            var piece = new GameObject("Piece");
            piece.transform.position = transform.position + 
                new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0f);

            var psr = piece.AddComponent<SpriteRenderer>();
            psr.sprite = CreateSquareSprite();
            psr.color = sr ? sr.color : Color.white;
            psr.sortingOrder = 10;

            float s = Random.Range(pieceSize * 0.5f, pieceSize * 1.5f);
            piece.transform.localScale = new Vector3(s, s, 1f);

            var prb = piece.AddComponent<Rigidbody2D>();
            prb.gravityScale = 2f;

            Vector2 dir = new Vector2(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1f)
            ).normalized;
            prb.AddForce(dir * explodeForce, ForceMode2D.Impulse);
            prb.AddTorque(Random.Range(-5f, 5f));

            Destroy(piece, lifetime);
        }

        // пыль
        SpawnDeathDust();

        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }

    void SpawnDeathDust()
    {
        for (int i = 0; i < 8; i++)
        {
            var dust = new GameObject("DeathDust");
            dust.transform.position = transform.position +
                new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(-0.2f, 0.2f), 0f);

            var psr = dust.AddComponent<SpriteRenderer>();
            psr.sprite = CreateSquareSprite();
            psr.color = new Color(0.8f, 0.8f, 0.8f, 0.7f);
            psr.sortingOrder = 9;

            float s = Random.Range(0.05f, 0.15f);
            dust.transform.localScale = new Vector3(s, s, 1f);

            var prb = dust.AddComponent<Rigidbody2D>();
            prb.gravityScale = 0.3f;

            Vector2 dir = new Vector2(
                Random.Range(-1f, 1f),
                Random.Range(0.3f, 1f)
            ).normalized;
            prb.AddForce(dir * Random.Range(1f, 3f), ForceMode2D.Impulse);

            Destroy(dust, 0.6f);
        }
    }

    Sprite CreateSquareSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), 
            new Vector2(0.5f, 0.5f), 1f);
    }
}