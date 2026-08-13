using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Quản lý kích thước hole.
///
/// Khởi tạo:
///   - CapsuleCollider (Visual): radius = initialHoleRadius (0.5)
///   - LimitedCollider (Player): radius = initialHoleRadius, center = (0, 5, 0)
///   - FakeGround BoxColliders:  center đẩy ra = initialHoleRadius
///   - HoleSkin:                 localScale = Vector3.one / 4 — luôn hiển thị
///   - DirectionArrow:           localScale = Vector3.one — ẩn/hiện theo input
///
/// Mỗi lần GrowHole (animate):
///   - CapsuleCollider.radius      += 0.5
///   - LimitedCollider.radius      += 0.5  (luôn bằng CapsuleCollider)
///   - FakeGround BoxCollider center += 0.5
///   - HoleSkin.localScale         += Vector3.one
///   - DirectionArrow.localScale   += Vector3.one
///   - Camera.position.y           += 1
///   - Camera.position.z           -= 1
///
/// Nhận config qua Init() từ HoleController — không có SerializeField.
/// </summary>
public class HoleSizeController : MonoBehaviour
{
    // ── Hằng số bước tăng ─────────────────────────────────────────────────────
    private const float ColliderGrowStep = 0.5f; // radius += 0.5 mỗi lần grow
    private const float VisualGrowStep   = 1f;   // scale  += 1   mỗi lần grow

    // ── Config — nhận qua Init() ───────────────────────────────────────────────
    private CapsuleCollider holeCollider;
    private CapsuleCollider limitedCollider;
    private Transform       holeSkin;
    private Transform       directionArrow;
    private Transform       fakeGround;
    private float           initialHoleRadius;
    private float           colliderHalfSize;
    private float           growDuration;
    private Ease            growEase;

    // ── Runtime state ──────────────────────────────────────────────────────────
    public float Radius { get; private set; }

    private bool isGrowing;
    private bool isInitialized;

    // ── Events ─────────────────────────────────────────────────────────────────
    public event Action<int>   OnScoreAdded;
    public event Action<float> OnGrown;

    // ── Internal refs ──────────────────────────────────────────────────────────
    private BoxCollider[]  fakeGroundColliders;
    private SwallowHandler swallowHandler;

    public void Init(
        CapsuleCollider holeCollider,
        CapsuleCollider limitedCollider,
        Transform       holeSkin,
        Transform       directionArrow,
        Transform       fakeGround,
        float           initialHoleRadius,
        float           colliderHalfSize,
        float           growDuration,
        Ease            growEase)
    {
        this.holeCollider      = holeCollider;
        this.limitedCollider   = limitedCollider;
        this.holeSkin          = holeSkin;
        this.directionArrow    = directionArrow;
        this.fakeGround        = fakeGround;
        this.initialHoleRadius = initialHoleRadius;
        this.colliderHalfSize  = colliderHalfSize;
        this.growDuration      = growDuration;
        this.growEase          = growEase;

        isInitialized = true;

        ValidateRefs();
        CollectFakeGroundColliders();
        SubscribeSwallowHandler();

        // ── Trạng thái ban đầu ─────────────────────────────────────────────────
        Radius = initialHoleRadius;

        holeCollider.radius = Radius;
        ApplyFakeGroundRadius(Radius);

        // LimitedCollider: center cố định (0, 5, 0), radius = initialHoleRadius
        if (limitedCollider != null)
        {
            limitedCollider.center = new Vector3(0f, 5f, 0f);
            limitedCollider.radius = Radius;
        }

        // HoleSkin và DirectionArrow bắt đầu scale = (0.25, 0.25, 0.25) và (1, 1, 1)
        if (holeSkin       != null) holeSkin.localScale       = Vector3.one / 4;
        if (directionArrow != null) directionArrow.localScale = Vector3.one;

        OnGrown?.Invoke(Radius);
    }

    /// <summary>Tăng hole lên 1 bậc. Gọi từ HoleController khi đủ điểm.</summary>
    public void GrowHole()
    {
        if (!isInitialized) return;
        if (isGrowing)      return;

        float targetRadius = Radius + ColliderGrowStep;
        float prevRadius   = Radius;
        Radius             = targetRadius;

        OnGrown?.Invoke(Radius);
        AnimateGrow(prevRadius, targetRadius);

        Debug.Log($"[HoleSizeController] GrowHole: Radius={Radius:F2}");
    }

