using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Gắn lên Tornado GameObject — một ParticleSystem child của Player/Visuals.
///
/// Implement ITimedEffect để ItemSlotUI có thể cập nhật Timer Image.
///
/// Phân tách trách nhiệm:
///   - Tornado (visual): MagnetEffect điều khiển activate/deactivate + particle play/stop + scale theo hole
///   - Magnet logic: Mỗi frame tìm all objects swallowable trong radius, AddForce về hole
///
/// Layer "Swallowable" = 9 (theo SwallowHandler convention).
///
/// Scale logic (giống BombShieldEffect):
///   initialHoleRadius = 0.5 → Tornado localScale = 1
///   scaleFactor = 1 / 0.5 = 2
///   Mỗi khi radius thay đổi: Tornado.localScale = radius * 2
///   Tween cùng growDuration để đồng bộ với hole grow animation.
///
/// Khi Initialize():
///   - Bật GameObject (SetActive true)
///   - Play ParticleSystem
///   - Subscribe OnGrown để scale theo hole
///   - Tạo magnet force loop trong FixedUpdate
///
/// Khi Deactivate():
///   - Stop ParticleSystem
///   - Tắt GameObject (SetActive false)
///   - Cleanup tween và event
///   - Fire OnExpired event
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class MagnetEffect : MonoBehaviour, ITimedEffect
{
    private const int SwallowableLayer = 9;

    [Header("Magnet Config (set from Definition SO)")]
    [Tooltip("Bán kính vùng hút (đơn vị Unity).")]
    [SerializeField] private float radius = 10f;

    [Tooltip("Lực hút áp dụng cho từng object/frame.")]
    [SerializeField] private float force = 5f;

    [Header("Scale Config")]
    [Tooltip("Tỉ lệ scale so với radius. Tornado initialScale=1, initialRadius=0.5 → factor=2")]
    [SerializeField] private float scaleFactor = 2f;

    // ── Runtime state ─────────────────────────────────────────────────────────
    private float remaining;
    private float totalDuration;
    private Transform holeTransform; // Center của magnet force
    private ParticleSystem particleSystem;
    private HoleSizeController holeSizeController;
    private bool isInitialized;

    private LayerMask swallowableLayerMask;

    // Reuse buffer để tránh GC alloc mỗi frame
    private Collider[] overlapBuffer = new Collider[128];

    // ── ITimedEffect ──────────────────────────────────────────────────────────
    public float Remaining     => remaining;
    public float TotalDuration => totalDuration;
    public event Action OnExpired;

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>
    /// Kích hoạt magnet effect. Gọi từ MagnetEffectDefinition.ApplyEffect().
    /// </summary>
    public void Initialize(float radius, float force, float duration, Transform holeTransform, HoleSizeController sizeController)
    {
        this.radius            = radius;
        this.force             = force;
        this.remaining         = duration;
        this.totalDuration     = duration;
        this.holeTransform     = holeTransform;
        this.holeSizeController = sizeController;
        this.isInitialized     = true;

        swallowableLayerMask = 1 << SwallowableLayer;

        // Resolve ParticleSystem nếu chưa cache
        if (particleSystem == null)
            particleSystem = GetComponent<ParticleSystem>();

        // Kích hoạt Tornado visual
        gameObject.SetActive(true);

        if (particleSystem != null && !particleSystem.isPlaying)
            particleSystem.Play();

        // Sync scale ngay với radius hiện tại (snap, không tween)
        if (holeSizeController != null)
        {
            ApplyScale(holeSizeController.Radius, snap: true);
            holeSizeController.OnGrown += OnHoleGrown;
        }

        Debug.Log($"[MagnetEffect] Initialized — radius={radius}, force={force}, duration={duration}s.");
    }

    /// <summary>
    /// Extend duration khi dùng lần 2 trong khi còn active.
    /// </summary>
    public void ExtendDuration(float additionalTime)
    {
        remaining += additionalTime;
        Debug.Log($"[MagnetEffect] Extended by {additionalTime}s. Remaining: {remaining:F1}s.");
    }

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        // Cache ParticleSystem reference
        if (particleSystem == null)
            particleSystem = GetComponent<ParticleSystem>();

        if (particleSystem == null)
            Debug.LogWarning("[MagnetEffect] ParticleSystem component không tìm thấy.", this);
    }

    private void FixedUpdate()
    {
        if (!isInitialized || remaining <= 0f) return;

        remaining -= Time.fixedDeltaTime;

        if (remaining <= 0f)
        {
            Deactivate();
            return;
        }

        ApplyMagnetForce();
    }

    private void OnDestroy()
    {
        Debug.Log("[MagnetEffect] Destroyed.");
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private void Deactivate()
    {
        Cleanup();

        isInitialized = false;
        remaining     = 0f;

        // Stop particle
        if (particleSystem != null && particleSystem.isPlaying)
            particleSystem.Stop();

        // Tắt GameObject
        gameObject.SetActive(false);

        // Fire event
        OnExpired?.Invoke();
        OnExpired = null; // Clear để tránh stale listener khi reuse

        Debug.Log("[MagnetEffect] Deactivated.");
    }

    private void Cleanup()
    {
        DOTween.Kill(transform);

        if (holeSizeController != null)
        {
            holeSizeController.OnGrown -= OnHoleGrown;
            holeSizeController = null;
        }
    }

    /// <summary>
    /// Callback từ HoleSizeController.OnGrown.
    /// Tween Tornado scale để đồng bộ với grow animation của hole.
    /// </summary>
    private void OnHoleGrown(float newRadius)
    {
        ApplyScale(newRadius, snap: false);
    }

    /// <summary>
    /// Set localScale của Tornado theo radius.
    /// snap = true: set ngay (khi Initialize).
    /// snap = false: tween với GrowDuration.
    /// </summary>
    private void ApplyScale(float radius, bool snap)
    {
        float   target  = radius * scaleFactor;
        Vector3 targetV = new Vector3(target, target, target);

        if (snap || holeSizeController == null)
        {
            transform.localScale = targetV;
        }
        else
        {
            DOTween.Kill(transform);
            transform.DOScale(targetV, holeSizeController.GrowDuration)
                     .SetEase(Ease.OutQuad);
        }
    }

    private void ApplyMagnetForce()
    {
        if (holeTransform == null)
        {
            Debug.LogWarning("[MagnetEffect] holeTransform is null — cannot apply magnet force.");
            return;
        }

        Vector3 center = holeTransform.position;

        int count = Physics.OverlapSphereNonAlloc(center, radius, overlapBuffer, swallowableLayerMask);

        for (int i = 0; i < count; i++)
        {
            Collider col = overlapBuffer[i];
            if (col == null || !col.gameObject.activeInHierarchy) continue;

            Rigidbody rb = col.attachedRigidbody;
            if (rb == null || rb.isKinematic) continue;

            // WakeUp để đảm bảo Rigidbody đang ngủ vẫn nhận được force
            rb.WakeUp();

            // Hút theo mặt phẳng XZ — không dùng y để tránh triệt tiêu gravity
            Vector3 toCenter    = center - rb.position;
            Vector3 directionXZ = new Vector3(toCenter.x, 0f, toCenter.z).normalized;
            rb.AddForce(directionXZ * force, ForceMode.Acceleration);
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
