using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Điểm khởi động duy nhất của game.
/// Điều phối thứ tự init theo đúng thứ tự async:
///   Boot → Load save → Init audio → Register UI → Show MainMenu → Play BGM
/// </summary>
public class BootstrapLoader : MonoBehaviour
{
    private const string MainMenuBGMId = AudioID.BGM.Music;

    private async void Start()
    {
        // Lấy Instance trong Start() để đảm bảo tất cả Awake() đã chạy xong
        var gameManager       = GameManager.Instance;
        var saveManager       = SaveManager.Instance;
        var audioManager      = AudioManager.Instance;
        var uiSceneRoot       = UISceneRoot.Instance;
        var transitionService = TransitionService.Instance;

        gameManager.ChangeState(GameState.Boot);

        await saveManager.Initialize();   // PlayerData phải có trước khi audio init

        audioManager.Initialize();        // đọc bgmVolume/sfxVolume từ PlayerData

        uiSceneRoot.RegisterAll();        // đăng ký toàn bộ UIWindow với UIManager

        transitionService.ShowMainMenuImmediate(); // hiện MainMenu, ẩn Gameplay

        audioManager.PlayBGM(MainMenuBGMId);
    }
}
