using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// Popup GameOver — nuốt phải bom (BombExplosion).
/// Mode = Popup trong Inspector.
///
/// Không có cơ chế hồi sinh — bomb explosion là kết thúc tức thì.
/// Người chơi chỉ có thể Quit về MainMenu.
/// </summary>
public class GameOverBombPopup : PopupWindow
{
    [Header("Quit")]
    [SerializeField] private Button quitButton;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenuScene";

    // ── Dependencies ──────────────────────────────────────────────────────────
    private SceneManagerService sceneManagerService;
    private GameManager         gameManager;
    private LevelManager        levelManager;

    [Inject]
    private void Construct(
        SceneManagerService sceneManagerService,
        GameManager         gameManager,
        LevelManager        levelManager)
    {
        this.sceneManagerService = sceneManagerService;
        this.gameManager         = gameManager;
        this.levelManager        = levelManager;
    }

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    public override void Open()
    {
        base.Open();
        UIManager?.PlaySFX(AudioID.SFX.UiLose);
    }

    private void Start()
    {
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        if (quitButton == null)
            Debug.LogWarning("[GameOverBombPopup] quitButton is not assigned.");
    }

    
    private void OnDestroy()
    {
        base.OnDestroy();

        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    // =========================================================================
    // Handler
    // =========================================================================

    private void OnQuitClicked()
    {
        Debug.Log("[GameOverBombPopup] Quit — cleanup level, về MainMenu.");

        // Cleanup level trước khi chuyển scene
        levelManager?.CleanupLevel();

        gameManager?.ChangeState(GameState.Loading);
        sceneManagerService?.LoadScene(mainMenuScene).Forget();
    }
}