    private void AnimateGrow(float prevRadius, float targetRadius)
    {
        isGrowing = true;

        int tweenDone  = 0;
        int tweenTotal = 0;

        void Done()
        {
            tweenDone++;
            if (tweenDone >= tweenTotal)
                isGrowing = false;
        }

        // ── CapsuleCollider radius ──────────────────────────────────────────────
        if (holeCollider != null)
        {
            tweenTotal++;
            DOTween.To(
                    () => holeCollider.radius,
                    r  => holeCollider.radius = r,
                    targetRadius,
                    growDuration)
                .SetEase(growEase)
                .OnComplete(Done);
        }

        // ── LimitedCollider radius — luôn bằng holeCollider ────────────────────
        if (limitedCollider != null)
        {
            tweenTotal++;
            DOTween.To(
                    () => limitedCollider.radius,
                    r  => limitedCollider.radius = r,
                    targetRadius,
                    growDuration)
                .SetEase(growEase)
                .OnComplete(Done);
        }

        // ── FakeGround BoxCollider centers ─────────────────────────────────────
        if (fakeGroundColliders != null && fakeGroundColliders.Length > 0)
        {
            tweenTotal++;
            float animatedRadius = prevRadius;
            DOTween.To(
                    () => animatedRadius,
                    r  => { animatedRadius = r; ApplyFakeGroundRadius(r); },
                    targetRadius,
                    growDuration)
                .SetEase(growEase)
                .OnComplete(Done);
        }

        // ── HoleSkin scale — cả 3 trục ────────────────────────────────────────
        if (holeSkin != null)
        {
            tweenTotal++;
            holeSkin.DOScale(
                    holeSkin.localScale + Vector3.one / 4 * VisualGrowStep,
                    growDuration)
                .SetEase(growEase)
                .OnComplete(Done);
        }

        // ── DirectionArrow scale — cả 3 trục ──────────────────────────────────
        if (directionArrow != null)
        {
            tweenTotal++;
            directionArrow.DOScale(
                    directionArrow.localScale + Vector3.one * VisualGrowStep,
                    growDuration)
                .SetEase(growEase)
                .OnComplete(Done);
        }

        // ── Camera position ─────────────────────────────────────────────────────
        // Camera tự xử lý qua HoleCameraController — không cần ở đây.

        if (tweenTotal == 0)
            isGrowing = false;
    }

    private void ApplyFakeGroundRadius(float radius)
    {
        if (fakeGroundColliders == null) return;

        foreach (BoxCollider col in fakeGroundColliders)
        {
            if (col == null) continue;
            col.size = new Vector3(colliderHalfSize * 2f, 0f, colliderHalfSize * 2f);
            Vector3 dir = col.center.normalized;
            if (dir == Vector3.zero) continue;
            col.center = dir * (colliderHalfSize + radius);
        }
    }

    // =========================================================================
    // Internal setup
    // =========================================================================

    private void CollectFakeGroundColliders()
    {
        if (fakeGround == null)
        {
            Debug.LogWarning("[HoleSizeController] fakeGround is null — cannot collect BoxColliders.");
            fakeGroundColliders = Array.Empty<BoxCollider>();
            return;
        }

        fakeGroundColliders = fakeGround.GetComponentsInChildren<BoxCollider>();

        if (fakeGroundColliders.Length == 0)
            Debug.LogWarning("[HoleSizeController] Không tìm thấy BoxCollider nào trong FakeGround.");
        else
            Debug.Log($"[HoleSizeController] Tìm thấy {fakeGroundColliders.Length} BoxCollider từ FakeGround.");
    }

    private void SubscribeSwallowHandler()
    {
        swallowHandler = GetComponentInChildren<SwallowHandler>();
        if (swallowHandler != null)
            swallowHandler.OnObjectSwallowed += ForwardScore;
        else
            Debug.LogWarning("[HoleSizeController] SwallowHandler not found in children.");
    }

    private void ValidateRefs()
    {
        if (holeCollider   == null) Debug.LogWarning("[HoleSizeController] holeCollider is null.");
        if (limitedCollider == null) Debug.LogWarning("[HoleSizeController] limitedCollider is null.");
        if (holeSkin       == null) Debug.LogWarning("[HoleSizeController] holeSkin is null.");
        if (directionArrow == null) Debug.LogWarning("[HoleSizeController] directionArrow is null.");
        if (fakeGround     == null) Debug.LogWarning("[HoleSizeController] fakeGround is null.");
    }

    private void ForwardScore(Obstacle obstacle)
    {
        if (obstacle != null && obstacle.ObstacleDefinition != null)
            OnScoreAdded?.Invoke(obstacle.ObstacleDefinition.ScoreValue);
    }

    private void OnDestroy()
    {
        if (swallowHandler != null)
            swallowHandler.OnObjectSwallowed -= ForwardScore;

        if (holeSkin      != null) DOTween.Kill(holeSkin);
        if (directionArrow != null) DOTween.Kill(directionArrow);
    }
}
