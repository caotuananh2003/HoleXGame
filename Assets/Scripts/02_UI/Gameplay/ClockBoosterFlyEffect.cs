using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Xử lý visual khi swallow Clock booster:
///   1. Icon bay từ vị trí hole → timerText
///   2. Khi icon đến nơi:
///      - GameTimer.AddTime()
///      - timerText flash xanh lá → màu gốc
///      - ExtraTimeText hiện "+X" với fade in → hold → fade out
///      - ExtraTimeExplosion particle kích hoạt
/// </summary>
public class ClockBoosterFlyEffect : MonoBehaviour
{
    [Header("References")]
    [Tooltip("ObjectiveFlyEffect đã có sẵn trong scene — tái dụng pool icon.")]
    [SerializeField] private ObjectiveFlyEffect objectiveFlyEffect;

    [Tooltip("RectTransform của timerText — đích đến của icon bay.")]
    [SerializeField] private RectTransform timerTextRect;

    [Tooltip("TextMeshProUGUI của timerText — để flash màu khi icon đến.")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Tooltip("TextMeshProUGUI ExtraTimeText — hiện '+X' khi nhận thêm giờ.")]
    [SerializeField] private TextMeshProUGUI extraTimeText;

    [Tooltip("ParticleSystem ExtraTimeExplosion — mặc định disabled, Stop Action = Disable.")]
    [SerializeField] private ParticleSystem extraTimeExplosion;

    [Header("Flash Config")]
    [Tooltip("Màu nháy cho cả timerText và extraTimeText.")]
    [SerializeField] private Color flashColor = new Color(0.2f, 0.85f, 0.2f);

    [SerializeField] private float flashInDuration   = 0.15f;
    [SerializeField] private float flashHoldDuration = 0.2f;
    [SerializeField] private float flashOutDuration  = 0.3f;

    [Header("ExtraTimeText Config")]
    [SerializeField] private float extraTextFadeIn   = 0.1f;
    [SerializeField] private float extraTextHold     = 0.6f;
    [SerializeField] private float extraTextFadeOut  = 0.4f;

    // ── Runtime ───────────────────────────────────────────────────────────────

    private HoleController _holeController;
    private Color          _timerOriginalColor;
    private Color          _extraOriginalColor;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Start()
    {
        _holeController = FindAnyObjectByType<HoleController>(FindObjectsInactive.Include);

        if (_holeController != null)
            _holeController.OnClockBoosterSwallowed += OnClockBoosterSwallowed;

        if (timerText != null)
            _timerOriginalColor = timerText.color;

        // ExtraTimeText: ẩn ngay từ đầu
        if (extraTimeText != null)
        {
            _extraOriginalColor   = extraTimeText.color;
            var c                 = extraTimeText.color;
            c.a                   = 0f;
            extraTimeText.color   = c;
            extraTimeText.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (_holeController != null)
            _holeController.OnClockBoosterSwallowed -= OnClockBoosterSwallowed;

        if (timerText    != null) DOTween.Kill(timerText);
        if (extraTimeText != null) DOTween.Kill(extraTimeText);
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private void OnClockBoosterSwallowed(Sprite icon, Vector3 holeWorldPos, float seconds)
    {
        if (objectiveFlyEffect == null || timerTextRect == null)
        {
            OnIconArrived(seconds);
            return;
        }

        objectiveFlyEffect.Fly(
            icon,
            holeWorldPos,
            timerTextRect,
            onArrived: () => OnIconArrived(seconds)
        );
    }

    private void OnIconArrived(float seconds)
    {
        GameTimer.Instance?.AddTime(seconds);
        FlashTimerText();
        ShowExtraTimeText(seconds);
        PlayExplosion();
    }

    private void FlashTimerText()
    {
        if (timerText == null) return;

        DOTween.Kill(timerText);

        DOTween.Sequence()
            .SetLink(timerText.gameObject)
            .Append(timerText.DOColor(flashColor, flashInDuration).SetEase(Ease.OutQuad))
            .AppendInterval(flashHoldDuration)
            .Append(timerText.DOColor(_timerOriginalColor, flashOutDuration).SetEase(Ease.InQuad));
    }

    private void ShowExtraTimeText(float seconds)
    {
        if (extraTimeText == null) return;

        DOTween.Kill(extraTimeText);

        // Format: "+10" hoặc "+10.5" nếu có thập phân
        int secs = Mathf.RoundToInt(seconds);
        extraTimeText.text = $"+{secs}";

        // Set màu flash, alpha = 0, rồi enable
        Color startColor = flashColor;
        startColor.a          = 0f;
        extraTimeText.color   = startColor;
        extraTimeText.gameObject.SetActive(true);

        DOTween.Sequence()
            .SetLink(extraTimeText.gameObject)
            // Fade in + hiện màu xanh
            .Append(extraTimeText.DOFade(1f, extraTextFadeIn).SetEase(Ease.OutQuad))
            .AppendInterval(extraTextHold)
            // Fade out
            .Append(extraTimeText.DOFade(0f, extraTextFadeOut).SetEase(Ease.InQuad))
            .OnComplete(() =>
            {
                extraTimeText.gameObject.SetActive(false);
                // Reset về màu gốc để lần sau hiển thị đúng
                extraTimeText.color = _extraOriginalColor;
            });
    }

    private void PlayExplosion()
    {
        if (extraTimeExplosion == null) return;

        extraTimeExplosion.gameObject.SetActive(true);
        extraTimeExplosion.Play();
    }
}
