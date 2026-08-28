using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainmenuPanel : UIWindow
{
    [Header("Navigation")]
    [SerializeField] private Button playButton;
    [SerializeField] private BottomPanel bottomPanel;

    [Header("Popup Buttons")]
    [SerializeField] private Button profileButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button removeAdsButton;
    [SerializeField] private ProfilePreview profilePreview;

    [Header("HUD Buttons")]
    [SerializeField] private Button   currencyButton;
    [SerializeField] private TMP_Text currencyText;
    [SerializeField] private Button   livesButton;

    [Header("Data")]
    [SerializeField] private PlayerProfile playerProfile;

    private void OnEnable()
    {
        if (SaveManager.Instance?.PlayerData != null)
            RefreshCurrency();
    }

    private void Start()
    {
        Validate();

        playButton.onClick.AddListener(OnPlayClicked);
        profileButton.onClick.AddListener(OnProfileClicked);
        settingButton.onClick.AddListener(OnSettingClicked);
        removeAdsButton.onClick.AddListener(OnRemoveAdsClicked);
        currencyButton.onClick.AddListener(OnCurrencyClicked);
        livesButton.onClick.AddListener(OnLivesClicked);

        profilePreview?.Init(
            playerProfile.AvatarDatabase,
            playerProfile.FrameDatabase,
            playerProfile.BadgeDatabase);

        SpawnDefaultProfile();
        RefreshPreview();
    }

    private void OnDestroy()
    {
        playButton.onClick.RemoveListener(OnPlayClicked);
        profileButton.onClick.RemoveListener(OnProfileClicked);
        settingButton.onClick.RemoveListener(OnSettingClicked);
        removeAdsButton.onClick.RemoveListener(OnRemoveAdsClicked);
        currencyButton.onClick.RemoveListener(OnCurrencyClicked);
        livesButton.onClick.RemoveListener(OnLivesClicked);
    }

    private void OnPlayClicked()       => MainmenuNavigator.GoToGameplayStatic();
    private void OnProfileClicked()    => UIManager.Instance.Open<ProfilePopup>();
    private void OnSettingClicked()    => UIManager.Instance.Open<SettingPopup>();
    private void OnRemoveAdsClicked()  => UIManager.Instance.Open<RemoveAdsPopup>();
    private void OnCurrencyClicked()   => bottomPanel?.NavigateToPanel<ShopPanel>();
    private void OnLivesClicked()      => UIManager.Instance?.Open<LifePopup>();

    private void SpawnDefaultProfile()
    {
        if (SaveManager.Instance?.PlayerData == null)
        {
            Debug.LogError("[MainmenuPanel] PlayerData is null — cannot seed profile.");
            return;
        }

        ProfileData profile = SaveManager.Instance.PlayerData.profileData;

        if (!string.IsNullOrEmpty(profile.selectedAvatarId) ||
            !string.IsNullOrEmpty(profile.selectedFrameId)  ||
            !string.IsNullOrEmpty(profile.selectedBadgeId))
            return;

        if (playerProfile.AvatarDatabase?.Avatars.Count > 0)
            profile.selectedAvatarId = playerProfile.AvatarDatabase.Avatars[0].Id;
        else
            Debug.LogError("Cannot spawn avatar");

        if (playerProfile.FrameDatabase?.Frames.Count > 0)
            profile.selectedFrameId = playerProfile.FrameDatabase.Frames[0].Id;
        else
            Debug.LogError("Cannot spawn frame");

        if (playerProfile.BadgeDatabase?.Badges.Count > 0)
            profile.selectedBadgeId = playerProfile.BadgeDatabase.Badges[0].Id;
        else
            Debug.LogError("Cannot spawn badge");

        SaveManager.Instance.Save().Forget();
    }

    public void RefreshPreview()  => profilePreview.Refresh(SaveManager.Instance.PlayerData.profileData);
    private void RefreshCurrency() => currencyText.text = SaveManager.Instance.PlayerData.currency.ToString();

    private void Validate()
    {
        if (UIManager.Instance == null) Debug.LogError("[MainmenuPanel] UIManager.Instance is NULL.", this);
        if (playButton       == null)   Debug.LogError("[MainmenuPanel] playButton is NULL.", this);
        if (bottomPanel      == null)   Debug.LogError("[MainmenuPanel] bottomPanel is NULL.", this);
        if (profileButton    == null)   Debug.LogError("[MainmenuPanel] profileButton is NULL.", this);
        if (settingButton    == null)   Debug.LogError("[MainmenuPanel] settingButton is NULL.", this);
        if (removeAdsButton  == null)   Debug.LogError("[MainmenuPanel] removeAdsButton is NULL.", this);
        if (profilePreview   == null)   Debug.LogError("[MainmenuPanel] profilePreview is NULL.", this);
        if (currencyButton   == null)   Debug.LogError("[MainmenuPanel] currencyButton is NULL.", this);
        if (currencyText     == null)   Debug.LogError("[MainmenuPanel] currencyText is NULL.", this);
        if (livesButton      == null)   Debug.LogError("[MainmenuPanel] livesButton is NULL.", this);
        if (playerProfile    == null)   Debug.LogError("[MainmenuPanel] playerProfile is NULL.", this);
        if (SaveManager.Instance?.PlayerData == null) Debug.LogError("[MainmenuPanel] SaveManager.PlayerData is null.", this);
    }
}
