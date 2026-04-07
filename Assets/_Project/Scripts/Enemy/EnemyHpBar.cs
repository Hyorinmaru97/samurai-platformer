using UnityEngine;

public class EnemyHpBar : MonoBehaviour
{
    public SpriteRenderer fill;
    public SpriteRenderer bg;  // ← добавь ссылку на Bg

    float fullScaleX;
    Vector3 startLocalPos;

    void Start()
    {
        if (!fill || !bg) return;
        // берём размер прямо с Bg в момент старта
        fullScaleX = bg.transform.localScale.x;
        fill.transform.localScale = new Vector3(
            fullScaleX, 
            fill.transform.localScale.y, 
            1f);
        startLocalPos = fill.transform.localPosition;
        SetPercent(1f);
    }

    public void SetPercent(float p)
    {
        if (!fill) return;
        p = Mathf.Clamp01(p);

        float newScaleX = fullScaleX * p;
        var t = fill.transform;
        t.localScale = new Vector3(newScaleX, t.localScale.y, 1f);
        t.localPosition = new Vector3(
            startLocalPos.x - (fullScaleX - newScaleX) * 0.5f,
            startLocalPos.y,
            startLocalPos.z
        );
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}