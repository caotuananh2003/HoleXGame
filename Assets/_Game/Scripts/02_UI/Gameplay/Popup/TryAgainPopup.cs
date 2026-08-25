using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// Popup hiện ra sau khi người dùng đóng GameOverBombPopup.
/// Vẫn ở GameplayScene — không load scene mới cho đến khi người dùng chọn.
///
/// Flow:
///   GameOverBombPopup (QuitButton lần 2) → Close self → Open TryAgainPopup
///
///   TryAgainButton:
///       → reload GameplayScene từ đầu (không reborn, không giữ state)
///
///   CloseButton:
///       → CleanupLevel → về MainMenuScene
///
/// Hierarchy gợi ý:
///   TryAgainPopup
///   ├── TryAgainButton
///   └── CloseButton
/// </summary>
public class TryAgainPopup : PopupWindow
{
    [Header("Buttons")]
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button closeButton;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenuScene";

    // ── Dependencies ──────────────────────────────────────────────────────────
    private SceneManagerService sceneManagerService;
    private GameManager         gameManager;
    private LevelManager        levelManager;
    private GameplayController  gameplayController;

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

    private void Start()
    {
        gameplayController = FindAnyObjectByType<GameplayController>();

        Validate();

        if (tryAgainButton != null) tryAgainButton.onClick.AddListener(OnTryAgainClicked);
        if (closeButton    != null) closeButton.onClick.AddListener(OnCloseClicked);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (tryAgainButton != null) tryAgainButton.onClick.RemoveListener(OnTryAgainClicked);
        if (closeButton    != null) closeButton.onClick.RemoveListener(OnCloseClicked);
    }

    // =========================================================================
    // Handlers
    // =========================================================================

    private void OnTryAgainClicked()
    {
        // Reload lại GameplayScene từ đầu — GameplayController.Start() sẽ
        // load lại đúng level từ saveData (currentLevelIndex không đổi vì
        // chỉ tăng khi WIN, không tăng khi GameOver).
        gameplayController?.RestartLevelAsync().Forget();
    }

    private void OnCloseClicked()
    {
        levelManager?.CleanupLevel();
        gameManager?.ChangeState(GameState.Loading);
        sceneManagerService?.LoadScene(mainMenuScene).Forget();
    }

    // =========================================================================
    // Validate
    // =========================================================================

    private void Validate()
    {
        if (tryAgainButton       == null) Debug.LogError("[TryAgainPopup] tryAgainButton is not assigned.",         this);
        if (closeButton          == null) Debug.LogError("[TryAgainPopup] closeButton is not assigned.",            this);
        if (sceneManagerService  == null) Debug.LogError("[TryAgainPopup] sceneManagerService is null. Check VContainer.", this);
        if (gameManager          == null) Debug.LogError("[TryAgainPopup] gameManager is null. Check VContainer.",  this);
        if (levelManager         == null) Debug.LogError("[TryAgainPopup] levelManager is null. Check VContainer.", this);
        if (gameplayController   == null) Debug.LogWarning("[TryAgainPopup] GameplayController not found in scene.", this);
    }
}
