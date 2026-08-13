using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// Một floating score text item được quản lý bởi FloatingScorePool.
/// Gắn vào prefab FloatingScoreText — con của Screen Overlay Canvas.
///
/// Vì Canvas dùng Screen Space Overlay, cần convert world position của obstacle
/// sang canvas local position trước khi đặt text.
/// </summary>
public class FloatingScoreText : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    private Sequence activeSequence;
    private System.Action<FloatingScoreText> onComplete;

    // =========================================================================
    // Public API — gọi bởi FloatingScorePool
    // =========================================================================

    /// <summary>
    /// Hiển thị floating text tại worldPosition (world space của obstacle).
    /// Tự convert sang canvas position và trả về pool khi animation kết thúc.
    /// </summary>
    public void Play(
        int      score,
        Vector3  worldPosition,
        Canvas   canvas,
        Camera   camera,
        System.Action<FloatingScoreText> returnToPool,
        float    riseHeight = 150f,
        float    duration   = 1.2f)
    {
        onComplete = returnToPool;

        label.text  = score > 0 ? $"+{score}" : score.ToString();
        label.alpha = 1f;

        // Convert world position → screen position → canvas local position
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 canvasPosition   = WorldToCanvasPosition(worldPosition, canvasRect, camera);

        RectTransform rt = transform as RectTransform;
        rt.anchoredPosition = canvasPosition;

        gameObject.SetActive(true);

        activeSequence?.Kill();

        // Animation: bay lên (anchoredPosition Y) + fade out
        activeSequence = DOTween.Sequence()
            .Append(rt.DOAnchorPosY(canvasPosition.y + riseHeight, duration)
                .SetEase(Ease.OutCubic))
            .Join(label.DOFade(0f, duration)
                .SetEase(Ease.InCubic))
            .OnComplete(ReturnToPool)
            .SetLink(gameObject);
    }

    // =========================================================================
    // Internal
    // =========================================================================

    /// <summary>
    /// Convert world position sang anchoredPosition trên Screen Overlay Canvas.
    /// </summary>
    private static Vector2 WorldToCanvasPosition(Vector3 worldPos, RectTransform canvasRect, Camera camera)
    {
        // World → Screen (pixels)
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, worldPos);

        // Screen → Canvas local position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPoint, null, // null vì Screen Overlay không cần camera
            out Vector2 localPoint);

        return localPoint;
    }

    private void ReturnToPool()
    {
        activeSequence = null;
        gameObject.SetActive(false);
        onComplete?.Invoke(this);
    }
}
