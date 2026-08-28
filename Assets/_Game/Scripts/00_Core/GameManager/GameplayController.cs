using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Điều phối flow Gameplay trong single-scene.
/// StartLevel() gọi từ TransitionService, Cleanup() khi về MainMenu,
/// RestartLevel() từ TryAgainPopup.
/// </summary>
public class GameplayController : MonoBehaviour
{
    public static GameplayController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        holeController = GetComponentInChildren<HoleController>(true);
        gameTimer      = GetComponentInChildren<GameTimer>(true);
        swallowHandler = GetComponentInChildren<SwallowHandler>(true);

        if (holeController == null) holeController = FindAnyObjectByType<HoleController>(FindObjectsInactive.Include);
        if (gameTimer      == null) gameTimer      = FindAnyObjectByType<GameTimer>(FindObjectsInactive.Include);
        if (swallowHandler == null) swallowHandler = FindAnyObjectByType<SwallowHandler>(FindObjectsInactive.Include);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        UnsubscribeEvents();
        GameplayObjectiveManager.Instance?.Cleanup();
    }
    [Header("Level")]
    [SerializeField] private int startLevelIndex = 0;

    private HoleController holeController;
    private GameTimer      gameTimer;
    private SwallowHandler swallowHandler;

    private int  currentLevelIndex;
    private bool isInitialized;

    // ── Public API ────────────────────────────────────────────────────────────

    public void StartLevel()
    {
        currentLevelIndex = SaveManager.Instance?.PlayerData?.currentLevelIndex ?? -1;
        if (currentLevelIndex < 0) currentLevelIndex = startLevelIndex;
        InitLevel();
    }

    public void RestartLevel()
    {
        LevelManager.Instance.CleanupLevel();
        UnsubscribeEvents();
        InitLevel();
    }

    public void Cleanup()
    {
        holeController?.SetInputEnabled(false);
        gameTimer?.StopTimer();
        LevelManager.Instance?.CleanupLevel();
        UnsubscribeEvents();
        isInitialized = false;
        Debug.Log("[GameplayController] Cleanup done.");
    }

    public void RebornPlayer()
    {
        GameManager.Instance.ChangeState(GameState.Gameplay);
        holeController?.SetInputEnabled(true);
        gameTimer?.AddTime(0f);
        Debug.Log("[GameplayController] Player hồi sinh — gameplay tiếp tục.");
    }

    public void CheatWin() => OnLevelWin();

    // ── Internal ──────────────────────────────────────────────────────────────

    private void InitLevel()
    {
        SubscribeEvents();

        LevelManager.Instance.LoadAndSpawnLevel(currentLevelIndex);

        var objectiveManager = GameplayObjectiveManager.Instance;
        objectiveManager.InitializeLevel(LevelManager.Instance.CurrentLevelDefinition);

        float timeLimit = LevelManager.Instance.CurrentLevelDefinition != null
            ? LevelManager.Instance.CurrentLevelDefinition.TimeLimit
            : 120f;
        gameTimer?.StartTimer(timeLimit);

        GameplayPanel gameplayPanel = UIManager.Instance.GetWindow<GameplayPanel>();
        if (gameplayPanel != null && LevelManager.Instance.CurrentLevelDefinition != null)
            gameplayPanel.SetupObjectives(LevelManager.Instance.CurrentLevelDefinition.LevelObjectives);

        GameManager.Instance.ChangeState(GameState.Gameplay);
        holeController?.SetInputEnabled(true);

        isInitialized = true;
        Debug.Log($"[GameplayController] Level {currentLevelIndex} started.");
    }

    private void SubscribeEvents()
    {
        UnsubscribeEvents();

        var objectiveManager = GameplayObjectiveManager.Instance;
        if (objectiveManager != null)
            objectiveManager.OnAllObjectivesCompleted += OnLevelWin;

        if (gameTimer != null)
            gameTimer.OnTimeUp += OnGameOverTimeUp;

        if (swallowHandler != null)
            swallowHandler.OnBombSwallowedWithoutShield += OnGameOverBomb;
        else
            Debug.LogWarning("[GameplayController] SwallowHandler not found.");
    }

    private void UnsubscribeEvents()
    {
        var objectiveManager = GameplayObjectiveManager.Instance;
        if (objectiveManager != null)
            objectiveManager.OnAllObjectivesCompleted -= OnLevelWin;

        if (gameTimer != null)    gameTimer.OnTimeUp -= OnGameOverTimeUp;
        if (swallowHandler != null) swallowHandler.OnBombSwallowedWithoutShield -= OnGameOverBomb;
    }

    private void OnLevelWin()
    {
        Debug.Log("[GameplayController] Level Win!");

        GameManager.Instance.ChangeState(GameState.Result);
        holeController?.SetInputEnabled(false);
        gameTimer?.StopTimer();
        LevelManager.Instance.CleanupLevel();
        UnsubscribeEvents();

        AdvanceAndSaveLevel();

        GameWinPopup panel = UIManager.Instance.Open<GameWinPopup>();
        int reward = LevelManager.Instance.CurrentLevelDefinition?.CurrencyReward ?? 0;
        panel?.Setup(reward);
    }

    private void OnGameOverTimeUp()
    {
        Debug.Log("[GameplayController] GameOver — TimeUp.");
        TriggerGameOver();
        UIManager.Instance.Open<GameOverTimeUpPopup>();
    }

    private void OnGameOverBomb()
    {
        Debug.Log("[GameplayController] GameOver — Bomb.");
        TriggerGameOver();
        UIManager.Instance.Open<GameOverBombPopup>();
    }

    private void TriggerGameOver()
    {
        GameManager.Instance.ChangeState(GameState.Result);
        holeController?.SetInputEnabled(false);
        gameTimer?.StopTimer();
    }

    private void AdvanceAndSaveLevel()
    {
        if (SaveManager.Instance?.PlayerData == null) return;

        int nextIndex = LevelManager.Instance.GetNextLevelIndex(currentLevelIndex);
        SaveManager.Instance.PlayerData.currentLevelIndex = nextIndex;
        SaveManager.Instance.Save().Forget();

        Debug.Log($"[GameplayController] Level advanced: {currentLevelIndex} → {nextIndex}.");
    }

}
