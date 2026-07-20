using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controller cho MainMenuScene.
/// Chịu trách nhiệm duy nhất: điều hướng từ Main Menu sang các scene khác.
/// Gán vào GameObject "MainMenu" trong scene và wire các button qua Inspector.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names (phải khớp với tên file .unity, không có extension)")]
    [SerializeField] private string gameplayScene = "Gameplay";

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingButton;

    private void Start()
    {
        // Đăng ký listener khi scene load xong
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (settingButton != null)
            settingButton.onClick.AddListener(OnSettingClicked);
    }

    private void OnDestroy()
    {
        // Dọn listener để tránh memory leak
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayClicked);

        if (settingButton != null)
            settingButton.onClick.RemoveListener(OnSettingClicked);
    }

    /// <summary>
    /// Chuyển sang Gameplay scene. Gán vào Play Button OnClick.
    /// </summary>
    private void OnPlayClicked()
    {
        SceneManager.LoadScene(gameplayScene);
    }

    /// <summary>
    /// Mở Settings panel. TODO (Bước 9): Implement Settings popup.
    /// </summary>
    private void OnSettingClicked()
    {
        // TODO: Mở settings popup khi có UI
        Debug.Log("[MainMenuController] Setting clicked — chưa implement.");
    }
}
