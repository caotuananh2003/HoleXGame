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
    [Tooltip("Index level bắt đầu (0-based). Có thể override từ SaveManager sau này.")]
    [SerializeField] private int startLevelIndex = 0;

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
    private SwallowHandler swallowHandler;
    private GameTimer gameTimer; // Event: OnTimeUp

    private void Awake()
    {
        holeController = FindAnyObjectByType<HoleController>();
        swallowHandler = FindAnyObjectByType<SwallowHandler>();
        gameTimer = FindAnyObjectByType<GameTimer>();
    }
    #endregion

    private void Start()
    {
        // Load thong tin tu Level trong database
        levelManager.LoadLevel(startLevelIndex);

        // Initialize objective system
        objectiveManager.InitializeLevel(levelManager.CurrentLevelDefinition);
        objectiveManager.OnAllObjectivesCompleted += OnLevelWin;

        // Subscribe events het gio
        gameTimer.OnTimeUp += OnGameOver;

        StartGameplay();
    }

    private void StartGameplay()
    {
        gameManager.ChangeState(GameState.Gameplay);
        holeController.SetInputEnabled(true);
        gameTimer.StartTimer();

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

        int finalScore = holeController.Score;

        // TODO: Hiển thị Win Panel thay vì GameOver Panel

        GameWinPanel panel = uiManager.Open<GameWinPanel>();
        //GameOverPanel panel = uiManager.Open<GameOverPanel>();
        //panel?.Setup(finalScore, saveManager?.Data?.highscore ?? 0);
    }

    // Disable Input, StopTimer, Setup GameOverPanel
    private void OnGameOver()
    {
        Debug.Log("GameOver");
        gameManager.ChangeState(GameState.Result);

        holeController.SetInputEnabled(false);
        gameTimer.StopTimer();

        int finalScore = holeController.Score;

        GameOverPanel panel = uiManager.Open<GameOverPanel>();
        panel?.Setup(finalScore, saveManager?.Data?.highscore ?? 0);
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

    private void OnDestroy()
    {
        if (gameTimer != null)
            gameTimer.OnTimeUp -= OnGameOver;

        if (objectiveManager != null)
        {
            objectiveManager.OnAllObjectivesCompleted -= OnLevelWin;
            objectiveManager.Cleanup();
        }
    }
}
