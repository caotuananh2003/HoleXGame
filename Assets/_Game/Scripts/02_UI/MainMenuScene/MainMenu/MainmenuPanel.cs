using DG.Tweening.Core.Easing;
using TMPro;
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
    [SerializeField] private BottomPanel bottomPanel;
    private MainmenuNavigator navigator;

    [Header("Popup Buttons")]
    [SerializeField] private Button profileButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button removeAdsButton;
    [SerializeField] private ProfilePreview profilePreview;

    [Header("HUD Buttons")]
    [SerializeField] private Button    currencyButton;
    [SerializeField] private TMP_Text  currencyText;   // Text hiển thị số currency
    [SerializeField] private Button    livesButton;

    [Header("Data")]
    [SerializeField] private PlayerProfile playerProfile;

    private SaveManager saveManager;
    [Inject]
    private void Construct(MainmenuNavigator navigator, SaveManager saveManager)
    {
        this.navigator = navigator;
        this.saveManager = saveManager;
    }

    private void OnEnable()
    {
        RefreshCurrency();
    }

    private void Start()
    {
        Register(playButton, OnPlayClicked);
        Register(profileButton, OnProfileClicked);
        Register(settingButton, OnSettingClicked);
        Register(removeAdsButton, OnRemoveAdsClicked);
        Register(currencyButton, OnCurrencyClicked);
        Register(livesButton, OnLivesClicked);

        if (playerProfile != null)
        {
            profilePreview?.Init(
                playerProfile.AvatarDatabase,
                playerProfile.FrameDatabase,
                playerProfile.BadgeDatabase);
        }
        else
        {
            Debug.LogWarning("[MainmenuPanel] playerProfile is not assigned. ProfilePreview will not display correctly.");
        }

        RefreshPreview();
        //RefreshCurrency();

        if (bottomPanel == null)
            Debug.LogWarning("[MainmenuPanel] bottomPanel is not assigned in Inspector.");
    }

    public void RefreshPreview() // Refresh preview theo dữ liệu hiện tại trong SaveManager.
    {
        if (saveManager?.PlayerData == null)
        {
            Debug.LogWarning("[ProfilePopup] SaveManager.Data is null. Cannot refresh preview.");
            return;
        }

        profilePreview?.Refresh(saveManager.PlayerData.profile);
    }

    /// <summary>Cập nhật text currency button theo dữ liệu hiện tại trong SaveManager.</summary>
    private void RefreshCurrency()
    {
        if (saveManager?.PlayerData == null) return;
        if (currencyText      == null) return;

        currencyText.text = saveManager.PlayerData.currency.ToString();
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

    private void OnCurrencyClicked()
    {
        bottomPanel?.NavigateToPanel<ShopPanel>();
    }

    private void OnLivesClicked()
    {
        UIManager?.Open<LifePopup>();
    }


    private void OnDestroy()
    {
        Unregister(playButton, OnPlayClicked);
        Unregister(profileButton, OnProfileClicked);
        Unregister(settingButton, OnSettingClicked);
        Unregister(removeAdsButton, OnRemoveAdsClicked);
        Unregister(currencyButton, OnCurrencyClicked);
        Unregister(livesButton, OnLivesClicked);
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
