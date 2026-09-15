using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Quản lý kích thước hole bằng cách tăng transform.localScale của Player.
///
/// Khi scale Player tăng, toàn bộ child (HoleSkin, HoleFill, arrow, Shield, Tornado,
/// SphereCollider) tự động scale theo — không cần set thủ công từng thứ.
///
/// HoleColliderController vẫn được notify qua OnGrown vì hole2DCollider là object
/// riêng trong scene 2D, không phải child của Player.
///
/// GrowHole(): tween transform.localScale += Vector3.one, fire OnGrown khi done.
/// Reset(): đưa scale về initialScale.
/// </summary>
public class HoleSizeController : MonoBehaviour
{
    [Header("Collider")]
    [Tooltip("HoleColliderController — object riêng trong scene, cần notify khi grow.")]
    [SerializeField] private HoleColliderController holeColliderController;

    [Header("Grow Animation")]
    [SerializeField] private float growDuration = 0.5f;
    [SerializeField] private Ease  growEase     = Ease.OutBack;

    [Header("Initial Scale")]
    [Tooltip("Scale ban đầu của Player. Reset() sẽ đưa về giá trị này.")]
    [SerializeField] private Vector3 initialScale = Vector3.one;

    // ── Runtime state ──────────────────────────────────────────────────────────
    public float GrowDuration => growDuration;

    private bool isGrowing;

    // ── Events ─────────────────────────────────────────────────────────────────
    /// <summary>Fire sau khi grow animation hoàn tất.</summary>
    public event Action OnGrown;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        if (holeColliderController == null)
            Debug.LogWarning("[HoleSizeController] holeColliderController is null — assign in Inspector.");
    }

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>
    /// Tăng hole 1 bậc: tween localScale += Vector3.one.
    /// Gọi từ HoleController khi đủ điểm milestone hoặc dùng item.
    /// </summary>
    public void GrowHole()
    {
        if (isGrowing) return;

        isGrowing = true;

        Vector3 targetScale = transform.localScale + Vector3.one * 0.4f;

        transform.DOScale(targetScale, growDuration)
            .SetEase(growEase)
            .OnComplete(() =>
            {
                isGrowing = false;
                holeColliderController?.SetRadius(transform.localScale.x);
                OnGrown?.Invoke();
            });

        Debug.Log($"[HoleSizeController] GrowHole → scale {targetScale}");
    }

    /// <summary>
    /// Đưa Player về initialScale. Gọi từ HoleController.ResetToInitial().
    /// </summary>
    public void Reset()
    {
        DOTween.Kill(transform);
        isGrowing = false;

        transform.localScale = initialScale;
        holeColliderController?.SetRadius(initialScale.x);

        Debug.Log("[HoleSizeController] Reset to initial scale.");
    }

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void OnDestroy()
    {
        DOTween.Kill(transform);
    }
}
