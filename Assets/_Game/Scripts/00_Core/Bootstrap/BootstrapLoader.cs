using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Điểm khởi động duy nhất của game.
///
/// Flow:
///   1. ShowOverlayImmediate  — che màn hình ngay (không flash)
///   2. ChangeState Boot
///   3. Initialize các hệ thống (save, audio, ui)
///   4. TransitionToMainMenuOnBootAsync — toggle sang MainMenu rồi hide overlay
///   5. PlayBGM
/// </summary>
public class BootstrapLoader : MonoBehaviour
{
    private const string MainMenuBGMId = AudioID.BGM.Music;

    private async void Start()
    {
        // Che màn hình ngay — player thấy loading thay vì flash scene trống
        TransitionService.Instance.ShowOverlayImmediate();

        GameManager.Instance.ChangeState(GameState.Boot);

        await SaveManager.Instance.Initialize();
        AudioManager.Instance.Initialize();
        UIManager.Instance.Initialize();

        // Toggle sang MainMenu và hide overlay với animation
        await TransitionService.Instance.TransitionToMainMenuAsync();

        AudioManager.Instance.PlayBGM(MainMenuBGMId);
    }
}
