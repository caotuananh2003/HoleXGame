using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// Điều phối chuỗi animation màn hình game over (bom).
/// Gắn vào GameObject con bên trong GameOverBombPopup.
///
/// Chuỗi tuần tự khi PlayAsync() được gọi:
///   1. Title       — typewriter
///   2. Description — typewriter
///   3. Phase1:
///       a. animationPhase1 — fade in + Animator play
///       b. descriptionTextPhase1 — typewriter
///   Buttons được enable sau phase 1 (GameOverBombPopup xử lý)
///
/// Khi GameOverBombPopup gọi PlayPhase2Async():
///   - Phase1 bị disable
///   4. Phase2:
///       a. animationPhase2 — fade in + Animator play
///       b. descriptionTextPhase2 — typewriter
///
/// Hierarchy gợi ý:
///   GameOverBombSequencePlayer
///   ├── Title               ← TMP_Text
///   ├── Description         ← TMP_Text
///   ├── Phase1              ← GameObject (enable/disable bởi Popup)
///   │   ├── AnimationPhase1 ← Animator + CanvasGroup
///   │   └── DescriptionTextPhase1 ← TMP_Text
///   └── Phase2              ← GameObject (enable/disable bởi Popup)
///       ├── AnimationPhase2 ← Animator + CanvasGroup
///       └── DescriptionTextPhase2 ← TMP_Text
/// </summary>
public class GameOverBombSequencePlayer : MonoBehaviour
{
    [Header("Title")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private float    titleCharsPerSecond = 50f;

    [Header("Description")]
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private float    descriptionCharsPerSecond = 40f;

    [Header("Phase 1")]
    [SerializeField] private GameObject phase1Root;
    [SerializeField] private Animator   animationPhase1;
    [SerializeField] private CanvasGroup animationPhase1CanvasGroup;
    [SerializeField] private TMP_Text   descriptionTextPhase1;
    [SerializeField] private float      phase1AnimFadeInDuration   = 0.4f;
    [SerializeField] private float      phase1AnimFadeInDelay      = 0f;
    [SerializeField] private float      phase1DescCharsPerSecond   = 40f;

    [Header("Phase 2")]
    [SerializeField] private GameObject phase2Root;
    [SerializeField] private Animator   animationPhase2;
    [SerializeField] private CanvasGroup animationPhase2CanvasGroup;
    [SerializeField] private TMP_Text   descriptionTextPhase2;
    [SerializeField] private float      phase2AnimFadeInDuration   = 0.4f;
    [SerializeField] private float      phase2AnimFadeInDelay      = 0f;
    [SerializeField] private float      phase2DescCharsPerSecond   = 40f;

    [Header("Buttons")]
    [SerializeField] private RectTransform rebornButtonRect;
    [SerializeField] private RectTransform quitButtonRect;
    [SerializeField] private float         buttonFromScale = 0f;
    [SerializeField] private float         buttonPeakScale = 1.1f;
    [SerializeField] private float         buttonToScale   = 1f;
    [SerializeField] private float         buttonDuration  = 0.3f;
    [SerializeField] private float         buttonDelay     = 0.1f;

    // TODO: Slide-in (chưa phát triển)
    //[Header("Title Slide")]
    //[SerializeField] private RectTransform titleRect;
    //[SerializeField] private CanvasGroup   titleCanvasGroup;
    //[SerializeField] private float         titleSlideDuration = 0.4f;
    //[SerializeField] private float         titleSlideOffset   = 300f;
    //[SerializeField] private Ease          titleEase          = Ease.OutCubic;

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>
    /// Chạy toàn bộ chuỗi mở đầu: title → description → phase1.
    /// Buttons KHÔNG được enable ở đây — GameOverBombPopup tự enable sau khi await.
    /// </summary>
    public async UniTask PlayAsync()
    {
        PrepareInitialState();

        await PlayTitleAsync();
        await PlayDescriptionAsync();
        await PlayPhaseAsync(
            animationPhase1, animationPhase1CanvasGroup, phase1AnimFadeInDuration, phase1AnimFadeInDelay,
            descriptionTextPhase1, phase1DescCharsPerSecond
        );
    }

    /// <summary>
    /// Chạy phase 2. Gọi từ GameOverBombPopup sau khi người dùng bấm Close lần 1.
    /// GameOverBombPopup tự disable phase1Root trước khi gọi hàm này.
    /// </summary>
    public async UniTask PlayPhase2Async()
    {
        if (phase2Root != null) phase2Root.SetActive(true);

        if (animationPhase2CanvasGroup != null) animationPhase2CanvasGroup.alpha = 0f;
        if (descriptionTextPhase2 != null)
        {
            descriptionTextPhase2.ForceMeshUpdate();
            descriptionTextPhase2.maxVisibleCharacters = 0;
        }

        await PlayPhaseAsync(
            animationPhase2, animationPhase2CanvasGroup, phase2AnimFadeInDuration, phase2AnimFadeInDelay,
            descriptionTextPhase2, phase2DescCharsPerSecond
        );
    }

    // =========================================================================
    // Initial state
    // =========================================================================

    private void PrepareInitialState()
    {
        // Title / Description chính
        if (titleText       != null) { titleText.ForceMeshUpdate();       titleText.maxVisibleCharacters       = 0; }
        if (descriptionText != null) { descriptionText.ForceMeshUpdate(); descriptionText.maxVisibleCharacters = 0; }

        // Phase 1
        if (phase1Root != null)                   phase1Root.SetActive(true);
        if (animationPhase1CanvasGroup != null)   animationPhase1CanvasGroup.alpha = 0f;
        if (descriptionTextPhase1 != null)
        {
            descriptionTextPhase1.ForceMeshUpdate();
            descriptionTextPhase1.maxVisibleCharacters = 0;
        }

        // Phase 2 — ẩn cho đến khi cần
        if (phase2Root != null) phase2Root.SetActive(false);

        // Buttons
        if (rebornButtonRect != null) rebornButtonRect.localScale = Vector3.zero;
        if (quitButtonRect   != null) quitButtonRect.localScale   = Vector3.zero;
    }

    // =========================================================================
    // Steps
    // =========================================================================

    private UniTask PlayTitleAsync()
        => TypewriterEffect.PlayAsync(titleText, titleCharsPerSecond, cancellationToken: destroyCancellationToken);

    private UniTask PlayDescriptionAsync()
        => TypewriterEffect.PlayAsync(descriptionText, descriptionCharsPerSecond, cancellationToken: destroyCancellationToken);

    /// <summary>
    /// Chạy 1 phase: fade in animation → Animator play → typewriter description.
    /// </summary>
    private async UniTask PlayPhaseAsync(
        Animator     animator,
        CanvasGroup  animCG,
        float        fadeInDuration,
        float        fadeInDelay,
        TMP_Text     descText,
        float        charsPerSecond)
    {
        // Fade in animation
        await FadeInAsync(animCG, fadeInDuration, fadeInDelay);

        // Animator play từ frame 0
        animator?.Play(0, 0, 0f);

        // Typewriter description
        await TypewriterEffect.PlayAsync(descText, charsPerSecond, cancellationToken: destroyCancellationToken);
    }

    /// <summary>
    /// Bounce in 2 button theo thứ tự, sau khi phase1 hoặc phase2 kết thúc.
    /// Gọi từ GameOverBombPopup.
    /// </summary>
    public async UniTask PlayButtonsAsync()
    {
        await BounceAsync(rebornButtonRect, buttonFromScale, buttonPeakScale, buttonToScale, buttonDuration);

        if (buttonDelay > 0f)
            await UniTask.Delay(System.TimeSpan.FromSeconds(buttonDelay), ignoreTimeScale: true);

        await BounceAsync(quitButtonRect, buttonFromScale, buttonPeakScale, buttonToScale, buttonDuration);
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    private UniTask FadeInAsync(CanvasGroup cg, float duration, float delay)
    {
        if (cg == null) return UniTask.CompletedTask;

        cg.alpha = 0f;

        Sequence seq = DOTween.Sequence().SetUpdate(true);

        if (delay > 0f) seq.AppendInterval(delay);

        seq.Append(cg.DOFade(1f, duration));

        return seq.ToUniTask();
    }

    /// <summary>
    /// fromScale → peakScale → toScale, thời gian mỗi nửa = halfDuration.
    /// </summary>
    private UniTask BounceAsync(Transform t, float fromScale, float peakScale, float toScale, float halfDuration)
    {
        if (t == null) return UniTask.CompletedTask;

        t.localScale = Vector3.one * fromScale;

        return DOTween.Sequence()
            .Append(t.DOScale(peakScale, halfDuration).SetEase(Ease.OutQuad))
            .Append(t.DOScale(toScale,   halfDuration).SetEase(Ease.InQuad))
            .SetUpdate(true)
            .ToUniTask();
    }
}
