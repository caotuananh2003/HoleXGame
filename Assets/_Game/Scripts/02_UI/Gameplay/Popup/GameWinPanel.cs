using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// Panel kết quả sau khi thắng.
/// Mode = Popup — đè lên HUD.
/// Wire GameplayController vào Inspector.
/// </summary>
public class GameWinPanel : UIWindow
{
    [Header("UI")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button watchAdsButton;

    private GameplayController gameplayController;

    private void Awake()
    {
        gameplayController = FindAnyObjectByType<GameplayController>();
    }
    private void Start()
    {
        //if (restartButton != null)
        //    restartButton.onClick.AddListener(() => gameplayController?.RestartAsync());

        //if (mainMenuButton != null)
        //    mainMenuButton.onClick.AddListener(() => gameplayController?.GoToMainMenuAsync());
    }

    private void OnDestroy()
    {
    }
}
