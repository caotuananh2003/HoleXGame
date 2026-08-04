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

    private void OnPlayClicked()
    {
        navigator?.GoToGameplay();
    }

    private void OnProfileClicked()
    {
        if (UIManager != null)
        {

            Debug.Log("OnProfileClicked");
            UIManager.Open<ProfilePopup>();
        } else
        {
            Debug.Log("UIManager is null");
        }
    }

    private void OnSettingClicked()
    {
        if (UIManager != null)
        {

            Debug.Log("OnSettingClicked");
            UIManager.Open<SettingPopup>();
        }
        else
        {
            Debug.Log("UIManager is null");
        }
    }

    private void OnRemoveAdsClicked()
    {
        if (UIManager != null)
        {

            Debug.Log("OnRemoveAdsClicked");
            UIManager.Open<RemoveAdsPopup>();
        }
        else
        {
            Debug.Log("UIManager is null");
        }
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
