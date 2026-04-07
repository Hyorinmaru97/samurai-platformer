using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Parallax")]
    [SerializeField] private float parallaxFactor = 0.5f;

    [Header("Auto Scroll")]
    [SerializeField] private float idleScrollSpeed = 0.1f;
    [SerializeField] private float moveSpeedBonus = 0.15f;

    [Header("References")]
    [SerializeField] private Rigidbody2D playerRb;

    private Camera cam;
    private SpriteRenderer sourceRenderer;
    private Transform[] segments;

    private float spriteWidth;
    private float baseX;
    private float startY;
    private float startZ;
    private float autoScrollOffset;
    private float lastCamX;

    void Start()
    {
        cam = Camera.main;
        sourceRenderer = GetComponent<SpriteRenderer>();

        if (cam == null || sourceRenderer == null || sourceRenderer.sprite == null)
        {
            Debug.LogError("ParallaxBackground: missing camera or sprite.");
            enabled = false;
            return;
        }

        spriteWidth = sourceRenderer.bounds.size.x;
        baseX  = transform.position.x;
        startY = transform.position.y;
        startZ = transform.position.z;
        lastCamX = cam.transform.position.x;

        segments = new Transform[3];
        segments[0] = transform;

        for (int i = 1; i < 3; i++)
        {
            GameObject copy = new GameObject(gameObject.name + "_seg" + i);
            copy.transform.parent = transform.parent;

            SpriteRenderer sr = copy.AddComponent<SpriteRenderer>();
            sr.sprite        = sourceRenderer.sprite;
            sr.sortingLayerID = sourceRenderer.sortingLayerID;
            sr.sortingOrder  = sourceRenderer.sortingOrder;
            sr.sharedMaterial = sourceRenderer.sharedMaterial;
            sr.color         = sourceRenderer.color;
            copy.transform.localScale = transform.localScale;
            segments[i] = copy.transform;
        }

        segments[0].position = new Vector3(baseX,               startY, startZ);
        segments[1].position = new Vector3(baseX - spriteWidth, startY, startZ);
        segments[2].position = new Vector3(baseX + spriteWidth, startY, startZ);
    }

    void LateUpdate()
    {
        float camX = cam.transform.position.x;
        float camDelta = camX - lastCamX;

        float playerSpeedX = 0f;
        if (playerRb != null)
            playerSpeedX = Mathf.Abs(playerRb.linearVelocity.x);

        float playerDirX = playerRb != null ? Mathf.Sign(playerRb.linearVelocity.x) : 1f;
        float currentScrollSpeed = idleScrollSpeed + playerSpeedX * moveSpeedBonus;
        autoScrollOffset += currentScrollSpeed * playerDirX * Time.deltaTime;

        // Обновляем базу через дельту камеры — стабильный параллакс
        baseX += camDelta * parallaxFactor;

        // Финальный сдвиг
        float parallaxDelta = baseX + autoScrollOffset;

        // Позиционируем сегменты
        for (int i = 0; i < segments.Length; i++)
        {
            float logicalOffset = Mathf.Round(
                (segments[i].position.x - parallaxDelta) / spriteWidth
            ) * spriteWidth;

            segments[i].position = new Vector3(
                parallaxDelta + logicalOffset,
                startY,
                startZ
            );
        }

        // Переработка сегментов
        float viewHalfWidth  = cam.orthographicSize * cam.aspect;
        float recycleThreshold = spriteWidth + viewHalfWidth;

        for (int i = 0; i < segments.Length; i++)
        {
            float dx = segments[i].position.x - camX;
            if (dx < -recycleThreshold)
                segments[i].position += new Vector3(spriteWidth * 3f, 0f, 0f);
            else if (dx > recycleThreshold)
                segments[i].position -= new Vector3(spriteWidth * 3f, 0f, 0f);
        }

        lastCamX = camX;
    }
}
