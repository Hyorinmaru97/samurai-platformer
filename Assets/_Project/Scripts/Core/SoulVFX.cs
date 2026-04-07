using UnityEngine;
using System.Collections;

public class SoulVFX : MonoBehaviour
{
    [Header("Animation")]
    public float arcHeight = 1.5f;
    public float duration = 0.6f;
    public float fadeDelay = 0.4f;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        if (sr != null)
        {
            var c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
        StopAllCoroutines();
        StartCoroutine(PlayVFX());
    }

    IEnumerator PlayVFX()
    {
        Vector3 start = transform.position;
        Vector3 end = start + new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(0.5f, 1.2f),
            0f
        );

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float height = arcHeight * 4f * t * (1f - t);
            Vector3 pos = Vector3.Lerp(start, end, t);
            pos.y += height;
            transform.position = pos;

            if (elapsed > fadeDelay && sr != null)
            {
                float fadeDuration = Mathf.Max(0.0001f, duration - fadeDelay);
                float fadeT = (elapsed - fadeDelay) / fadeDuration;
                var c = sr.color;
                c.a = 1f - fadeT;
                sr.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}