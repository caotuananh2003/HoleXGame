using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Điều phối chuỗi animation màn hình thắng.
///
/// Phase 1 — tất cả chạy song song:
///   - Particle chạy nền
///   - Victory animation scale in
///   - Ribbon mở ra
///   - 3 star slot lần lượt bounce in → hold → bounce out (lệch nhau starDelay)
///   Sau khi stars xong → ribbon + victory out cùng lúc
///
/// Phase 2 — tất cả chạy song song:
///   - WellDone bounce in, sau đó loop scale 1.8 ↔ 2.2
///   - Reward bounce in
///   - Buttons lần lượt bounce in (lệch nhau buttonDelay)
/// </summary>
public class WinSequencePlayer : MonoBehaviour
{
    [Header("Phase 1 — Particle")]
    [SerializeField] private ParticleSystem winParticle;

    [Header("Phase 1 — Victory Animation")]
    [SerializeField] private Animator      victoryAnimator;
    [SerializeField] private RectTransform victoryAnimRect;
    [SerializeField] private float         victoryInDuration  = 0.5f;
    [SerializeField] private float         victoryOutDuration = 0.4f;

    [Header("Phase 1 — Ribbon")]
    [SerializeField] private RectTransform ribbonRect;
    [SerializeField] private float         ribbonDuration = 0.4f;

    [Header("Phase 1 — Stars")]
    [SerializeField] private Sprite      starOffSprite;
    [SerializeField] private Sprite      starOnSprite;
    [SerializeField] private Transform[] starSpawnPoints;
    [SerializeField] private float       starDelay        = 0.1f;
    [SerializeField] private float       starDuration     = 0.3f;
    [SerializeField] private float       starHoldDuration = 0.4f;

    [Header("Phase 2 — WellDone")]
    [SerializeField] private RectTransform wellDoneRect;
    [SerializeField] private float         wellDoneDuration  = 0.3f;
    [SerializeField] private float         wellDonePulseMin  = 1.8f;
    [SerializeField] private float         wellDonePulseMax  = 2.2f;
    [SerializeField] private float         wellDonePulseDuration = 0.5f;

    [Header("Phase 2 — Reward")]
    [SerializeField] private RectTransform rewardRect;
    [SerializeField] private float         rewardDuration = 0.3f;

    [Header("Phase 2 — Buttons")]
    [SerializeField] private RectTransform[] buttons;
    [SerializeField] private float           buttonDelay    = 0.1f;
    [SerializeField] private float           buttonDuration = 0.3f;

    private readonly List<GameObject> spawnedStars = new();
    private Tween wellDonePulseTween;

    // =========================================================================
    // Public API
    // =========================================================================

    public async UniTask PlayAsync()
    {
        PrepareInitialState();
        await PlayPhase1Async();
        await PlayPhase2Async();
    }

    public void Cleanup()
    {
        wellDonePulseTween?.Kill();
        wellDonePulseTween = null;

        foreach (GameObject star in spawnedStars)
            if (star != null) Destroy(star);

        spawnedStars.Clear();
    }

    #region Phase 1
    private async UniTask PlayPhase1Async()
    {
        winParticle?.Play();
        victoryAnimator?.Play(0, 0, 0f);

        // Stars chạy độc lập, lệch nhau starDelay
        var starTasks = new UniTask[starSpawnPoints?.Length ?? 0];
        for (int i = 0; i < starTasks.Length; i++)
            starTasks[i] = PlayOneStarSlotAsync(i, starDelay * i);

        // VictoryAnim + Ribbon in + tất cả stars chạy song song
        await UniTask.WhenAll(
            PlayScaleAsync(victoryAnimRect, from: 0f, to: 5f, victoryInDuration),
            PlayRibbonAsync(scaleIn: true),
            UniTask.WhenAll(starTasks)
        );

        // Stars xong → VictoryAnim + ribbon out song song
        await UniTask.WhenAll(
            PlayScaleAsync(victoryAnimRect, from: 5f, to: 0f, victoryOutDuration),
            PlayRibbonAsync(scaleIn: false)
        );
    }

