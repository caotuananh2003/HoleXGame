using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object pool cho FloatingScoreText trên Screen Space Overlay Canvas.
/// Gắn vào một GameObject trong GameplayScene.
/// </summary>
public class FloatingScorePool : MonoBehaviour
{
    [SerializeField] private FloatingScoreText prefab;
    [SerializeField] private int               initialPoolSize = 10;

    [Header("Animation Config")]
    [SerializeField] private float riseHeight = 150f; // pixels trên canvas
    [SerializeField] private float duration   = 1.2f;

    [Header("Spawn Offset")]
    [Tooltip("Hệ số nhân với radius truyền vào để tạo vùng spawn ngẫu nhiên. 1 = đúng bằng radius player.")]
    [SerializeField] private float spawnRadiusMultiplier = 1f;

    [Header("References")]
    [SerializeField] private Canvas canvas;   // Screen Space Overlay Canvas
    [SerializeField] private Camera mainCamera;

    private readonly Queue<FloatingScoreText> pool = new();

    private void Awake()
    {
        if (prefab == null)
        {
            Debug.LogWarning("[FloatingScorePool] prefab is not assigned.");
            return;
        }

        if (canvas == null)
            Debug.LogWarning("[FloatingScorePool] canvas is not assigned.");

        if (mainCamera == null)
            mainCamera = Camera.main;

        // Pre-warm pool
        for (int i = 0; i < initialPoolSize; i++)
            pool.Enqueue(CreateInstance());
    }

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>Spawn floating text tại vị trí ngẫu nhiên quanh playerPosition (world space).</summary>
    public void Spawn(int score, Vector3 playerPosition, float playerRadius)
    {
        if (prefab == null || canvas == null) return;

        // Random offset trên mặt phẳng XZ quanh player
        Vector2 randomCircle = Random.insideUnitCircle * (playerRadius * spawnRadiusMultiplier);
        Vector3 spawnWorld   = new Vector3(
            playerPosition.x + randomCircle.x,
            playerPosition.y,
            playerPosition.z + randomCircle.y);

        FloatingScoreText item = pool.Count > 0 ? pool.Dequeue() : CreateInstance();
        item.Play(score, spawnWorld, canvas, mainCamera, ReturnToPool, riseHeight, duration);
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private FloatingScoreText CreateInstance()
    {
        // Instantiate trực tiếp vào Canvas để RectTransform hoạt động đúng
        FloatingScoreText instance = Instantiate(prefab, canvas.transform);
        instance.gameObject.SetActive(false);
        return instance;
    }

    private void ReturnToPool(FloatingScoreText item)
    {
        pool.Enqueue(item);
    }
}
