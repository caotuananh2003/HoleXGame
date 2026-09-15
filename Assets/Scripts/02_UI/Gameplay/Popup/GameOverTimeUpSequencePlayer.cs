using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// Điều phối chuỗi animation màn hình game over (hết giờ).
/// Gắn vào GameObject con bên trong GameOverTimeUpPopup.
///
/// Chuỗi tuần tự khi PlayAsync() được gọi:
///   1. Title          — typewriter
///   2. Description    — typewriter
///   3. Animation      — fade in + Animator play
///   4. ProgressTimeAdd — fade in
///   5. Clock1, Clock2, Clock3 — bounce in lần lượt
///   6. AdsBonusButton — bounce in
///   7. ResumeButton   — bounce in
///   8. QuitButton     — bounce in
///
/// Hierarchy gợi ý:
///   GameOverTimeUpSequencePlayer
///   ├── Title                 ← TMP_Text
///   ├── Description           ← TMP_Text
///   ├── Animation             ← Animator + CanvasGroup
///   ├── ProgressTimeAdd       ← RectTransform + CanvasGroup
///   │   ├── Clock1            ← RectTransform
///   │   ├── Clock2            ← RectTransform
///   │   └── Clock3            ← RectTransform
///   ├── AdsBonusButton        ← RectTransform
///   ├── ResumeButton          ← RectTransform
///   └── QuitButton            ← RectTransform
/// </summary>
public class GameOverTimeUpSequencePlayer : MonoBehaviour
{
    [Header("Title")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private float    titleCharsPerSecond = 50f;

    [Header("Description")]
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private float    descriptionCharsPerSecond = 40f;

    [Header("Animation")]
    [SerializeField] private Animator    animator;
    [SerializeField] private CanvasGroup animationCanvasGroup;
    [SerializeField] private float       animFadeInDuration = 0.4f;
    [SerializeField] private float       animFadeInDelay    = 0f;

    [Header("Progress Time Add")]
    [SerializeField] private CanvasGroup progressCanvasGroup;
    [SerializeField] private float       progressFadeInDuration = 0.3f;
    [SerializeField] private float       progressFadeInDelay    = 0.1f;

    [Header("Clocks")]
    [SerializeField] private RectTransform clock1Rect;
    [SerializeField] private RectTransform clock2Rect;
    [SerializeField] private RectTransform clock3Rect;
    [SerializeField] private float         clockFromScale = 0f;
    [SerializeField] private float         clockPeakScale = 1.2f;
    [SerializeField] private float         clockToScale   = 1f;
    [SerializeField] private float         clockDuration  = 0.3f;
    [SerializeField] private float         clockDelay     = 0.1f;

    [Header("Buttons")]
    [SerializeField] private RectTransform adsBonusButtonRect;
    [SerializeField] private RectTransform resumeButtonRect;
    [SerializeField] private RectTransform quitButtonRect;
    [SerializeField] private float         buttonFromScale = 0f;
    [SerializeField] private float         buttonPeakScale = 1.1f;
    [SerializeField] private float         buttonToScale   = 1f;
    [SerializeField] private float         buttonDuration  = 0.3f;
    [SerializeField] private float         buttonDelay     = 0.1f;

    /// <summary>
    /// Chạy toàn bộ chuỗi intro.
    /// Buttons KHÔNG được enable ở đây — GameOverTimeUpPopup tự enable sau khi await.
    /// </summary>
    public async UniTask PlayAsync()
    {
        Validate();
        PrepareInitialState();

        await PlayTitleAsync();
        await PlayDescriptionAsync();
        await PlayAnimationAsync();
        await PlayProgressAsync();
        await PlayClocksAsync();
    }

    /// <summary>
    /// Bounce in AdsBonusButton → ResumeButton → QuitButton theo thứ tự.
    /// Gọi từ GameOverTimeUpPopup sau khi PlayAsync() hoàn tất.
    /// </summary>
    public async UniTask PlayButtonsAsync()
    {
        await BounceAsync(adsBonusButtonRect, buttonFromScale, buttonPeakScale, buttonToScale, buttonDuration);

        if (buttonDelay > 0f)
            await UniTask.Delay(System.TimeSpan.FromSeconds(buttonDelay), ignoreTimeScale: true);

        await BounceAsync(resumeButtonRect, buttonFromScale, buttonPeakScale, buttonToScale, buttonDuration);

        if (buttonDelay > 0f)
            await UniTask.Delay(System.TimeSpan.FromSeconds(buttonDelay), ignoreTimeScale: true);

        await BounceAsync(quitButtonRect, buttonFromScale, buttonPeakScale, buttonToScale, buttonDuration);
    }

    private void PrepareInitialState() // Initial state
    {
        titleText.ForceMeshUpdate();
        titleText.maxVisibleCharacters = 0;
        descriptionText.ForceMeshUpdate();
        descriptionText.maxVisibleCharacters = 0;

        animationCanvasGroup.alpha = 0f;

        progressCanvasGroup.alpha = 0f;

        clock1Rect.localScale = Vector3.zero;
        clock2Rect.localScale = Vector3.zero;
        clock3Rect.localScale = Vector3.zero;

        adsBonusButtonRect.localScale = Vector3.zero;
        resumeButtonRect.localScale = Vector3.zero;
        quitButtonRect.localScale = Vector3.zero;
    }

    #region Steps
    private UniTask PlayTitleAsync()
    {
        return TypewriterEffect.PlayAsync(titleText, titleCharsPerSecond, cancellationToken: destroyCancellationToken);
    }

    private UniTask PlayDescriptionAsync()
    {
        return TypewriterEffect.PlayAsync(descriptionText, descriptionCharsPerSecond, cancellationToken: destroyCancellationToken);
    }

    private async UniTask PlayAnimationAsync()
    {
        await FadeInAsync(animationCanvasGroup, animFadeInDuration, animFadeInDelay);
        animator?.Play(0, 0, 0f);
    }

    private UniTask PlayProgressAsync()
    {
        return FadeInAsync(progressCanvasGroup, progressFadeInDuration, progressFadeInDelay);
    }

    private async UniTask PlayClocksAsync()
    {
        await BounceAsync(clock1Rect, clockFromScale, clockPeakScale, clockToScale, clockDuration);

        if (clockDelay > 0f)
            await UniTask.Delay(System.TimeSpan.FromSeconds(clockDelay), ignoreTimeScale: true);

        await BounceAsync(clock2Rect, clockFromScale, clockPeakScale, clockToScale, clockDuration);

        if (clockDelay > 0f)
            await UniTask.Delay(System.TimeSpan.FromSeconds(clockDelay), ignoreTimeScale: true);

        await BounceAsync(clock3Rect, clockFromScale, clockPeakScale, clockToScale, clockDuration);
    }
    #endregion

    #region Effect
    private UniTask FadeInAsync(CanvasGroup cg, float duration, float delay)
    {
        if (cg == null) return UniTask.CompletedTask;

        cg.alpha = 0f;

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        if (delay > 0f)
        {
            seq.AppendInterval(delay);
        }

        seq.Append(cg.DOFade(1f, duration));

        return seq.ToUniTask();
    }

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
    #endregion

    private void Validate() // Validate kiểm tra nếu có null thì log ra error tương ứng
    {
        if (titleText == null)                  Debug.LogError($"{nameof(GameOverTimeUpSequencePlayer)}: Title Text is not assigned.", this);
        if (descriptionText == null)            Debug.LogError($"{nameof(GameOverTimeUpSequencePlayer)}: Description Text is not assigned.", this);
        if (animator == null)                   Debug.LogError($"{nameof(GameOverTimeUpSequencePlayer)}: Animator is not assigned.", this);
        if (animationCanvasGroup == null)       Debug.LogError($"{nameof(GameOverTimeUpSequencePlayer)}: Animation CanvasGroup is not assigned.", this);
        if (progressCanvasGroup == null)        Debug.LogError($"{nameof(GameOverTimeUpSequencePlayer)}: Progress CanvasGroup is not assigned.", this);
        if (clock1Rect == null)                 Debug.LogError($"{nameof(GameOverTimeUpSequencePlayer)}: Clock1 RectTransform is not assigned.", this);
        if (clock2Rect == null)                 Debug.LogError($"{nameof(GameOverTimeUpSequencePlayer)}: Clock2 RectTransform is not assigned.", this);
        if (clock3Rect == null)                 Debug.LogError($"{nameof(GameOverTimeUpSequencePlayer)}: Clock3 RectTransform is not assigned.", this);
        if (adsBonusButtonRect == null)         Debug.LogError($"{nameof(GameOverTimeUpSequencePlayer)}: AdsBonusButton RectTransform is not assigned.", this);
        if (resumeButtonRect == null)           Debug.LogError($"{nameof(GameOverTimeUpSequencePlayer)}: ResumeButton RectTransform is not assigned.", this);
        if (quitButtonRect == null)             Debug.LogError($"{nameof(GameOverTimeUpSequencePlayer)}: QuitButton RectTransform is not assigned.", this);
        if (titleCharsPerSecond <= 0f)          Debug.LogError($"{nameof(GameOverTimeUpSequencePlayer)}: titleCharsPerSecond must be > 0.", this);
        if (descriptionCharsPerSecond <= 0f)    Debug.LogError($"{nameof(GameOverTimeUpSequencePlayer)}: descriptionCharsPerSecond must be > 0.", this);
        if (animFadeInDuration < 0f)            Debug.LogError($"{nameof(GameOverTimeUpSequencePlayer)}: animFadeInDuration must be >= 0.", this);
        if (progressFadeInDuration < 0f)        Debug.LogError($"{nameof(GameOverTimeUpSequencePlayer)}: progressFadeInDuration must be >= 0.", this);
        if (clockDuration < 0f)                 Debug.LogError($"{nameof(GameOverTimeUpSequencePlayer)}: clockDuration must be >= 0.", this);
        if (buttonDuration < 0f)                Debug.LogError($"{nameof(GameOverTimeUpSequencePlayer)}: buttonDuration must be >= 0.", this);
        if (clockDelay < 0f)                    Debug.LogError($"{nameof(GameOverTimeUpSequencePlayer)}: clockDelay must be >= 0.", this);
        if (buttonDelay < 0f)                   Debug.LogError($"{nameof(GameOverTimeUpSequencePlayer)}: buttonDelay must be >= 0.", this);
    }
}
