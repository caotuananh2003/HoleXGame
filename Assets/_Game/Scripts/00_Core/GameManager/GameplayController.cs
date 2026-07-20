using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

/// <summary>
/// Điều phối flow của GameplayScene:
///   LoadLevel → StartGameplay → GameOver → Restart / MainMenu.
/// Gắn vào một GameObject trong GameplayScene.
/// Wire HoleController, GameTimer, HoleSizeController qua Inspector.
/// </summary>
public class GameplayController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HoleController     holeController;
    [SerializeField] private HoleSizeController sizeController;
    [SerializeField] private GameTimer          gameTimer;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenuScene";

    [Header("Level")]
    [Tooltip("Index level bắt đầu (0-based). Có thể override từ SaveManager sau này.")]
    [SerializeField] private int startLevelIndex = 0;

    // ── Injected services ─────────────────────────────────────────────────────

    private GameManager          gameManager;
    private SaveManager          saveManager;
    private UIManager            uiManager;
    private SceneManagerService  sceneManagerService;
    private LevelManager         levelManager;

    [Inject]
    private void Construct(GameManager gameManager, SaveManager saveManager, UIManager uiManager, SceneManagerService sceneManagerService, LevelManager levelManager)
    {
        this.gameManager         = gameManager;
        this.saveManager         = saveManager;
        this.uiManager           = uiManager;
        this.sceneManagerService = sceneManagerService;
        this.levelManager        = levelManager;
    }

    private void Start()
    {
        // Load thong tin tu Level trong database
        levelManager.LoadLevel(startLevelIndex);

        // Subscribe events het gio
        gameTimer.OnTimeUp += TriggerGameOver;

        //if (sizeController != null)
        //{
        //    sizeController.OnScoreAdded += OnObjectSwallowed;
        //} else
        //{
        //    Debug.Log("sizeController is null");
        //}
        StartGameplay();
    }

    private void OnDestroy()
    {
        if (gameTimer != null)
            gameTimer.OnTimeUp -= TriggerGameOver;

        if (sizeController != null)
            sizeController.OnScoreAdded -= OnObjectSwallowed;
    }

    private void StartGameplay()
    {
        gameManager.ChangeState(GameState.Gameplay);
        holeController.SetInputEnabled(true);
        gameTimer.StartTimer();

        // Persistent windows đã tự active — chỉ cần lấy reference để setup
        GameplayPanel panel = uiManager.GetWindow<GameplayPanel>();
        panel?.Setup(levelManager.TotalSpawnedCount);
    }

    private void OnObjectSwallowed(int delta)
    {
        GameplayPanel panel = uiManager.GetWindow<GameplayPanel>();
        panel?.OnObjectSwallowed();
    }

    private void TriggerGameOver()
    {
        gameManager.ChangeState(GameState.Result);

        holeController.SetInputEnabled(false);
        gameTimer.StopTimer();
        levelManager.Cleanup();

        int finalScore = holeController.Score;
        //SaveHighscore(finalScore).Forget();

        GameOverPanel panel = uiManager.Open<GameOverPanel>();
        panel?.Setup(finalScore, saveManager?.Data?.highscore ?? 0);
    }

    public async UniTaskVoid RestartAsync()
    {
        gameManager.ChangeState(GameState.Loading);
        await sceneManagerService.LoadScene("Gameplay");
    }

    public async UniTaskVoid GoToMainMenuAsync()
    {
        gameManager.ChangeState(GameState.Loading);
        await sceneManagerService.LoadScene(mainMenuScene);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    //private async UniTask SaveHighscore(int score)
    //{
    //    if (saveManager?.Data == null) return;

    //    if (score > saveManager.Data.highscore)
    //    {
    //        saveManager.Data.highscore = score;
    //        await saveManager.Save();
    //    }
    //}
}
