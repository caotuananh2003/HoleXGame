using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pool các Image bay từ vị trí player về đúng ObjectiveUIItem trên Canvas.
///
/// Setup trong Inspector:
///   - iconPrefab   : prefab chứa Image component (không có text, chỉ icon)
///   - canvas       : Screen Space Overlay Canvas chứa objective UI
///   - mainCamera   : MainCamera để convert world → screen position
///
/// Gọi: objectiveFlyEffect.Fly(sprite, playerWorldPos, targetRect, onArrived)
/// </summary>
public class ObjectiveFlyEffect : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Prefab chứa Image component dùng làm icon bay.")]
    [SerializeField] private Image     iconPrefab;
    [SerializeField] private Canvas    canvas;
    [SerializeField] private Camera    mainCamera;

    [Header("Pool")]
    [SerializeField] private int initialPoolSize = 8;

    [Header("Animation")]
    [Tooltip("Thời gian bay từ player đến objective (giây).")]
    [SerializeField] private float flyDuration = 0.45f;

    [Tooltip("Scale icon khi bắt đầu bay.")]
    [SerializeField] private float startScale  = 1.2f;

    [Tooltip("Scale icon khi đến nơi (trước khi Destroy/return pool).")]
    [SerializeField] private float endScale    = 0.4f;

    [Tooltip("Ease của chuyển động bay.")]
    [SerializeField] private Ease  flyEase     = Ease.InCubic;

    // ── Pool ──────────────────────────────────────────────────────────────────
    private readonly Queue<Image> _pool = new();

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Pre-warm pool
        for (int i = 0; i < initialPoolSize; i++)
            _pool.Enqueue(CreateInstance());
    }

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>
    /// Spawn icon tại vị trí world của player, bay về targetRect.
    /// onArrived được gọi khi icon đến nơi (dùng để trigger pulse trên ObjectiveUIItem).
    /// </summary>
    public void Fly(Sprite icon, Vector3 playerWorldPos, RectTransform targetRect,
                    System.Action onArrived = null)
    {
        if (canvas == null || iconPrefab == null) return;

        Image img = _pool.Count > 0 ? _pool.Dequeue() : CreateInstance();

        img.sprite = icon;
        img.gameObject.SetActive(true);

        RectTransform rt        = img.rectTransform;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // Convert player world position → canvas local position
        Vector2 startPos = WorldToCanvasPosition(playerWorldPos, canvasRect);
        rt.anchoredPosition  = startPos;
        rt.localScale        = Vector3.one * startScale;

        // Vị trí đích = anchoredPosition của ObjectiveUIItem trong cùng Canvas
        Vector2 endPos = GetCanvasPosition(targetRect, canvasRect);

        DOTween.Sequence()
            .Join(rt.DOAnchorPos(endPos, flyDuration).SetEase(flyEase))
            .Join(rt.DOScale(endScale, flyDuration).SetEase(flyEase))
            .SetLink(img.gameObject)
            .OnComplete(() =>
            {
                ReturnToPool(img);
                onArrived?.Invoke();
            });
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private Image CreateInstance()
    {
        Image instance = Instantiate(iconPrefab, canvas.transform);
        instance.gameObject.SetActive(false);
        instance.raycastTarget = false;
        return instance;
    }

    private void ReturnToPool(Image img)
    {
        img.gameObject.SetActive(false);
        _pool.Enqueue(img);
    }

    /// <summary>Convert world position → anchoredPosition trên Screen Overlay Canvas.</summary>
    private Vector2 WorldToCanvasPosition(Vector3 worldPos, RectTransform canvasRect)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(mainCamera, worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPoint, null, out Vector2 localPoint);
        return localPoint;
    }

    /// <summary>Lấy anchoredPosition của một RectTransform bất kỳ quy về gốc toạ độ của Canvas.</summary>
    private static Vector2 GetCanvasPosition(RectTransform target, RectTransform canvasRect)
    {
        // Dùng corners của target để lấy screen position trung tâm
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        // Tâm = trung bình 4 góc
        Vector3 centerWorld = (corners[0] + corners[1] + corners[2] + corners[3]) / 4f;

        // Canvas Overlay: world corners của UI = screen position (pixel) trực tiếp
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, centerWorld);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPoint, null, out Vector2 localPoint);
        return localPoint;
    }

    private void OnDisable()
    {
        // Khi gameplay group bị disable, ẩn hết icon đang bay
        foreach (Image img in _pool)
            img.gameObject.SetActive(false);
    }
}
