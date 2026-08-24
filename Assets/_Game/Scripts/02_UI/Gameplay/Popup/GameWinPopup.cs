using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// Màn hình kết quả thắng level.
/// Kế thừa UIWindow trực tiếp (không qua PopupWindow) vì có chuỗi animation
/// nội bộ riêng do WinSequencePlayer quản lý.
///
/// Flow:
///   GameplayController gọi Open() → SetActive(true)
///   → WinSequencePlayer.PlayAsync() chạy Phase1 rồi Phase2
///   → Buttons được enable, người dùng tương tác
///
/// Hierarchy gợi ý:
///   GameWinPopup
///   ├── WinSequencePlayer       ← component WinSequencePlayer.cs
///   ├── Phase1Group
///   │   ├── Particle
///   │   ├── Ribbon
///   │   ├── Stars (off + spawn points)
///   │   ├── MainAnimation
///   │   └── Horns (Left / Right)
///   └── Phase2Group
///       ├── WellDone
///       ├── ProgressBar
///       ├── RewardGroup
///       └── Buttons (Continue / WatchAds)
/// </summary>
public class GameWinPopup : UIWindow
{
    [Header("Sequence")]
    [SerializeField] private WinSequencePlayer winSequencePlayer;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button watchAdsButton;

    [Header("Reward Display")]
    [SerializeField] private TMP_Text currencyRewardText;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenuScene";

    // ── Dependencies ──────────────────────────────────────────────────────────
    private SaveManager         saveManager;
    private SceneManagerService sceneManagerService;
    private GameManager         gameManager;

    [Inject]
    private void Construct(
        SaveManager         saveManager,
        SceneManagerService sceneManagerService,
        GameManager         gameManager)
    {
        this.saveManager         = saveManager;
        this.sceneManagerService = sceneManagerService;
        this.gameManager         = gameManager;
    }

    // ── Runtime state ─────────────────────────────────────────────────────────
    private int currencyReward;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Start()
    {
        Validate();

        // Buttons bị tắt tương tác cho đến khi Phase 2 xong
        SetButtonsInteractable(false);

        if (resumeButton   != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (watchAdsButton != null) watchAdsButton.onClick.AddListener(OnWatchAdsClicked);
    }

    private void OnDestroy()
    {
        if (resumeButton   != null) resumeButton.onClick.RemoveListener(OnResumeClicked);
        if (watchAdsButton != null) watchAdsButton.onClick.RemoveListener(OnWatchAdsClicked);

        winSequencePlayer?.Cleanup();
    }

    // =========================================================================
    // UIWindow override
    // =========================================================================

    public override void Open()
    {
        base.Open(); // SetActive(true)

        UIManager?.PlaySFX(AudioID.SFX.UiWin);

        SetButtonsInteractable(false);
        PlaySequenceAsync().Forget();
    }

    public override void Close()
    {
        winSequencePlayer?.Cleanup();
        base.Close(); // SetActive(false)
    }

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>
    /// Truyền currency reward từ LevelDefinition.
    /// Gọi từ GameplayController sau Open().
    /// </summary>
    public void Setup(int reward)
    {
        currencyReward = reward;

        if (currencyRewardText != null)
            currencyRewardText.text = $"x{reward}";
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private async UniTaskVoid PlaySequenceAsync()
    {
        if (winSequencePlayer != null)
            await winSequencePlayer.PlayAsync();

        // Phase 2 xong → cho phép tương tác
        SetButtonsInteractable(true);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (resumeButton   != null) resumeButton.interactable   = interactable;
        if (watchAdsButton != null) watchAdsButton.interactable = interactable;
    }

    // =========================================================================
    // Button handlers
    // =========================================================================

    private void OnResumeClicked()
    {
        AddCurrency(currencyReward);
        GoToMainMenu();
    }

    private void OnWatchAdsClicked()
    {
        Debug.Log("[GameWinPopup] WatchAds — nhân đôi currency.");
        AddCurrency(currencyReward * 2);
        GoToMainMenu();
    }

    private void AddCurrency(int amount)
    {
        if (saveManager?.PlayerData == null)
        {
            Debug.LogError("[GameWinPopup] SaveManager.PlayerData is null — không thể cộng currency.");
            return;
        }

        saveManager.PlayerData.currency += amount;
        saveManager.Save().Forget();

        Debug.Log($"[GameWinPopup] +{amount} currency. Tổng: {saveManager.PlayerData.currency}");
    }

    private void GoToMainMenu()
    {
        gameManager?.ChangeState(GameState.Loading);
        sceneManagerService?.LoadScene(mainMenuScene).Forget();
    }

    // =========================================================================
    // Validate
    // =========================================================================

    private void Validate()
    {
        if (winSequencePlayer  == null) Debug.LogError("[GameWinPopup] winSequencePlayer is not assigned.",  this);
        if (resumeButton       == null) Debug.LogError("[GameWinPopup] resumeButton is not assigned.",       this);
        if (watchAdsButton     == null) Debug.LogError("[GameWinPopup] watchAdsButton is not assigned.",     this);
        if (currencyRewardText == null) Debug.LogError("[GameWinPopup] currencyRewardText is not assigned.", this);
        if (saveManager        == null) Debug.LogError("[GameWinPopup] saveManager is null. Check VContainer.", this);
        if (sceneManagerService == null) Debug.LogError("[GameWinPopup] sceneManagerService is null. Check VContainer.", this);
        if (gameManager        == null) Debug.LogError("[GameWinPopup] gameManager is null. Check VContainer.", this);
    }
}
