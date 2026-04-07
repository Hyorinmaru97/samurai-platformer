using UnityEngine;
using UnityEngine.UI;

// Повесь на объект Fill внутри HPBar
public class HPBarPulse : MonoBehaviour
{
    [Header("Цвета пульсации")]
    public Color colorA = new Color(0.8f, 0.05f, 0.05f, 1f); // тёмно-красный
    public Color colorB = new Color(1f, 0.2f, 0.1f, 1f);     // ярко-красный

    [Header("Скорость и интенсивность")]
    public float pulseSpeed = 2f;      // скорость пульсации
    public float waveSpeed = 3f;       // скорость волны слева направо
    public float waveIntensity = 0.15f; // интенсивность волны

    Image image;
    float offset;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        if (image == null) return;

        offset += Time.deltaTime * waveSpeed;

        // Базовая пульсация
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        // Волна поверх пульсации
        float wave = (Mathf.Sin(Time.time * waveSpeed + offset) + 1f) * 0.5f * waveIntensity;

        image.color = Color.Lerp(colorA, colorB, pulse + wave);
    }
}
