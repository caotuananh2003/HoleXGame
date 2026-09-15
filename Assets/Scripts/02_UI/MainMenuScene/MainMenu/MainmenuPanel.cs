using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainmenuPanel : UIWindow
{
    [Header("Navigation")]
    [SerializeField] private Button _playButton;
    [SerializeField] private BottomPanel _bottomPanel;

    [Header("Popup Buttons")]
    [SerializeField] private Button _profileButton;
    [SerializeField] private Button _settingButton;
    [SerializeField] private Button _removeAdsButton;
    [SerializeField] private ProfilePreview _profilePreview;

    [Header("HUD Buttons")]
    [SerializeField] private Button   _currencyButton;
    [SerializeField] private TMP_Text _currencyText;
    [SerializeField] private Button   _livesButton;

    [Header("Data")]
    [SerializeField] private PlayerProfile _playerProfile;

    private void OnEnable()
    {
        if (SaveManager.Instance?.PlayerData != null)
            RefreshCurrency();
    }

    private void Start()
    {
        Validate();

        _playButton.onClick.AddListener(OnPlayClicked);
        _profileButton.onClick.AddListener(OnProfileClicked);
        _settingButton.onClick.AddListener(OnSettingClicked);
        _removeAdsButton.onClick.AddListener(OnRemoveAdsClicked);
        _currencyButton.onClick.AddListener(OnCurrencyClicked);
        _livesButton.onClick.AddListener(OnLivesClicked);

        _profilePreview?.Init(
            _playerProfile.AvatarDatabase,
            _playerProfile.FrameDatabase,
            _playerProfile.BadgeDatabase);

        SpawnDefaultProfile();
        RefreshPreview();
    }

    private void OnDestroy()
    {
        _playButton.onClick.RemoveListener(OnPlayClicked);
        _profileButton.onClick.RemoveListener(OnProfileClicked);
        _settingButton.onClick.RemoveListener(OnSettingClicked);
        _removeAdsButton.onClick.RemoveListener(OnRemoveAdsClicked);
        _currencyButton.onClick.RemoveListener(OnCurrencyClicked);
        _livesButton.onClick.RemoveListener(OnLivesClicked);
    }

    private void OnPlayClicked()       => MainmenuNavigator.Instance.GoToGameplay();
    private void OnProfileClicked()    => UIManager.Instance.Open<ProfilePopup>();
    private void OnSettingClicked()    => UIManager.Instance.Open<SettingPopup>();
    private void OnRemoveAdsClicked()  => UIManager.Instance.Open<RemoveAdsPopup>();
    private void OnCurrencyClicked()   => _bottomPanel?.NavigateToPanel<ShopPanel>();
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

        if (_playerProfile.AvatarDatabase?.Avatars.Count > 0)
            profile.selectedAvatarId = _playerProfile.AvatarDatabase.Avatars[0].Id;
        else
            Debug.LogError("Cannot spawn avatar");

        if (_playerProfile.FrameDatabase?.Frames.Count > 0)
            profile.selectedFrameId = _playerProfile.FrameDatabase.Frames[0].Id;
        else
            Debug.LogError("Cannot spawn frame");

        if (_playerProfile.BadgeDatabase?.Badges.Count > 0)
            profile.selectedBadgeId = _playerProfile.BadgeDatabase.Badges[0].Id;
        else
            Debug.LogError("Cannot spawn badge");

        SaveManager.Instance.Save().Forget();
    }

    public void RefreshPreview()  => _profilePreview.Refresh(SaveManager.Instance.PlayerData.profileData);
    private void RefreshCurrency() => _currencyText.text = SaveManager.Instance.PlayerData.currency.ToString();

    private void Validate()
    {
        if (UIManager.Instance == null) Debug.LogError("[MainmenuPanel] UIManager.Instance is NULL.", this);
        if (_playButton       == null)   Debug.LogError("[MainmenuPanel] playButton is NULL.", this);
        if (_bottomPanel      == null)   Debug.LogError("[MainmenuPanel] bottomPanel is NULL.", this);
        if (_profileButton    == null)   Debug.LogError("[MainmenuPanel] profileButton is NULL.", this);
        if (_settingButton    == null)   Debug.LogError("[MainmenuPanel] settingButton is NULL.", this);
        if (_removeAdsButton  == null)   Debug.LogError("[MainmenuPanel] removeAdsButton is NULL.", this);
        if (_profilePreview   == null)   Debug.LogError("[MainmenuPanel] profilePreview is NULL.", this);
        if (_currencyButton   == null)   Debug.LogError("[MainmenuPanel] currencyButton is NULL.", this);
        if (_currencyText     == null)   Debug.LogError("[MainmenuPanel] currencyText is NULL.", this);
        if (_livesButton      == null)   Debug.LogError("[MainmenuPanel] livesButton is NULL.", this);
        if (_playerProfile    == null)   Debug.LogError("[MainmenuPanel] playerProfile is NULL.", this);
        if (SaveManager.Instance?.PlayerData == null) Debug.LogError("[MainmenuPanel] SaveManager.PlayerData is null.", this);
    }
}
