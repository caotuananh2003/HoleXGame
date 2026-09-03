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
    private SaveManager _saveManager;
    private LevelManager _levelManager;
    private GameManager _gameManager;
    private UIManager _uiManager;
    private GameplayObjectiveManager _gameplayObjectiveManager;

    [Header("Level")]
    [SerializeField] private int _startLevelIndex = 0;

    [SerializeField] private HoleController _holeController;
    [SerializeField] private GameTimer _gameTimer;
    [SerializeField] private SwallowHandler _swallowHandler;
    [SerializeField] private HoleSkinApplier _holeSkinApplier;
    [SerializeField] private CameraController _cameraController;

    private int currentLevelIndex;
    private void Awake()
    {
        Instance = this;

        if (_holeController  == null) Debug.LogWarning("[GameplayController] HoleController is null");
        if (_gameTimer       == null) Debug.LogWarning("[GameplayController] GameTimer is null");
        if (_swallowHandler  == null) Debug.LogWarning("[GameplayController] SwallowHandler is null");
        if (_holeSkinApplier == null) Debug.LogWarning("[GameplayController] HoleSkinApplier is null");
        if (_cameraController == null) Debug.LogWarning("[GameplayController] CameraController is null");
    }

    /// <summary>
    /// Resolve tất cả singleton một lần, ngay trước lần đầu tiên cần dùng.
    /// Không thể dùng Start() vì gameplayGroup bắt đầu inactive —
    /// Start() chỉ chạy frame sau SetActive(true), nhưng StartLevel() được gọi cùng frame.
    /// </summary>
    private void ResolveDependencies()
    {
        _saveManager              ??= SaveManager.Instance;
        _levelManager             ??= LevelManager.Instance;
        _gameManager              ??= GameManager.Instance;
        _uiManager                ??= UIManager.Instance;
        _gameplayObjectiveManager ??= GameplayObjectiveManager.Instance;
    }
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        UnsubscribeEvents();
        _gameplayObjectiveManager?.Cleanup();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void StartLevel()
    {
        ResolveDependencies();
        currentLevelIndex = _saveManager?.PlayerData?.currentLevelIndex ?? -1;
        if (currentLevelIndex < 0) currentLevelIndex = _startLevelIndex;
        InitLevel();
    }

    public void RestartLevel()
    {
        ResolveDependencies();
        _levelManager.CleanupLevel();
        UnsubscribeEvents();
        InitLevel();
    }

    public void Cleanup()
    {
        ResolveDependencies();
        _holeController?.SetInputEnabled(false);
        _gameTimer?.StopTimer();
        _levelManager?.CleanupLevel();
        UnsubscribeEvents();
        Debug.Log("[GameplayController] Cleanup done.");
    }

    public void RebornPlayer()
    {
        ResolveDependencies();
        _gameManager.ChangeState(GameState.Gameplay);
        _holeController?.SetInputEnabled(true);
        _gameTimer?.AddTime(0f);
        Debug.Log("[GameplayController] Player hồi sinh — gameplay tiếp tục.");
    }

    public void CheatWin() => OnLevelWin();

    // ── Internal ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Reset toàn bộ gameplay state về ban đầu.
    /// Gọi ở đầu mỗi InitLevel() — cả StartLevel, RestartLevel, và sau Win/Lose.
    /// </summary>
    private void ResetGameplay()
    {
        _holeController?.ResetToInitial();
        _cameraController?.ResetToInitial();
    }

    private void InitLevel()
    {
        ResetGameplay();
        SubscribeEvents();

        _levelManager.LoadAndSpawnLevel(currentLevelIndex);

        var objectiveManager = _gameplayObjectiveManager;
        objectiveManager.InitializeLevel(_levelManager.CurrentLevelDefinition);

        float timeLimit = _levelManager.CurrentLevelDefinition != null
            ? _levelManager.CurrentLevelDefinition.TimeLimit
            : 120f;
        _gameTimer?.StartTimer(timeLimit);

        GameplayPanel gameplayPanel = _uiManager.GetWindow<GameplayPanel>();
        if (gameplayPanel != null && _levelManager.CurrentLevelDefinition != null)
            gameplayPanel.SetupObjectives(_levelManager.CurrentLevelDefinition.LevelObjectives);

        // Áp dụng hole skin sau khi save đã load xong
        _holeSkinApplier.Apply();

        _gameManager.ChangeState(GameState.Gameplay);
        _holeController?.SetInputEnabled(true);

        Debug.Log($"[GameplayController] Level {currentLevelIndex} started.");
    }

    private void SubscribeEvents()
    {
        UnsubscribeEvents();

        var objectiveManager = _gameplayObjectiveManager;
        if (objectiveManager != null)
            objectiveManager.OnAllObjectivesCompleted += OnLevelWin;

        if (_gameTimer != null)
            _gameTimer.OnTimeUp += OnGameOverTimeUp;

        if (_swallowHandler != null)
            _swallowHandler.OnBombSwallowedWithoutShield += OnGameOverBomb;
        else
            Debug.LogWarning("[GameplayController] SwallowHandler not found.");
    }

    private void UnsubscribeEvents()
    {
        var objectiveManager = _gameplayObjectiveManager;
        if (objectiveManager != null)
            objectiveManager.OnAllObjectivesCompleted -= OnLevelWin;

        if (_gameTimer != null)    _gameTimer.OnTimeUp -= OnGameOverTimeUp;
        if (_swallowHandler != null) _swallowHandler.OnBombSwallowedWithoutShield -= OnGameOverBomb;
    }

    private void OnLevelWin()
    {
        Debug.Log("[GameplayController] Level Win!");

        _gameManager.ChangeState(GameState.Result);
        _holeController?.SetInputEnabled(false);
        _gameTimer?.StopTimer();
        _levelManager.CleanupLevel();
        UnsubscribeEvents();

        AdvanceAndSaveLevel();

        // Kiểm tra unlock item ngay sau khi level tăng.
        // Lấy ItemDatabase từ GameplayPanel — tránh duplicate field.
        GameplayPanel gameplayPanel = _uiManager.GetWindow<GameplayPanel>();
        ItemManager.Instance?.CheckAndUnlockItems(gameplayPanel?.ItemDatabase);

        GameWinPopup panel = _uiManager.Open<GameWinPopup>();
        int reward = _levelManager.CurrentLevelDefinition?.CurrencyReward ?? 0;
        panel?.Setup(reward);
    }

    private void OnGameOverTimeUp()
    {
        Debug.Log("[GameplayController] GameOver — TimeUp.");
        TriggerGameOver();
        _uiManager.Open<GameOverTimeUpPopup>();
    }

    private void OnGameOverBomb()
    {
        Debug.Log("[GameplayController] GameOver — Bomb.");
        TriggerGameOver();
        _uiManager.Open<GameOverBombPopup>();
    }

    private void TriggerGameOver()
    {
        _gameManager.ChangeState(GameState.Result);
        _holeController?.SetInputEnabled(false);
        _gameTimer?.StopTimer();
    }

    private void AdvanceAndSaveLevel()
    {
        if (_saveManager?.PlayerData == null) return;

        int nextIndex = _levelManager.GetNextLevelIndex(currentLevelIndex);
        _saveManager.PlayerData.currentLevelIndex = nextIndex;
        _saveManager.Save().Forget();

        Debug.Log($"[GameplayController] Level advanced: {currentLevelIndex} → {nextIndex}.");
    }

}
