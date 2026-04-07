using UnityEngine;

public class FaceFlip2D : MonoBehaviour
{
    [Header("Refs")]
    public Transform face;

    [Header("Tuning")]
    public bool invert = false;          // если смотрит наоборот — включи
    public float deadZone = 0.01f;       // игнор мелких значений

    float baseScaleX;
    int facing = 1; // 1 = вправо, -1 = влево

    void Awake()
    {
        if (!face) return;
        baseScaleX = Mathf.Abs(face.localScale.x);
        if (baseScaleX < 0.0001f) baseScaleX = 1f;
    }

    void Update()
    {
        if (!face) return;

        float x = Input.GetAxisRaw("Horizontal"); // A/D
        if (Mathf.Abs(x) <= deadZone) return;

        facing = x > 0 ? 1 : -1;
        int dir = invert ? -facing : facing;

        var s = face.localScale;
        s.x = baseScaleX * dir;
        face.localScale = s;
    }
}
