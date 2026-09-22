using System;
using UnityEngine;

/// <summary>
/// Gắn lên MagnetParent — parent của MagnetParticle child object.
/// MagnetParent luôn active. Chỉ MagnetParticle child bị bật/tắt theo effect.
///
/// MagnetParticle có ParticleSystem root và 5 child ParticleSystem con.
/// Khi Initialize(), duration của tất cả ParticleSystem (root + children)
/// được đồng bộ với duration từ MagnetEffectDefinition.
///
/// radius và force do MagnetEffectDefinition truyền vào — không config tại đây.
/// </summary>
public class MagnetEffect : MonoBehaviour, ITimedEffect
{
    [Header("References")]
    [Tooltip("MagnetParticle child object — kéo từ Hierarchy vào.")]
    [SerializeField] private GameObject magnetParticle;

    [Tooltip("Các layer mà magnet sẽ hút. Chọn nhiều layer trong dropdown.")]
    [SerializeField] private LayerMask attractLayers;

    // ── ITimedEffect ──────────────────────────────────────────────────────────

    public float Remaining
    {
        get { return remaining; }
    }

    public float TotalDuration
    {
        get { return totalDuration; }
    }

    public event Action OnExpired;

    // ── Runtime state ─────────────────────────────────────────────────────────

    private float      remaining;
    private float      totalDuration;
    private float      radius;
    private float      force;
    private Transform  holeTransform;
    private bool       isInitialized;

    // ParticleSystem root trên MagnetParticle
    private ParticleSystem rootParticle;

    // Buffer tái sử dụng để tránh GC alloc mỗi frame
    private Collider[] overlapBuffer = new Collider[128];

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        if (magnetParticle == null)
        {
            Debug.LogWarning("[MagnetEffect] magnetParticle chưa được gán.");
            return;
        }

        magnetParticle.SetActive(false);

        rootParticle = magnetParticle.GetComponent<ParticleSystem>();

        if (rootParticle == null)
        {
            Debug.LogWarning("[MagnetEffect] Không tìm thấy ParticleSystem trên MagnetParticle.");
        }
    }

    private void FixedUpdate()
    {
        if (isInitialized == false) return;
        if (remaining <= 0f) return;

        remaining -= Time.fixedDeltaTime;

        if (remaining <= 0f)
        {
            Deactivate();
            return;
        }

        ApplyMagnetForce();
    }

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>Kích hoạt magnet effect. Gọi từ MagnetEffectDefinition.ApplyEffect().</summary>
    public void Initialize(float radius, float force, float duration, Transform holeTransform)
    {
        this.radius        = radius;
        this.force         = force;
        this.remaining     = duration;
        this.totalDuration = duration;
        this.holeTransform = holeTransform;
        this.isInitialized = true;

        // Đồng bộ duration cho root particle và tất cả child particle
        if (magnetParticle != null)
        {
            SetDurationForAll(magnetParticle, duration);
            magnetParticle.SetActive(true);
        }

        if (rootParticle != null && rootParticle.isPlaying == false)
        {
            rootParticle.Play();
        }

        Debug.Log("[MagnetEffect] Initialized — radius=" + radius + ", force=" + force + ", duration=" + duration + "s.");
    }

    /// <summary>Extend duration khi dùng lần 2 trong khi còn active.</summary>
    public void ExtendDuration(float additionalTime)
    {
        remaining += additionalTime;
        Debug.Log("[MagnetEffect] Extended by " + additionalTime + "s. Remaining: " + remaining.ToString("F1") + "s.");
    }

    // =========================================================================
    // Internal
    // =========================================================================

    /// <summary>
    /// Set duration cho ParticleSystem trên root và tất cả child của target.
    /// </summary>
    private void SetDurationForAll(GameObject target, float duration)
    {
        ParticleSystem[] allParticles = target.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in allParticles)
        {
            ParticleSystem.MainModule main = ps.main;
            main.duration = duration;
        }
    }

    private void Deactivate()
    {
        isInitialized = false;
        remaining     = 0f;

        // Không cần Stop() hay SetActive(false) thủ công —
        // ParticleSystem đã cài StopAction = Disable trong Inspector,
        // object tự disable khi particle chạy xong.

        if (OnExpired != null)
        {
            OnExpired.Invoke();
            OnExpired = null;
        }

        Debug.Log("[MagnetEffect] Deactivated.");
    }

    /// <summary>
    /// Tắt ngay lập tức — dùng khi Cleanup (về MainMenu, restart).
    /// Dừng particle và tắt object ngay không chờ StopAction.
    /// </summary>
    public void ForceDeactivate()
    {
        if (!isInitialized) return;

        isInitialized = false;
        remaining     = 0f;

        if (rootParticle != null && rootParticle.isPlaying)
        {
            rootParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (magnetParticle != null)
        {
            magnetParticle.SetActive(false);
        }

        if (OnExpired != null)
        {
            OnExpired.Invoke();
            OnExpired = null;
        }

        Debug.Log("[MagnetEffect] ForceDeactivated.");
    }

    private void ApplyMagnetForce()
    {
        if (holeTransform == null)
        {
            Debug.LogWarning("[MagnetEffect] holeTransform is null.");
            return;
        }

        Vector3 center    = holeTransform.position;
        float   holeRadius = holeTransform.localScale.x * 0.5f;

        int count = Physics.OverlapSphereNonAlloc(center, radius, overlapBuffer, attractLayers);

        for (int i = 0; i < count; i++)
        {
            Collider col = overlapBuffer[i];
            if (col == null || col.gameObject.activeInHierarchy == false) continue;

            Rigidbody rb = col.attachedRigidbody;
            if (rb == null || rb.isKinematic) continue;

            // Không hút bomb
            Obstacle obstacle = col.GetComponentInParent<Obstacle>();
            if (obstacle != null && obstacle.ObstacleDefinition != null
                && obstacle.ObstacleDefinition.Type == ObstacleType.Bomb) continue;

            // Đã vào phạm vi hole — dừng hút, để obstacle tự rơi xuống
            Vector3 current = rb.position;
            float   distXZ  = new Vector2(current.x - center.x, current.z - center.z).magnitude;
            if (distXZ <= holeRadius) continue;

            // Di chuyển về tâm hole trên mặt phẳng XZ, giữ nguyên Y
            Vector3 targetXZ = new Vector3(center.x, current.y, center.z);
            Vector3 newPos   = Vector3.MoveTowards(current, targetXZ, force * Time.fixedDeltaTime);

            rb.MovePosition(newPos);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
