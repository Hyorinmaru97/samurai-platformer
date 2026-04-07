using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class HitboxVisualizer : MonoBehaviour
{
    public Color color = new Color(1f, 0f, 0f, 0.35f);
    public bool showInGame = true;

    BoxCollider2D col;
    GameObject visual;

    void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        CreateVisual();
    }

    void CreateVisual()
    {
        visual = new GameObject("_HitboxVisual");
        visual.transform.SetParent(transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = CreateRectSprite();
        sr.color = color;
        sr.sortingOrder = 999;
        visual.SetActive(false);
    }

    void Update()
    {
        if (visual == null) return;

        // подгоняем размер под коллайдер
        visual.transform.localPosition = (Vector3)col.offset;
        visual.transform.localScale = new Vector3(col.size.x, col.size.y, 1f);

        var sr = visual.GetComponent<SpriteRenderer>();
        if (sr) sr.color = color;
    }

    Sprite CreateRectSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}