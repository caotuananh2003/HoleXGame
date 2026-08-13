using UnityEngine;

/// <summary>
/// Runtime component gắn vào hole GameObject khi BombShield active.
/// Expose static method IsActive() để SwallowHandler check trước khi trigger bomb game over.
/// Tự destroy sau duration.
///
/// Pattern: Singleton-like static accessor — chỉ có 1 instance active tại một thời điểm trên hole.
/// </summary>
public class BombShieldEffect : MonoBehaviour
{
    private static BombShieldEffect activeInstance;

    private float remaining;

    /// <summary>
    /// Check shield có đang active không.
    /// Gọi từ SwallowHandler khi swallow bomb.
    /// </summary>
    public static bool IsActive => activeInstance != null && activeInstance.remaining > 0f;

    private void Awake()
    {
        // Guard: clear stale reference từ scene trước nếu static vẫn trỏ vào
        // instance đã bị destroy nhưng chưa qua OnDestroy (ví dụ: scene reload đột ngột).
        if (activeInstance != null && activeInstance != this)
        {
            Destroy(activeInstance);
        }
    }

    public void Initialize(float duration)
    {
        remaining = duration;
        activeInstance = this;

        Debug.Log($"[BombShieldEffect] Initialized — duration={duration}s.");
    }

    /// <summary>
    /// Extend duration khi dùng shield item lần 2 trong khi effect còn active.
    /// </summary>
    public void ExtendDuration(float additionalTime)
    {
        remaining += additionalTime;
        Debug.Log($"[BombShieldEffect] Duration extended by {additionalTime}s. New remaining: {remaining:F1}s.");
    }

    private void Update()
    {
        remaining -= Time.deltaTime;

        if (remaining <= 0f)
        {
            Debug.Log("[BombShieldEffect] Duration expired — destroying component.");
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (activeInstance == this)
            activeInstance = null;

        Debug.Log("[BombShieldEffect] Destroyed.");
    }

#if UNITY_EDITOR
    // Visualize shield active trong Scene view
    private void OnDrawGizmos()
    {
        if (remaining > 0f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 2f);
        }
    }
#endif
}
