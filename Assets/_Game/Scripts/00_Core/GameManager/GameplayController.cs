using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

/// <summary>
/// Điều phối flow của GameplayScene:
///   LoadLevel → StartGameplay → GameOver → Restart / MainMenu.
/// Gắn vào một ChildGameObject của GameplayContext.
/// </summary>
public class GameplayController : MonoBehaviour
{

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenuScene";

    [Header("Level")]
    [Tooltip("Index level dùng khi lần đầu chơi (chưa có save data).")]
    [SerializeField] private int startLevelIndex = 0;

    // ── Runtime state ─────────────────────────────────────────────────────────
    private int currentLevelIndex;

    #region Inject and auto ref
    private GameManager                 gameManager;
    private SaveManager                 saveManager;
    private UIManager                   uiManager;
    private SceneManagerService         sceneManagerService;
    private LevelManager                levelManager;
    private GameplayObjectiveManager    objectiveManager;

    [Inject]
    private void Construct(
        GameManager gameManager, 
        SaveManager saveManager, 
        UIManager uiManager, 
        SceneManagerService sceneManagerService, 
        LevelManager levelManager,
        GameplayObjectiveManager objectiveManager)
    {
        this.gameManager         = gameManager;
        this.saveManager         = saveManager;
        this.uiManager           = uiManager;
        this.sceneManagerService = sceneManagerService;
        this.levelManager        = levelManager;
        this.objectiveManager    = objectiveManager;
    }

    private HoleController holeController;
    private GameTimer      gameTimer;
    private SwallowHandler swallowHandler;

    private void Awake()
    {
        holeController = FindAnyObjectByType<HoleController>();
        gameTimer      = FindAnyObjectByType<GameTimer>();
        swallowHandler = FindAnyObjectByType<SwallowHandler>();
    }
    #endregion

    private void Start()
    {
        // Đọc level index từ save data.
        // -1 = lần đầu chơi → dùng startLevelIndex từ Inspector.
        currentLevelIndex = saveManager?.PlayerData?.currentLevelIndex ?? -1;
        if (currentLevelIndex < 0)
            currentLevelIndex = startLevelIndex;

        // Load data + spawn level prefab
        levelManager.LoadAndSpawnLevel(currentLevelIndex);

        // Initialize objective system
        objectiveManager.InitializeLevel(levelManager.CurrentLevelDefinition);
        objectiveManager.OnAllObjectivesCompleted += OnLevelWin;

        // Subscribe GameOver events
        gameTimer.OnTimeUp += OnGameOverTimeUp;

        if (swallowHandler != null)
            swallowHandler.OnBombSwallowedWithoutShield += OnGameOverBomb;
        else
            Debug.LogWarning("[GameplayController] SwallowHandler not found — bomb game over will not trigger.");

        StartGameplay();
    }

    private void StartGameplay()
    {
        gameManager.ChangeState(GameState.Gameplay);
        holeController.SetInputEnabled(true);

        // Lấy timeLimit từ LevelDefinition — không dùng giá trị hardcode trong GameTimer Inspector
        float timeLimit = levelManager.CurrentLevelDefinition != null
            ? levelManager.CurrentLevelDefinition.TimeLimit
            : 120f;
        gameTimer.StartTimer(timeLimit);

        // Setup UI với objectives
        GameplayPanel gameplayPanel = uiManager.GetWindow<GameplayPanel>();
        if (gameplayPanel != null && levelManager.CurrentLevelDefinition != null)
        {
            gameplayPanel.SetupObjectives(levelManager.CurrentLevelDefinition.LevelObjectives);
        }
    }

    private void OnLevelWin()
    {
        Debug.Log("[GameplayController] Level Win!");
        gameManager.ChangeState(GameState.Result);
        holeController.SetInputEnabled(false);
        gameTimer.StopTimer();
        levelManager.CleanupLevel();

        // Tăng level và lưu — chỉ tăng khi WIN, không tăng khi GameOver
        AdvanceAndSaveLevel();

        GameWinPopup panel = uiManager.Open<GameWinPopup>();
        int reward = levelManager.CurrentLevelDefinition != null
            ? levelManager.CurrentLevelDefinition.CurrencyReward
            : 0;
        panel?.Setup(reward);
    }

    /// <summary>
    /// Tính level tiếp theo (mod % để quay vòng) và lưu vào save data.
    /// Chỉ gọi khi player WIN — GameOver không gọi hàm này.
    /// </summary>
    private void AdvanceAndSaveLevel()
    {
        if (saveManager?.PlayerData == null) return;

        int nextIndex = levelManager.GetNextLevelIndex(currentLevelIndex);
        saveManager.PlayerData.currentLevelIndex = nextIndex;
        saveManager.Save().Forget();

        Debug.Log($"[GameplayController] Level advanced: {currentLevelIndex} → {nextIndex} (total: {levelManager.TotalLevels}).");
    }

    // Disable Input, StopTimer — hết giờ
    private void OnGameOverTimeUp()
    {
        Debug.Log("[GameplayController] GameOver — TimeUp.");
        TriggerGameOver();
        uiManager.Open<GameOverTimeUpPopup>();
    }

    // Disable Input, StopTimer — nuốt phải bom
    private void OnGameOverBomb()
    {
        Debug.Log("[GameplayController] GameOver — BombExplosion.");
        TriggerGameOver();
        uiManager.Open<GameOverBombPopup>();
    }

    /// <summary>Trạng thái chung khi GameOver bất kể nguyên nhân.</summary>
    private void TriggerGameOver()
    {
        gameManager.ChangeState(GameState.Result);
        holeController.SetInputEnabled(false);
        gameTimer.StopTimer();
        // KHÔNG cleanup level ở đây — player có thể revive và tiếp tục chơi
        // Chỉ cleanup khi thực sự quit hoặc win
    }

    //public async UniTaskVoid RestartAsync()
    //{
    //    gameManager.ChangeState(GameState.Loading);
    //    await sceneManagerService.LoadScene("Gameplay");
    //}

    //public async UniTaskVoid GoToMainMenuAsync()
    //{
    //    gameManager.ChangeState(GameState.Loading);
    //    await sceneManagerService.LoadScene(mainMenuScene);
    //}

#if UNITY_EDITOR
    /// <summary>
    /// Cheat win — chỉ dùng trong Editor, gọi từ EditorCheatController [N].
    /// Bỏ qua objective, mở thẳng GameWinPopup.
    /// </summary>
    public void CheatWin()
    {
        OnLevelWin();
    }
#endif

    private void OnDestroy()
    {
        if (gameTimer != null)
            gameTimer.OnTimeUp -= OnGameOverTimeUp;

        if (swallowHandler != null)
            swallowHandler.OnBombSwallowedWithoutShield -= OnGameOverBomb;

        if (objectiveManager != null)
        {
            objectiveManager.OnAllObjectivesCompleted -= OnLevelWin;
            objectiveManager.Cleanup();
        }
    }
}
