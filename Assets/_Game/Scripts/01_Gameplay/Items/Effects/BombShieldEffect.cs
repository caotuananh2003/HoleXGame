using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Gắn lên ShieldVisual — một GameObject wrapper rỗng, child của Player/Visuals.
/// Shield01_Blue (prefab gốc) là child của ShieldVisual.
///
/// Implement ITimedEffect để ItemSlotUI có thể cập nhật Timer Image mà không
/// cần biết đây là BombShieldEffect hay effect nào khác.
///
/// Phân tách trách nhiệm:
///   - ShieldVisual (wrapper): BombShieldEffect điều khiển scale theo radius + fade out
///   - Shield01_Blue (child):  Animation component tự chạy ShieldGrowing01 (spawn bounce)
///
/// Scale logic:
///   initialHoleRadius = 0.5 → wrapper localScale = 0.55
///   scaleFactor = 0.55 / 0.5 = 1.1
///   Mỗi khi radius thay đổi: wrapper.localScale = radius * 1.1
///   Tween cùng growDuration để đồng bộ với hole grow animation.
///
/// Fade out: điều chỉnh alpha của FrontColor_ và BackColor_ trên material
///   (ShieldShader không có float property alpha riêng biệt)
///   remaining > fadeOutDuration → alpha = 1
///   remaining <= fadeOutDuration → alpha lerp 1 → 0
/// </summary>
public class BombShieldEffect : MonoBehaviour, ITimedEffect
{
    [Header("Scale Config")]
    [Tooltip("Tỉ lệ scale so với radius. initialRadius=0.5, targetScale=0.55 → factor=1.1")]
    [SerializeField] private float scaleFactor = 1.1f;

    [Header("Fade Out")]
    [Tooltip("Số giây cuối bắt đầu fade alpha về 0.")]
    [SerializeField] private float fadeOutDuration = 3f;

    [Header("Shield Child Reference")]
    [Tooltip("Shield01_Blue — child có MeshRenderer. Để trống sẽ tự GetComponentInChildren.")]
    [SerializeField] private Renderer shieldRenderer;

    // ── Static accessor ───────────────────────────────────────────────────────
    private static BombShieldEffect activeInstance;

    /// <summary>SwallowHandler check khi swallow bomb.</summary>
    public static bool IsActive => activeInstance != null && activeInstance.remaining > 0f;

    // ── ITimedEffect ──────────────────────────────────────────────────────────
    public float Remaining     => remaining;
    public float TotalDuration => totalDuration;
    public event System.Action OnExpired;

    // ── Runtime state ─────────────────────────────────────────────────────────
    private float              remaining;
    private float              totalDuration;
    private Material           materialInstance;
    private HoleSizeController holeSizeController;
    private bool               isInitialized;

    // Cache màu gốc để restore khi deactivate
    private Color originalFrontColor;
    private Color originalBackColor;

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>
    /// Kích hoạt shield. Gọi từ BombShieldEffectDefinition.ApplyEffect().
    /// </summary>
    public void Initialize(float duration, HoleSizeController sizeController)
    {
        remaining          = duration;
        totalDuration      = duration;
        holeSizeController = sizeController;
        activeInstance     = this;
        isInitialized      = true;

        // Resolve renderer từ child nếu chưa gán tay
        if (shieldRenderer == null)
            shieldRenderer = GetComponentInChildren<Renderer>(true);

        // Tạo material instance riêng (không ảnh hưởng shared material)
        if (shieldRenderer != null)
        {
            materialInstance   = shieldRenderer.material;
            originalFrontColor = materialInstance.GetColor("FrontColor_");
            originalBackColor  = materialInstance.GetColor("BackColor_");
        }
        else
        {
            Debug.LogWarning("[BombShieldEffect] Renderer không tìm thấy — fade out sẽ không hoạt động.");
        }

        // Reset alpha về 1
        SetAlpha(1f);

        // Bật visual
        gameObject.SetActive(true);

        // Sync scale ngay với radius hiện tại (snap, không tween)
        if (holeSizeController != null)
        {
            ApplyScale(holeSizeController.Radius, snap: true);
            holeSizeController.OnGrown += OnHoleGrown;
        }

        Debug.Log($"[BombShieldEffect] Initialized — duration={duration}s.");
    }

    /// <summary>
    /// Extend duration khi dùng lần 2 trong khi còn active.
    /// </summary>
    public void ExtendDuration(float additionalTime)
    {
        remaining += additionalTime;
        Debug.Log($"[BombShieldEffect] Extended by {additionalTime}s. Remaining: {remaining:F1}s.");
    }

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        // Guard: clear stale static ref từ scene reload trước
        if (activeInstance != null && activeInstance != this)
            activeInstance.Deactivate();
    }

    private void Update()
    {
        if (!isInitialized || remaining <= 0f) return;

        remaining -= Time.deltaTime;
        UpdateFadeOut();

        if (remaining <= 0f)
            Deactivate();
    }

    private void OnDestroy()
    {
        Cleanup();
        if (activeInstance == this)
            activeInstance = null;
    }

    // =========================================================================
    // Internal
    // =========================================================================

    /// <summary>
    /// Callback từ HoleSizeController.OnGrown.
    /// Tween wrapper scale để đồng bộ với grow animation của hole.
    /// </summary>
    private void OnHoleGrown(float newRadius)
    {
        ApplyScale(newRadius, snap: false);
    }

    /// <summary>
    /// Set localScale của wrapper theo radius.
    /// snap = true: set ngay (khi Initialize, không override animation).
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

    /// <summary>
    /// Fade alpha của FrontColor_ và BackColor_ về 0 trong fadeOutDuration giây cuối.
    /// ShieldShader không có property alpha float riêng — alpha được bake vào color.
    /// </summary>
    private void UpdateFadeOut()
    {
        if (materialInstance == null) return;

        float alpha = remaining <= fadeOutDuration
            ? Mathf.Clamp01(remaining / fadeOutDuration)
            : 1f;

        SetAlpha(alpha);
    }

    private void SetAlpha(float alpha)
    {
        if (materialInstance == null) return;

        Color front   = originalFrontColor;
        Color back    = originalBackColor;
        front.a       = alpha;
        back.a        = alpha;
        materialInstance.SetColor("FrontColor_", front);
        materialInstance.SetColor("BackColor_",  back);
    }

    private void Deactivate()
    {
        Cleanup();

        // Restore alpha trước khi tắt để lần sau dùng lại đúng
        SetAlpha(1f);

        gameObject.SetActive(false);

        if (activeInstance == this)
            activeInstance = null;

        isInitialized = false;
        remaining     = 0f;

        OnExpired?.Invoke();
        OnExpired = null; // Clear để tránh stale listener khi reuse

        Debug.Log("[BombShieldEffect] Deactivated.");
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

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (remaining > 0f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, transform.lossyScale.x * 0.5f);
        }
    }
#endif
}
