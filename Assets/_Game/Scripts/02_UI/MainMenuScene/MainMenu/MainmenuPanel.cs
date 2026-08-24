using Cysharp.Threading.Tasks;
using DG.Tweening.Core.Easing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

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

    #region Life Cycle
    private void OnEnable()
    {
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
    #endregion

    #region OnClick method
    private void OnPlayClicked()
    {
        navigator?.GoToGameplay();
    }

    private void OnProfileClicked()
    {
        UIManager.Open<ProfilePopup>();
    }

    private void OnSettingClicked()
    {
        UIManager.Open<SettingPopup>();
    }

    private void OnRemoveAdsClicked()
    {
        UIManager.Open<RemoveAdsPopup>();
    }

    private void OnCurrencyClicked()
    {
        bottomPanel?.NavigateToPanel<ShopPanel>();
    }

    private void OnLivesClicked()
    {
        UIManager?.Open<LifePopup>();
    }
    #endregion

    #region private method

    private void SpawnDefaultProfile() // Khởi tạo profile mặc định. Gán giá trị đầu tiên từ database và save lại.
    {
        ProfileData profile = saveManager.PlayerData.profileData;

        // Chỉ seed khi cả 3 ID đều rỗng (lần đầu tiên chơi)
        if (!string.IsNullOrEmpty(profile.selectedAvatarId) ||
            !string.IsNullOrEmpty(profile.selectedFrameId) ||
            !string.IsNullOrEmpty(profile.selectedBadgeId))
        {
            return; // Đã có dữ liệu rồi, không cần seed
        }

        if (playerProfile.AvatarDatabase != null && playerProfile.AvatarDatabase.Avatars.Count > 0) // Seed Avatar
        {
            AvatarDefinition firstAvatar = playerProfile.AvatarDatabase.Avatars[0];
            if (firstAvatar != null)
            {
                profile.selectedAvatarId = firstAvatar.Id;
                Debug.Log($"[MainmenuPanel] Seeded default avatar: {firstAvatar.Id}");
            }
        }
        else
        {
            Debug.LogError("Cannot spawn avatar");
        }

        if (playerProfile.FrameDatabase != null && playerProfile.FrameDatabase.Frames.Count > 0) // Seed Frame
        {
            FrameDefinition firstFrame = playerProfile.FrameDatabase.Frames[0];
            if (firstFrame != null)
            {
                profile.selectedFrameId = firstFrame.Id;
                Debug.Log($"[MainmenuPanel] Seeded default frame: {firstFrame.Id}");
            }
        }
        else
        {
            Debug.LogError("Cannot spawn frame");
        }

        if (playerProfile.BadgeDatabase != null && playerProfile.BadgeDatabase.Badges.Count > 0) // Seed Badge
        {
            BadgeDefinition firstBadge = playerProfile.BadgeDatabase.Badges[0];
            if (firstBadge != null)
            {
                profile.selectedBadgeId = firstBadge.Id;
                Debug.Log($"[MainmenuPanel] Seeded default badge: {firstBadge.Id}");
            }
        }
        else
        {
            Debug.LogError("Cannot spawn badge");
        }

        // Save ngay sau khi seed
        saveManager.Save().Forget();
        Debug.Log("[MainmenuPanel] Default profile seeded and saved.");
    }

    public void RefreshPreview() // Refresh preview theo dữ liệu hiện tại trong SaveManager.
    {
        profilePreview.Refresh(saveManager.PlayerData.profileData);
    }

    private void RefreshCurrency() // Cập nhật text currency button theo dữ liệu hiện tại trong SaveManager.
    {
        currencyText.text = saveManager.PlayerData.currency.ToString();
    }

    #endregion


    private void Validate()
    {
        if (UIManager == null)                              Debug.LogError("[MainmenuPanel] UIManager is NULL.", this);
        if (playButton == null)                             Debug.LogError("[MainmenuPanel] playButton is NULL.", this);
        if (bottomPanel == null)                            Debug.LogError("[MainmenuPanel] bottomPanel is NULL.", this);
        if (navigator == null)                              Debug.LogError("[MainmenuPanel] navigator is NULL. Check VContainer registration/injection.", this);
        if (profileButton == null)                          Debug.LogError("[MainmenuPanel] profileButton is NULL.", this);
        if (settingButton == null)                          Debug.LogError("[MainmenuPanel] settingButton is NULL.", this);
        if (removeAdsButton == null)                        Debug.LogError("[MainmenuPanel] removeAdsButton is NULL.", this);
        if (profilePreview == null)                         Debug.LogError("[MainmenuPanel] profilePreview is NULL.", this);
        if (currencyButton == null)                         Debug.LogError("[MainmenuPanel] currencyButton is NULL.", this);
        if (currencyText == null)                           Debug.LogError("[MainmenuPanel] currencyText is NULL.", this);
        if (livesButton == null)                            Debug.LogError("[MainmenuPanel] livesButton is NULL.", this);
        if (playerProfile == null)                          Debug.LogError("[MainmenuPanel] playerProfile is NULL. Assign PlayerProfile ScriptableObject in Inspector.", this);
        if (saveManager == null)                            Debug.LogError("[MainmenuPanel] saveManager is NULL. Check VContainer registration/injection.", this);
        if (saveManager == null)                            Debug.LogError("[MainmenuPanel] saveManager is null.");
        if (saveManager?.PlayerData == null)                Debug.LogError("[MainmenuPanel] saveManager.PlayerData is null.");
        if (saveManager?.PlayerData?.profileData == null)   Debug.LogError("[MainmenuPanel] saveManager.PlayerData.profileData is null.");
    }
}
