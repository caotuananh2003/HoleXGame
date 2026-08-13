using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// Popup kết quả sau khi thắng level.
/// Mode = Popup trong Inspector.
///
/// resumeButton   → +currencyReward (lấy từ LevelDefinition), load MainMenuScene.
/// watchAdsButton → +currencyReward * 2, log "WatchAds để thêm currency", load MainMenuScene.
///
/// Gọi Setup(currencyReward) từ GameplayController trước hoặc sau Open().
/// </summary>
public class GameWinPopup : UIWindow
{
    [Header("Buttons")]
    [SerializeField] private Button   resumeButton;
    [SerializeField] private Button   watchAdsButton;

    [Header("Reward Display")]
    [SerializeField] private TMP_Text currencyRewardText; // Hiển thị "x{reward}" từ LevelDefinition

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
    // Unity lifecycle
    // =========================================================================

    private void Start()
    {
        if (resumeButton   != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (watchAdsButton != null) watchAdsButton.onClick.AddListener(OnWatchAdsClicked);

        if (resumeButton   == null) Debug.LogWarning("[GameWinPopup] resumeButton is not assigned.");
        if (watchAdsButton == null) Debug.LogWarning("[GameWinPopup] watchAdsButton is not assigned.");
    }

    private void OnDestroy()
    {
        if (resumeButton   != null) resumeButton.onClick.RemoveListener(OnResumeClicked);
        if (watchAdsButton != null) watchAdsButton.onClick.RemoveListener(OnWatchAdsClicked);
    }

    // =========================================================================
    // Handlers
    // =========================================================================

    private void OnResumeClicked()
    {
        AddCurrency(currencyReward);
        GoToMainMenu();
    }

    private void OnWatchAdsClicked()
    {
        Debug.Log("WatchAds để thêm currency");
        AddCurrency(currencyReward * 2);
        GoToMainMenu();
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private void AddCurrency(int amount)
    {
        if (saveManager?.PlayerData == null)
        {
            Debug.LogWarning("[GameWinPopup] SaveManager.Data is null — không thể cộng currency.");
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
}
