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
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float scaleUpAmount = 1.2f;
    [SerializeField] private float scaleUpDuration = 0.15f;

    private LevelObjective objective;
    private CanvasGroup    canvasGroup;

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

        progressText.text = $"{objective.CurrentCount}/{objective.RequiredCount}";

        if (objective.IsCompleted)
            progressText.color = Color.green;
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
