using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// Panel chính của MainMenu.
/// Gắn vào MainmenuPanel GameObject trong ContentPanel.
/// - playButton      → chuyển sang GameplayScene
/// - profileButton   → mở ProfilePopup
/// - settingButton   → mở SettingPopup
/// - removeAdsButton → mở RemoveAdsPopup
/// Wire tất cả trong Inspector.
/// </summary>
public class MainmenuPanel : UIWindow
{
    [Header("Navigation")]
    [SerializeField] private Button playButton;
    private MainmenuNavigator navigator;

    [Header("Popup Buttons")]
    [SerializeField] private Button profileButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button removeAdsButton;

    [Inject]
    private void Construct(MainmenuNavigator navigator)
    {
        Debug.Log("==============================");
        this.navigator = navigator;
    }

    private void Start()
    {
        Register(playButton, OnPlayClicked);
        Register(profileButton, OnProfileClicked);
        Register(settingButton, OnSettingClicked);
        Register(removeAdsButton, OnRemoveAdsClicked);
    }

    private void OnDestroy()
    {
        Unregister(playButton, OnPlayClicked);
        Unregister(profileButton, OnProfileClicked);
        Unregister(settingButton, OnSettingClicked);
        Unregister(removeAdsButton, OnRemoveAdsClicked);
    }

    // -------------------------------------------------------------------------

    private void OnPlayClicked()
    {
        Debug.Log("MainmenuPanel.OnPlayClicked");
        navigator?.GoToGameplay();
    }

    private void OnProfileClicked()
    {
        UIManager?.Open<ProfilePopup>();
    }

    private void OnSettingClicked()
    {
        UIManager?.Open<SettingPopup>();
    }

    private void OnRemoveAdsClicked()
    {
        UIManager?.Open<RemoveAdsPopup>();
    }

    // -------------------------------------------------------------------------

    private static void Register(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn != null) btn.onClick.AddListener(action);
    }

    private static void Unregister(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn != null) btn.onClick.RemoveListener(action);
    }
}
