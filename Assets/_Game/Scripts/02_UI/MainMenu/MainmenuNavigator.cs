using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

/// <summary>
/// Điều hướng scene từ MainMenu.
/// Gắn vào MainMenuContext GameObject trong MainmenuScene.
/// VContainer inject từ GameLifetimeScope (parent scope).
/// </summary>
public class MainmenuNavigator : MonoBehaviour
{
    [SerializeField] private string gameplayScene = "GameplayScene";

    private SceneManagerService sceneManagerService;
    private GameManager gameManager;

    [Inject]
    private void Construct(SceneManagerService sceneManagerService, GameManager gameManager)
    {
        this.sceneManagerService = sceneManagerService;
        this.gameManager = gameManager;
    }

    public void GoToGameplay()
    {
        Debug.Log("MainmenuNavigator.GoToGameplay");
        gameManager.ChangeState(GameState.Loading);
        sceneManagerService.LoadScene(gameplayScene).Forget();
    }
}