    private async UniTask PlayOneStarSlotAsync(int index, float delay) // Anim cho 1 star
    {
        if (delay > 0f)
            await UniTask.Delay(System.TimeSpan.FromSeconds(delay), ignoreTimeScale: true);

        Transform parent = starSpawnPoints[index];

        GameObject starOff = CreateStar(starOffSprite, parent);
        spawnedStars.Add(starOff);
        await BounceAsync(starOff.transform, 0f, 1.1f, 1f);

        GameObject starOn = CreateStar(starOnSprite, parent);
        spawnedStars.Add(starOn);
        await BounceAsync(starOn.transform, 0f, 1.1f, 1f);

        await UniTask.Delay(System.TimeSpan.FromSeconds(starHoldDuration), ignoreTimeScale: true);

        await BounceAsync(starOn.transform,  1f, 1.1f, 0f);
        await BounceAsync(starOff.transform, 1f, 1.1f, 0f);
    }

    private UniTask PlayRibbonAsync(bool scaleIn) // Anim cho Ribbon
    {
        if (ribbonRect == null) return UniTask.CompletedTask;

        if (scaleIn) ribbonRect.localScale = new Vector3(0f, 1f, 1f);

        float peak = scaleIn ? 1.1f : 1.1f;
        float end  = scaleIn ? 1f   : 0f;

        return DOTween.Sequence()
            .Append(ribbonRect.DOScaleX(peak, ribbonDuration).SetEase(Ease.OutQuad))
            .Append(ribbonRect.DOScaleX(end,  ribbonDuration).SetEase(Ease.InQuad))
            .SetUpdate(true)
            .ToUniTask();
    }
    #endregion

    #region Phase 2
    private async UniTask PlayPhase2Async()
    {
        // WellDone, Reward và Buttons bắt đầu song song
        await UniTask.WhenAll(
            PlayWellDoneAsync(),
            PlayRewardAsync(),
            PlayButtonsAsync()
        );
    }

    private async UniTask PlayWellDoneAsync()
    {
        if (wellDoneRect == null) return;

        await BounceAsync(wellDoneRect, 0f, 2.2f, 2f, wellDoneDuration);

        // Loop scale 1.8 ↔ 2.2 vô hạn sau khi xuất hiện
        wellDonePulseTween = wellDoneRect
            .DOScale(wellDonePulseMax, wellDonePulseDuration)
            .From(wellDonePulseMin)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    private UniTask PlayRewardAsync()
    {
        if (rewardRect == null) return UniTask.CompletedTask;

        return BounceAsync(rewardRect, 0f, 1.1f, 1f, rewardDuration);
    }

    private async UniTask PlayButtonsAsync()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;

            // Stagger: button[i] bắt đầu sau i * buttonDelay
            await UniTask.Delay(
                System.TimeSpan.FromSeconds(i == 0 ? 0f : buttonDelay),
                ignoreTimeScale: true
            );

            BounceAsync(buttons[i], 0f, 1.1f, 1f, buttonDuration).Forget();
        }
    }
    #endregion

    // =========================================================================
    // Helpers
    // =========================================================================

    // fromScale → peakScale → toScale, thời gian mỗi nửa = starDuration
    private UniTask BounceAsync(Transform t, float from, float peak, float to)
        => BounceAsync(t, from, peak, to, starDuration);

    private UniTask BounceAsync(Transform t, float from, float peak, float to, float halfDuration)
    {
        t.localScale = Vector3.one * from;

        return DOTween.Sequence()
            .Append(t.DOScale(peak, halfDuration).SetEase(Ease.OutQuad))
            .Append(t.DOScale(to,   halfDuration).SetEase(Ease.InQuad))
            .SetUpdate(true)
            .ToUniTask();
    }

    private UniTask PlayScaleAsync(RectTransform rect, float from, float to, float duration)
    {
        if (rect == null) return UniTask.CompletedTask;

        rect.localScale = Vector3.one * from;

        return rect.DOScale(to, duration)
            .SetEase(from < to ? Ease.OutQuad : Ease.InQuad)
            .SetUpdate(true)
            .ToUniTask();
    }

    private GameObject CreateStar(Sprite sprite, Transform parent)
    {
        var star = new GameObject(sprite.name, typeof(RectTransform), typeof(Image));
        star.transform.SetParent(parent, false);

        var img  = star.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.SetNativeSize();

        var rect = star.GetComponent<RectTransform>();
        rect.anchorMin        = new Vector2(0.5f, 0.5f);
        rect.anchorMax        = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.localRotation    = Quaternion.identity;
        rect.localScale       = Vector3.zero;

        return star;
    }

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void PrepareInitialState()
    {
        Cleanup();

        if (wellDoneRect != null) wellDoneRect.localScale = Vector3.zero;
        if (rewardRect   != null) rewardRect.localScale   = Vector3.zero;

        foreach (RectTransform btn in buttons)
            if (btn != null) btn.localScale = Vector3.zero;
    }

    private void OnDestroy() => Cleanup();
}
