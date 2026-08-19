using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// Popup GameOver — hết giờ (TimeUp).
/// Mode = Popup trong Inspector.
///
/// Người chơi có tối đa 3 lần hồi sinh, mỗi lần +20 giây.
///   - Lần 1 hồi sinh → clock1 bật
///   - Lần 2 hồi sinh → clock2 bật
///   - Lần 3 hồi sinh → clock3 bật
///   - Hết 3 lần       → ẩn 2 nút hồi sinh, chỉ còn Quit
///
/// Khi Quit: trừ 1 life → về MainMenuScene.
/// </summary>
public class GameOverTimeUpPopup : UIWindow
{
    private const int   MaxRevives         = 3;
    private const float AddSeconds         = 20f;
    private const int   ReviveCurrencyCost = 900;

    [Header("Revive Buttons")]
    [SerializeField] private Button addTimeByAdsButton;
    [SerializeField] private Button addTimeByCurrencyButton;

    [Header("Quit")]
    [SerializeField] private Button quitButton;

    [Header("Clock Indicators (+20s / +40s / +60s)")]
    [SerializeField] private ClockIndicator clock1;
    [SerializeField] private ClockIndicator clock2;
    [SerializeField] private ClockIndicator clock3;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenuScene";

    // ── Dependencies ──────────────────────────────────────────────────────────
    private GameTimer           gameTimer;
    private HoleController      holeController;
    private SceneManagerService sceneManagerService;
    private GameManager         gameManager;
    private SaveManager         saveManager;
    private LevelManager        levelManager;

    [Inject]
    private void Construct(
        GameTimer           gameTimer,
        HoleController      holeController,
        SceneManagerService sceneManagerService,
        GameManager         gameManager,
        SaveManager         saveManager,
        LevelManager        levelManager)
    {
        this.gameTimer           = gameTimer;
        this.holeController      = holeController;
        this.sceneManagerService = sceneManagerService;
        this.gameManager         = gameManager;
        this.saveManager         = saveManager;
        this.levelManager        = levelManager;
    }

    // ── Runtime state ─────────────────────────────────────────────────────────
    private int reviveCount;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Start()
    {
        if (addTimeByAdsButton      != null) addTimeByAdsButton.onClick.AddListener(OnAddTimeByAdsClicked);
        if (addTimeByCurrencyButton != null) addTimeByCurrencyButton.onClick.AddListener(OnAddTimeByCurrencyClicked);
        if (quitButton              != null) quitButton.onClick.AddListener(OnQuitClicked);

        ValidateRefs();
    }

    private void OnDestroy()
    {
        if (addTimeByAdsButton      != null) addTimeByAdsButton.onClick.RemoveListener(OnAddTimeByAdsClicked);
        if (addTimeByCurrencyButton != null) addTimeByCurrencyButton.onClick.RemoveListener(OnAddTimeByCurrencyClicked);
        if (quitButton              != null) quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    // =========================================================================
    // UIWindow override — reset state mỗi lần mở
    // =========================================================================

    public override void Open()
    {
        base.Open();
        UIManager?.PlaySFX(AudioID.SFX.UiLose);
        RefreshUI();
    }

    // =========================================================================
    // Handlers
    // =========================================================================

    private void OnAddTimeByAdsClicked()
    {
        Debug.Log("[GameOverTimeUpPopup] Hồi sinh bằng Ads.");
        Revive();
    }

    private void OnAddTimeByCurrencyClicked()
    {
        if (saveManager?.PlayerData == null)
        {
            Debug.LogWarning("[GameOverTimeUpPopup] SaveManager.PlayerData is null.");
            return;
        }

        if (saveManager.PlayerData.currency < ReviveCurrencyCost)
        {
            Debug.Log($"[GameOverTimeUpPopup] Không đủ {ReviveCurrencyCost} currency để hồi sinh.");
            return;
        }

        saveManager.PlayerData.currency -= ReviveCurrencyCost;
        saveManager.Save().Forget();

        Debug.Log($"[GameOverTimeUpPopup] Hồi sinh bằng currency. Còn lại: {saveManager.PlayerData.currency}");
        Revive();
    }

    private void OnQuitClicked()
    {
        Debug.Log("[GameOverTimeUpPopup] Quit — cleanup level, về MainMenu.");

        // Cleanup level trước khi chuyển scene
        levelManager?.CleanupLevel();

        gameManager?.ChangeState(GameState.Loading);
        sceneManagerService?.LoadScene(mainMenuScene).Forget();
    }

    // =========================================================================
    // Revive logic
    // =========================================================================

    private void Revive()
    {
        if (reviveCount >= MaxRevives) return;

        reviveCount++;
        MarkClockUsed(reviveCount);

        gameTimer?.AddTime(AddSeconds);
        holeController?.SetInputEnabled(true);
        gameManager?.ChangeState(GameState.Gameplay);

        UIManager?.Close<GameOverTimeUpPopup>();
    }

    private void MarkClockUsed(int revive)
    {
        switch (revive)
        {
            case 1: clock1?.SetUsed(true); break;
            case 2: clock2?.SetUsed(true); break;
            case 3: clock3?.SetUsed(true); break;
        }
    }

    // =========================================================================
    // UI refresh
    // =========================================================================

    private void RefreshUI()
    {
        bool canRevive = reviveCount < MaxRevives;

        if (addTimeByAdsButton      != null) addTimeByAdsButton.gameObject.SetActive(canRevive);
        if (addTimeByCurrencyButton != null) addTimeByCurrencyButton.gameObject.SetActive(canRevive);
    }

    // =========================================================================
    // Validation
    // =========================================================================

    private void ValidateRefs()
    {
        if (addTimeByAdsButton      == null) Debug.LogWarning("[GameOverTimeUpPopup] addTimeByAdsButton is not assigned.");
        if (addTimeByCurrencyButton == null) Debug.LogWarning("[GameOverTimeUpPopup] addTimeByCurrencyButton is not assigned.");
        if (quitButton              == null) Debug.LogWarning("[GameOverTimeUpPopup] quitButton is not assigned.");
        if (clock1                  == null) Debug.LogWarning("[GameOverTimeUpPopup] clock1 is not assigned.");
        if (clock2                  == null) Debug.LogWarning("[GameOverTimeUpPopup] clock2 is not assigned.");
        if (clock3                  == null) Debug.LogWarning("[GameOverTimeUpPopup] clock3 is not assigned.");
        if (gameTimer               == null) Debug.LogWarning("[GameOverTimeUpPopup] gameTimer is null — injection failed?");
        if (holeController          == null) Debug.LogWarning("[GameOverTimeUpPopup] holeController is null.");
    }
}
