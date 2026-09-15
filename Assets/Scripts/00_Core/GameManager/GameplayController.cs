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


    [Header("Level")]
    [SerializeField] private int _startLevelIndex = 0;

    [SerializeField] private HoleController _holeController;
    [SerializeField] private GameTimer _gameTimer;
    [SerializeField] private KillerCollider      _killerCollider;
    [SerializeField] private HoleSkinApplier     _holeSkinApplier;
    [SerializeField] private MapThemeApplier     _mapThemeApplier;
    [SerializeField] private CameraController _cameraController;

    private int currentLevelIndex;
    private void Awake()
    {
        Instance = this;

        if (_holeController  == null) Debug.LogWarning("[GameplayController] HoleController is null");
        if (_gameTimer       == null) Debug.LogWarning("[GameplayController] GameTimer is null");
        if (_killerCollider  == null) Debug.LogWarning("[GameplayController] KillerCollider is null");
        if (_holeSkinApplier == null) Debug.LogWarning("[GameplayController] HoleSkinApplier is null");
        if (_mapThemeApplier == null) Debug.LogWarning("[GameplayController] MapThemeApplier is null");
        if (_cameraController == null) Debug.LogWarning("[GameplayController] CameraController is null");
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        UnsubscribeEvents();
        GameplayObjectiveManager.Instance?.Cleanup();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void StartLevel()
    {
        currentLevelIndex = SaveManager.Instance?.PlayerData?.currentLevelIndex ?? -1;
        if (currentLevelIndex < 0) currentLevelIndex = _startLevelIndex;
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
        Time.timeScale = 1f;
        _holeController?.SetInputEnabled(false);
        _gameTimer?.StopTimer();
        LevelManager.Instance?.CleanupLevel();
        UnsubscribeEvents();
        Debug.Log("[GameplayController] Cleanup done.");
    }

    public void RebornPlayer()
    {
        GameManager.Instance.ChangeState(GameState.Gameplay);
        _holeController?.SetInputEnabled(true);
        _gameTimer?.AddTime(0f);
        Debug.Log("[GameplayController] Player hồi sinh — gameplay tiếp tục.");
    }

    public void CheatWin() => OnLevelWin();

    /// <summary>
    /// Pause game: timeScale = 0, disable input, pause timer, ChangeState Pause.
    /// Gọi từ SettingPopup hoặc bất kỳ nơi nào cần pause.
    /// </summary>
    public void Pause()
    {
        if (GameManager.Instance.CurrentState != GameState.Gameplay) return;

        GameManager.Instance.ChangeState(GameState.Pause);
        _holeController?.SetInputEnabled(false);
        _gameTimer?.Pause();

        Debug.Log("[GameplayController] Game paused.");
    }

    /// <summary>
    /// Unpause game: timeScale = 1, enable input, resume timer, ChangeState Gameplay.
    /// </summary>
    public void Unpause()
    {
        if (GameManager.Instance.CurrentState != GameState.Pause) return;

        GameManager.Instance.ChangeState(GameState.Gameplay);
        Time.timeScale = 1f;
        _holeController?.SetInputEnabled(true);
        _gameTimer?.Resume();

        Debug.Log("[GameplayController] Game unpaused.");
    }

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

        LevelManager.Instance.LoadAndSpawnLevel(currentLevelIndex);

        var objectiveManager = GameplayObjectiveManager.Instance;
        objectiveManager.InitializeLevel(LevelManager.Instance.CurrentLevelDefinition);

        float timeLimit = LevelManager.Instance.CurrentLevelDefinition != null
            ? LevelManager.Instance.CurrentLevelDefinition.TimeLimit
            : 120f;
        _gameTimer?.StartTimer(timeLimit);

        GameplayPanel gameplayPanel = UIManager.Instance.GetWindow<GameplayPanel>();
        if (gameplayPanel != null && LevelManager.Instance.CurrentLevelDefinition != null)
            gameplayPanel.SetupObjectives(LevelManager.Instance.CurrentLevelDefinition.LevelObjectives);

        // Áp dụng hole skin và map theme sau khi save đã load xong
        _holeSkinApplier.Apply();
        _mapThemeApplier?.Apply();

        GameManager.Instance.ChangeState(GameState.Gameplay);
        _holeController?.SetInputEnabled(true);

        Debug.Log($"[GameplayController] Level {currentLevelIndex} started.");
    }

    private void SubscribeEvents()
    {
        UnsubscribeEvents();

        var objectiveManager = GameplayObjectiveManager.Instance;
        if (objectiveManager != null)
            objectiveManager.OnAllObjectivesCompleted += OnLevelWin;

        if (_gameTimer != null)
            _gameTimer.OnTimeUp += OnGameOverTimeUp;
    }

    private void UnsubscribeEvents()
    {
        var objectiveManager = GameplayObjectiveManager.Instance;
        if (objectiveManager != null)
            objectiveManager.OnAllObjectivesCompleted -= OnLevelWin;

        if (_gameTimer != null)
            _gameTimer.OnTimeUp -= OnGameOverTimeUp;
    }

    private void OnLevelWin()
    {
        Debug.Log("[GameplayController] Level Win!");

        GameManager.Instance.ChangeState(GameState.Result);
        _holeController?.SetInputEnabled(false);
        _gameTimer?.StopTimer();
        UnsubscribeEvents();

        AdvanceAndSaveLevel();

        GameplayPanel gameplayPanel = UIManager.Instance.GetWindow<GameplayPanel>();
        ItemManager.Instance?.CheckAndUnlockItems(gameplayPanel?.ItemDatabase);

        // Đợi fly animation xong rồi mới mở popup
        WaitThenOpenWinPopupAsync().Forget();
    }

    private async UniTaskVoid WaitThenOpenWinPopupAsync()
    {
        var objectiveManager = GameplayObjectiveManager.Instance;
        if (objectiveManager != null)
            await objectiveManager.WaitForAllAnimationsAsync();

        LevelManager.Instance.CleanupLevel();

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

    /// <summary>
    /// Gọi từ KillerCollider ngay khi phát hiện bomb (trước khi particle play).
    /// Disable input và stop timer ngay lập tức.
    /// </summary>
    public void OnBombHit()
    {
        _holeController?.SetInputEnabled(false);
        _gameTimer?.StopTimer();
        Debug.Log("[GameplayController] Bomb hit — input disabled.");
    }

    /// <summary>
    /// Gọi từ KillerCollider sau khi explosion particle chạy xong.
    /// Chuyển state và mở popup GameOver.
    /// </summary>
    public void OnBombExplosionFinished()
    {
        Debug.Log("[GameplayController] Bomb explosion finished — GameOver.");
        GameManager.Instance.ChangeState(GameState.Result);
        UIManager.Instance.Open<GameOverBombPopup>();
    }

    private void TriggerGameOver()
    {
        GameManager.Instance.ChangeState(GameState.Result);
        _holeController?.SetInputEnabled(false);
        _gameTimer?.StopTimer();
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
