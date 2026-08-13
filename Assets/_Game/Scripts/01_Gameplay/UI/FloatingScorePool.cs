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

    /// <summary>Spawn floating text tại worldPosition (world space của obstacle).</summary>
    public void Spawn(int score, Vector3 worldPosition)
    {
        if (prefab == null || canvas == null) return;

        FloatingScoreText item = pool.Count > 0 ? pool.Dequeue() : CreateInstance();
        item.Play(score, worldPosition, canvas, mainCamera, ReturnToPool, riseHeight, duration);
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
