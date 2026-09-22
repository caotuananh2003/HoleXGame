using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI item hiển thị 1 objective: icon + progress text (3/5).
/// Được spawn runtime từ GameplayPanel dựa trên danh sách objectives.
///
/// Khi objective hoàn thành:
///   - PlayCompleteAnimation() fade out CanvasGroup bằng DOTween
///   - OnComplete: gọi callback (để GameplayPanel biết animation xong)
///     rồi Destroy gameObject
///   - ContentSizeFitter trên ObjectiveContainer sẽ tự recalculate width
/// </summary>
public class ObjectiveUIItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image              iconImage;
    [SerializeField] private TextMeshProUGUI    progressText;

    [Header("Animation")]
    [SerializeField] private float fadeDuration    = 0.4f;
    [SerializeField] private float scaleUpAmount   = 1.2f;
    [SerializeField] private float scaleUpDuration = 0.15f;

    [Tooltip("Scale pulse khi nhận được 1 icon bay đến.")]
    [SerializeField] private float pulseScale    = 1.15f;
    [SerializeField] private float pulseDuration = 0.12f;

    private LevelObjective objective;
    private CanvasGroup    canvasGroup;

    /// <summary>RectTransform của item này — ObjectiveFlyEffect dùng làm điểm đến.</summary>
    public RectTransform RectTransform => transform as RectTransform;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        // CanvasGroup dùng để fade — tự thêm nếu chưa có
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void OnDestroy()
    {
        DOTween.Kill(transform);
        DOTween.Kill(canvasGroup);
    }

    // =========================================================================
    // Public API
    // =========================================================================

    public void Initialize(LevelObjective objective)
    {
        this.objective = objective;

        if (objective?.ObstacleDefinition != null)
        {
            if (iconImage != null && objective.ObstacleDefinition.Icon != null)
                iconImage.sprite = objective.ObstacleDefinition.Icon;

            UpdateProgress();
        }
    }

    public void UpdateProgress()
    {
        if (objective == null || progressText == null) return;

        int remaining = objective.RequiredCount - objective.CurrentCount;
        if (remaining < 0) remaining = 0;

        progressText.text = remaining.ToString();

        if (objective.IsCompleted)
            progressText.color = Color.green;
    }

    /// <summary>
    /// Pulse nhẹ khi nhận được icon bay đến — feedback "đã cộng 1".
    /// Không chạy nếu objective đã complete (sắp PlayCompleteAnimation).
    /// </summary>
    public void PulseOnReceive()
    {
        if (objective == null || objective.IsCompleted) return;

        DOTween.Kill(transform, complete: false);

        transform.DOScale(Vector3.one * pulseScale, pulseDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => transform.DOScale(Vector3.one, pulseDuration).SetEase(Ease.InQuad))
            .SetLink(gameObject);
    }

    /// <summary>
    /// Chạy animation hoàn thành: scale up nhẹ → fade out → Destroy.
    /// onAnimationDone được gọi SAU KHI Destroy để GameplayPanel cập nhật state.
    /// </summary>
    public void PlayCompleteAnimation(Action onAnimationDone = null)
    {
        // Tắt interaction ngay để tránh click lại
        if (canvasGroup != null)
            canvasGroup.interactable = false;

        // Scale up nhẹ rồi fade out
        Sequence seq = DOTween.Sequence()
            .Append(transform.DOScale(Vector3.one * scaleUpAmount, scaleUpDuration)
                .SetEase(Ease.OutBack))
            .Append(canvasGroup.DOFade(0f, fadeDuration)
                .SetEase(Ease.InCubic))
            .OnComplete(() =>
            {
                onAnimationDone?.Invoke();
                Destroy(gameObject);
            })
            .SetLink(gameObject);
    }
}
