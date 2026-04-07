using UnityEngine;

public class IdleBreath2D : MonoBehaviour
{
    [Header("Breath")]
    public float rotAmplitude = 1.5f;   // градусы
    public float posAmplitude = 0.01f;  // юниты
    public float speed = 2.0f;

    Quaternion startRot;
    Vector3 startPos;

    void Awake()
    {
        startRot = transform.localRotation;
        startPos = transform.localPosition;
    }

    void LateUpdate()
    {
        float t = Mathf.Sin(Time.time * speed);
        transform.localRotation = startRot * Quaternion.Euler(0f, 0f, t * rotAmplitude);
        transform.localPosition = startPos + new Vector3(0f, t * posAmplitude, 0f);
    }
}
