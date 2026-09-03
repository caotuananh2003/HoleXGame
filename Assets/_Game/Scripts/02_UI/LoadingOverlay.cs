using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Loading overlay hiển thị khi chuyển scene.
/// SHOW : Alpha 0→1, Scale 0.9→1.0 — chờ animation xong mới return.
/// HIDE : Alpha 1→0, Scale 1.0→0.9 — chờ animation xong rồi SetActive(false).
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class LoadingOverlay : MonoBehaviour
{
    [Header("Animation")]
    private float showDuration = 2f;
    private float hideDuration = 1f;
    private float startScale   = 0.9f;
    [SerializeField] private Ease  showEase     = Ease.OutQuad;
    [SerializeField] private Ease  hideEase     = Ease.InQuad;
    [SerializeField] private CanvasGroup canvasGroup;

    private Sequence    currentSequence;

    private void Awake()
    {
        // Bắt đầu ở trạng thái ẩn
        canvasGroup.alpha    = 0f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        currentSequence?.Kill(false);
    }

    /// <summary>
    /// Hiện overlay tức thì, không animation.
    /// Dùng khi Boot — che màn hình ngay trước khi các hệ thống init.
    /// </summary>
    public void ShowImmediate()
    {
        currentSequence?.Kill(false);
        currentSequence = null;

        transform.localScale       = Vector3.one;
        canvasGroup.alpha          = 1f;
        canvasGroup.blocksRaycasts = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Hiện overlay với animation.
    /// Chờ đến khi animation hoàn tất mới return.
    /// </summary>
    public async UniTask ShowAsync()
    {
        currentSequence?.Kill(false);
        currentSequence = null;

        transform.localScale     = Vector3.one * startScale;
        canvasGroup.alpha        = 0f;
        canvasGroup.blocksRaycasts = true;
        gameObject.SetActive(true);

        var tcs = new UniTaskCompletionSource();

        currentSequence = DOTween.Sequence()
            .Join(transform.DOScale(Vector3.one, showDuration).SetEase(showEase))
            .Join(canvasGroup.DOFade(1f, showDuration))
            .SetUpdate(true)
            .OnComplete(() =>
            {
                currentSequence = null;
                tcs.TrySetResult();
            });

        await tcs.Task;
    }

    /// <summary>
    /// Ẩn overlay với animation.
    /// Chờ đến khi animation hoàn tất rồi mới SetActive(false).
    /// </summary>
    public async UniTask HideAsync()
    {
        currentSequence?.Kill(false);
        currentSequence = null;

        var tcs = new UniTaskCompletionSource();

        currentSequence = DOTween.Sequence()
            .Join(transform.DOScale(startScale, hideDuration).SetEase(hideEase))
            .Join(canvasGroup.DOFade(0f, hideDuration))
            .SetUpdate(true)
            .OnComplete(() =>
            {
                currentSequence = null;
                HideImmediate();
                tcs.TrySetResult();
            });

        await tcs.Task;
    }

    /// <summary>
    /// Ẩn overlay tức thì, không animation. Dùng để reset trạng thái khi cần.
    /// </summary>
    public void HideImmediate()
    {
        currentSequence?.Kill(false);
        currentSequence = null;

        canvasGroup.alpha          = 0f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}
