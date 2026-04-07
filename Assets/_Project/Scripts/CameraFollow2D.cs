using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    public float smoothTime = 0.15f;

    // Ограничения камеры (границы уровня)
    public bool useBounds = false;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    Vector3 velocity;

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desired = target.position + offset;
        Vector3 smoothed = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);

        if (useBounds)
        {
            smoothed.x = Mathf.Clamp(smoothed.x, minBounds.x, maxBounds.x);
            smoothed.y = Mathf.Clamp(smoothed.y, minBounds.y, maxBounds.y);
        }

        transform.position = smoothed;
    }
}
