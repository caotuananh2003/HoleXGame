using DG.Tweening;
using UnityEngine;

/// <summary>
/// Base class cho tất cả Popup trong project.
/// Kế thừa UIWindow, tự động thêm CanvasGroup và chạy animation Open/Close bằng DOTween.
///
/// OPEN  : Scale 0.7→1.0, Alpha 0→1 song song
/// CLOSE : Scale 1.0→0.7, Alpha 1→0 song song → SetActive(false) sau khi xong
///
/// ─── Tại sao Awake() không log khi scene start ───────────────────────────────
/// Các popup có SetActive = false trong Inspector.
/// Unity KHÔNG gọi Awake() cho GameObject đang inactive.
/// Awake() chỉ chạy lần đầu tiên GameObject được SetActive(true).
/// → Trong flow Open(): canvasGroup.alpha = 0 được gọi TRƯỚC base.Open()
///   (tức là TRƯỚC SetActive(true)) → canvasGroup vẫn null → NullReferenceException.
/// → Fix: dùng EnsureCanvasGroup() — lazy init, gọi ở đầu Open() trước khi dùng.
///
/// ─── Cơ chế chống conflict ───────────────────────────────────────────────────
/// 1. Một Sequence duy nhất (currentSequence) tại mọi thời điểm.
///    Kill(false) sequence cũ trước khi tạo mới → không trigger OnComplete cũ.
///
/// 2. Generation counter:
///    - Mỗi Open() tăng generation lên 1.
///    - Close OnComplete snapshot generation → chỉ SetActive(false) nếu khớp.
///    - Open() giữa Close animation → generation tăng → callback stale → bị bỏ qua.
///
/// ─── DOTween lifecycle ───────────────────────────────────────────────────────
/// OnDestroy: Kill(false) — không trigger OnComplete → không có callback stale.
/// Không Kill trong OnDisable — Close animation cần tiếp tục chạy.
/// </summary>
public abstract class PopupWindow : UIWindow
{
    // ── Animation config ──────────────────────────────────────────────────────
    [Header("Popup Animation")]
    [SerializeField] private float openDuration  = 0.25f;
    [SerializeField] private float closeDuration = 0.18f;
    [SerializeField] private float startScale    = 0.7f;
    [SerializeField] private Ease  openEase      = Ease.OutBack;
    [SerializeField] private Ease  closeEase     = Ease.InBack;

    // ── Runtime ───────────────────────────────────────────────────────────────
    private CanvasGroup canvasGroup;
    private Sequence    currentSequence;
    private int         generation;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    protected virtual void OnDestroy()
    {
        currentSequence?.Kill(false);
        currentSequence = null;
    }

    // =========================================================================
    // UIWindow overrides
    // =========================================================================

    public override void Open()
    {
        // Lazy init: chạy trước mọi thứ, an toàn dù Awake chưa chạy
        EnsureCanvasGroup();

        currentSequence?.Kill(false);
        currentSequence = null;

        generation++;

        // Set initial state TRƯỚC base.Open() (trước SetActive(true)) → không flash
        transform.localScale = Vector3.one * startScale;
        canvasGroup.alpha    = 0f;

        base.Open();

        currentSequence = DOTween.Sequence()
            .Join(transform.DOScale(Vector3.one, openDuration).SetEase(openEase))
            .Join(canvasGroup.DOFade(1f, openDuration))
            .SetUpdate(true)
            .OnComplete(() => currentSequence = null);
    }

    public override void Close()
    {
        if (Mode == WindowMode.Persistent) return;

        // Lazy init: Close có thể được gọi trước Open trong edge case
        EnsureCanvasGroup();

        currentSequence?.Kill(false);
        currentSequence = null;

        int closeGen = generation;

        currentSequence = DOTween.Sequence()
            .Join(transform.DOScale(startScale, closeDuration).SetEase(closeEase))
            .Join(canvasGroup.DOFade(0f, closeDuration))
            .SetUpdate(true)
            .OnComplete(() =>
            {
                currentSequence = null;
                if (generation == closeGen)
                    gameObject.SetActive(false);
            });
    }

    // =========================================================================
    // Internal
    // =========================================================================

    /// <summary>
    /// Lazy init CanvasGroup. Gọi ở đầu Open() và Close() thay vì Awake()
    /// vì Awake() không chạy trên inactive GameObject.
    /// </summary>
    private void EnsureCanvasGroup()
    {
        if (canvasGroup != null) return;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
}
