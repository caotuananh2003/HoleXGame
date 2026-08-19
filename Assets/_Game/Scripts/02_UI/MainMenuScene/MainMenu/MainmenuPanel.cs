using Cysharp.Threading.Tasks;
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

        SeedDefaultProfileIfNeeded();
        RefreshPreview();
        //RefreshCurrency();

        if (bottomPanel == null)
            Debug.LogWarning("[MainmenuPanel] bottomPanel is not assigned in Inspector.");
    }

    /// <summary>
    /// Khởi tạo profile mặc định nếu chưa có avatar/frame/badge (lần đầu chơi).
    /// Gán giá trị đầu tiên từ database và save lại.
    /// </summary>
    private void SeedDefaultProfileIfNeeded()
    {
        if (saveManager?.PlayerData?.profile == null)
        {
            Debug.LogWarning("[MainmenuPanel] ProfileData is null — cannot seed default profile.");
            return;
        }

        if (playerProfile == null)
        {
            Debug.LogWarning("[MainmenuPanel] playerProfile SO is not assigned — cannot seed default profile.");
            return;
        }

        ProfileData profile = saveManager.PlayerData.profile;

        // Chỉ seed khi cả 3 ID đều rỗng (lần đầu tiên chơi)
        if (!string.IsNullOrEmpty(profile.selectedAvatarId) ||
            !string.IsNullOrEmpty(profile.selectedFrameId)  ||
            !string.IsNullOrEmpty(profile.selectedBadgeId))
        {
            return; // Đã có dữ liệu rồi, không cần seed
        }

        bool seeded = false;

        // Seed Avatar
        if (playerProfile.AvatarDatabase != null && playerProfile.AvatarDatabase.Avatars.Count > 0)
        {
            AvatarDefinition firstAvatar = playerProfile.AvatarDatabase.Avatars[0];
            if (firstAvatar != null)
            {
                profile.selectedAvatarId = firstAvatar.Id;
                seeded = true;
                Debug.Log($"[MainmenuPanel] Seeded default avatar: {firstAvatar.Id}");
            }
        }

        // Seed Frame
        if (playerProfile.FrameDatabase != null && playerProfile.FrameDatabase.Frames.Count > 0)
        {
            FrameDefinition firstFrame = playerProfile.FrameDatabase.Frames[0];
            if (firstFrame != null)
            {
                profile.selectedFrameId = firstFrame.Id;
                seeded = true;
                Debug.Log($"[MainmenuPanel] Seeded default frame: {firstFrame.Id}");
            }
        }

        // Seed Badge
        if (playerProfile.BadgeDatabase != null && playerProfile.BadgeDatabase.Badges.Count > 0)
        {
            BadgeDefinition firstBadge = playerProfile.BadgeDatabase.Badges[0];
            if (firstBadge != null)
            {
                profile.selectedBadgeId = firstBadge.Id;
                seeded = true;
                Debug.Log($"[MainmenuPanel] Seeded default badge: {firstBadge.Id}");
            }
        }

        // Save ngay sau khi seed
        if (seeded)
        {
            saveManager.Save().Forget();
            Debug.Log("[MainmenuPanel] Default profile seeded and saved.");
        }
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
