using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Hiệu ứng scale "nảy nhẹ" khi nhấn button.
/// Gắn lên bất kỳ GameObject có Button component.
///
/// PointerDown → scale về 0.9 (nhanh)
/// PointerUp   → scale về 1.0 với Ease.OutBack (nảy nhẹ)
/// PointerExit → scale về 1.0 ngay lập tức nếu ngón tay/chuột rời ra khi đang giữ
///
/// Kill tween cũ trước khi tạo tween mới → không bao giờ xung đột.
/// </summary>
public class UIButtonScaleEffect : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [Header("Scale")]
    [SerializeField] private float pressedScale  = 0.9f;
    [SerializeField] private float normalScale   = 1.0f;

    [Header("Duration")]
    [SerializeField] private float pressDuration   = 0.08f;
    [SerializeField] private float releaseDuration = 0.35f;

    // Target là transform của chính GameObject này
    private Transform cachedTransform;

    private void Awake()
    {
        cachedTransform = transform;
    }

    private void OnDestroy()
    {
        DOTween.Kill(cachedTransform);
    }

    // =========================================================================
    // Pointer events
    // =========================================================================

    public void OnPointerDown(PointerEventData eventData)
    {
        DOTween.Kill(cachedTransform);
        cachedTransform
            .DOScale(pressedScale, pressDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true); // Chạy ngay cả khi Time.timeScale = 0
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        DOTween.Kill(cachedTransform);
        cachedTransform
            .DOScale(normalScale, releaseDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Nếu rời khỏi vùng button khi đang giữ → trả về scale bình thường
        DOTween.Kill(cachedTransform);
        cachedTransform
            .DOScale(normalScale, releaseDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }
}
