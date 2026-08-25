using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// Màn hình game over khi nuốt phải bom.
/// Kế thừa UIWindow trực tiếp (không qua PopupWindow) vì có chuỗi animation
/// nội bộ riêng do GameOverBombSequencePlayer quản lý.
///
/// Flow tổng quát:
///   GameplayController gọi Open()
///   → sequencePlayer.PlayAsync()   — title → description → phase1 (anim + text)
///   → Buttons enable, người dùng tương tác:
///
///   QuitButton lần 1:
///       → phase1Root disable
///       → sequencePlayer.PlayPhase2Async() — phase2 (anim + text)
///       → Buttons enable lại
///
///   QuitButton lần 2:
///       → đóng GameOverBombPopup → mở TryAgainPopup
///
///   RebornButton (bất cứ lúc nào buttons đang enable):
///       → trừ 900 vàng → đóng popup → resume gameplay
///
/// Hierarchy gợi ý:
///   GameOverBombPopup
///   ├── GameOverBombSequencePlayer
///   ├── Title                       ← TMP_Text
///   ├── Description                 ← TMP_Text
///   ├── Phase1                      ← GameObject (enable/disable tự động)
///   │   ├── AnimationPhase1         ← Animator + CanvasGroup
///   │   └── DescriptionTextPhase1   ← TMP_Text
///   ├── Phase2                      ← GameObject (enable/disable tự động)
///   │   ├── AnimationPhase2         ← Animator + CanvasGroup
///   │   └── DescriptionTextPhase2   ← TMP_Text
///   ├── RebornButton
///   └── QuitButton
/// </summary>
public class GameOverBombPopup : UIWindow
{
    private const int RebornCost = 900;

    [Header("Sequence")]
    [SerializeField] private GameOverBombSequencePlayer sequencePlayer;

    [Header("Phase GameObjects")]
    [SerializeField] private GameObject phase1Root;
    [SerializeField] private GameObject phase2Root;

    [Header("Buttons")]
    [SerializeField] private Button rebornButton;
    [SerializeField] private Button quitButton;

    // ── Dependencies ──────────────────────────────────────────────────────────
    private SaveManager        saveManager;
    private GameplayController gameplayController;

    // ── State ─────────────────────────────────────────────────────────────────
    /// <summary>
    /// Số lần người dùng đã bấm QuitButton.
    ///   0 = chưa bấm (phase1 đang hiện)
    ///   1 = bấm lần 1 → chuyển sang phase2
    ///   2 = bấm lần 2 → đóng popup, mở TryAgainPopup
    /// </summary>
    private int quitClickCount;

    [Inject]
    private void Construct(SaveManager saveManager)
    {
        this.saveManager = saveManager;
    }

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Start()
    {
        gameplayController = FindAnyObjectByType<GameplayController>();

        Validate();

        SetButtonsInteractable(false);

        if (rebornButton != null) rebornButton.onClick.AddListener(OnRebornClicked);
        if (quitButton   != null) quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnDestroy()
    {
        if (rebornButton != null) rebornButton.onClick.RemoveListener(OnRebornClicked);
        if (quitButton   != null) quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    // =========================================================================
    // UIWindow override
    // =========================================================================

    public override void Open()
    {
        base.Open();

        quitClickCount = 0;

        UIManager?.PlaySFX(AudioID.SFX.UiLose);

        SetButtonsInteractable(false);
        PlaySequenceAsync().Forget();
    }

    // =========================================================================
    // Sequence
    // =========================================================================

    /// <summary>
    /// Chạy chuỗi mở đầu: title → description → phase1 (anim + text) → buttons bounce in.
    /// </summary>
    private async UniTaskVoid PlaySequenceAsync()
    {
        if (sequencePlayer != null)
            await sequencePlayer.PlayAsync();

        await PlayButtonsAndEnableAsync();
    }

    /// <summary>
    /// Chạy phase 2: disable phase1 → phase2 (anim + text) → buttons bounce in.
    /// </summary>
    private async UniTaskVoid PlayPhase2Async()
    {
        if (phase1Root != null) phase1Root.SetActive(false);

        if (sequencePlayer != null)
            await sequencePlayer.PlayPhase2Async();

        await PlayButtonsAndEnableAsync();
    }

    /// <summary>
    /// Bounce in buttons rồi enable tương tác.
    /// Ẩn rebornButton nếu không đủ tiền.
    /// </summary>
    private async UniTask PlayButtonsAndEnableAsync()
    {
        if (sequencePlayer != null)
            await sequencePlayer.PlayButtonsAsync();

        SetButtonsInteractable(true);
        RefreshRebornButtonState();
    }

    // =========================================================================
    // Button helpers
    // =========================================================================

    private void SetButtonsInteractable(bool interactable)
    {
        if (rebornButton != null) rebornButton.interactable = interactable;
        if (quitButton   != null) quitButton.interactable   = interactable;
    }

    /// <summary>
    /// Disable rebornButton nếu currency nhỏ hơn RebornCost.
    /// Gọi sau khi sequence xong để player thấy rõ button trước khi bị disable.
    /// </summary>
    private void RefreshRebornButtonState()
    {
        if (rebornButton == null) return;

        bool canAfford = saveManager?.PlayerData != null
                      && saveManager.PlayerData.currency >= RebornCost;

        rebornButton.interactable = canAfford;
    }

    // =========================================================================
    // Handlers
    // =========================================================================

    private void OnQuitClicked()
    {
        quitClickCount++;

        if (quitClickCount == 1)
        {
            // Lần 1: chuyển sang phase 2
            SetButtonsInteractable(false);
            PlayPhase2Async().Forget();
        }
        else
        {
            // Lần 2: đóng popup này, mở TryAgainPopup
            UIManager?.Close<GameOverBombPopup>();
            UIManager?.Open<TryAgainPopup>();
        }
    }

    private void OnRebornClicked()
    {
        if (saveManager?.PlayerData == null)
        {
            Debug.LogError("[GameOverBombPopup] SaveManager.PlayerData is null.");
            return;
        }

        if (saveManager.PlayerData.currency < RebornCost)
        {
            Debug.LogWarning("[GameOverBombPopup] Không đủ vàng để hồi sinh.");
            return;
        }

        saveManager.PlayerData.currency -= RebornCost;
        saveManager.Save().Forget();

        Debug.Log($"[GameOverBombPopup] Hồi sinh — trừ {RebornCost} vàng. Còn: {saveManager.PlayerData.currency}");

        UIManager?.Close<GameOverBombPopup>();
        gameplayController?.RebornPlayer();
    }

    // =========================================================================
    // Validate
    // =========================================================================

    private void Validate()
    {
        if (sequencePlayer     == null) Debug.LogError("[GameOverBombPopup] sequencePlayer is not assigned.",    this);
        if (phase1Root         == null) Debug.LogError("[GameOverBombPopup] phase1Root is not assigned.",        this);
        if (phase2Root         == null) Debug.LogError("[GameOverBombPopup] phase2Root is not assigned.",        this);
        if (rebornButton       == null) Debug.LogError("[GameOverBombPopup] rebornButton is not assigned.",      this);
        if (quitButton         == null) Debug.LogError("[GameOverBombPopup] quitButton is not assigned.",        this);
        if (saveManager        == null) Debug.LogError("[GameOverBombPopup] saveManager is null. Check VContainer.", this);
        if (gameplayController == null) Debug.LogWarning("[GameOverBombPopup] GameplayController not found in scene.", this);
    }
}
