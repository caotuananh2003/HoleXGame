using UnityEngine;
using VContainer;

public class BootstrapLoader : MonoBehaviour
{
    [Header("Scene Names (phải khớp với tên file .unity, không có extension)")]
    //[SerializeField] private string gameplayScene = "Gameplay";
    [SerializeField] private string MainMenuScene = "MainMenuScene"; // File: MainMenuScene.unity
    private const string MainMenuBGMId = AudioID.BGM.Music;

    private SaveManager saveManager;
    private AudioManager audioManager;
    private UIManager uiManager;
    private GameManager gameManager;
    private SceneManagerService sceneManagerService;

    [Inject]
    private void Construct(
        SaveManager saveManager,
        AudioManager audioManager,
        UIManager uiManager,
        GameManager gameManager,
        SceneManagerService sceneManagerService)
    {
        this.saveManager = saveManager;
        this.audioManager = audioManager;
        this.uiManager = uiManager;
        this.gameManager = gameManager;
        this.sceneManagerService = sceneManagerService;
    }

    private async void Start()
    {
        gameManager.ChangeState(GameState.Boot);

        await saveManager.Initialize();

        audioManager.Initialize();

        uiManager.Initialize();

        gameManager.ChangeState(GameState.Loading);

        await sceneManagerService.LoadScene(MainMenuScene);

        gameManager.ChangeState(GameState.MainMenu);

        audioManager.PlayBGM(MainMenuBGMId);
    }
}
