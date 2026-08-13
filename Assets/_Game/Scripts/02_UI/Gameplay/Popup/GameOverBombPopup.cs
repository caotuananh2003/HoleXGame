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
public class GameOverBombPopup : UIWindow
{
    [Header("Quit")]
    [SerializeField] private Button quitButton;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenuScene";

    // ── Dependencies ──────────────────────────────────────────────────────────
    private SceneManagerService sceneManagerService;
    private GameManager         gameManager;

    [Inject]
    private void Construct(
        SceneManagerService sceneManagerService,
        GameManager         gameManager)
    {
        this.sceneManagerService = sceneManagerService;
        this.gameManager         = gameManager;
    }

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Start()
    {
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        if (quitButton == null)
            Debug.LogWarning("[GameOverBombPopup] quitButton is not assigned.");
    }

    private void OnDestroy()
    {
        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    // =========================================================================
    // Handler
    // =========================================================================

    private void OnQuitClicked()
    {
        Debug.Log("[GameOverBombPopup] Quit — về MainMenu.");
        gameManager?.ChangeState(GameState.Loading);
        sceneManagerService?.LoadScene(mainMenuScene).Forget();
    }
}
