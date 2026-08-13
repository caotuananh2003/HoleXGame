using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class EditProfilePopup : UIWindow
{
    private enum ProfileTab { Avatar, Frame, Badge } // Enum các button

    [Header("Navigation")]
    [SerializeField] private Button closeButton;

    [Header("Preview")]
    [SerializeField] private ProfilePreview profilePreview; // ChildObject

    [Header("Tab Buttons")]
    [SerializeField] private Button avatarTabButton;
    [SerializeField] private Button frameTabButton;
    [SerializeField] private Button badgeTabButton;

    [Header("Scroll Contents")]
    [SerializeField] private Transform avatarContent;
    [SerializeField] private Transform frameContent;
    [SerializeField] private Transform badgeContent;

    // ScrollRect dùng chung cho cả 3 tab — content sẽ được swap khi đổi tab
    [Header("ScrollRect")]
    [SerializeField] private ScrollRect scrollRect;

    [Header("Item Prefabs")]
    [SerializeField] private AvatarItemUI avatarItemPrefab;
    [SerializeField] private FrameItemUI  frameItemPrefab;
    [SerializeField] private BadgeItemUI  badgeItemPrefab;

    [Header("Bottom")]
    [SerializeField] private Button saveButton;

    [Header("Data")]
    [SerializeField] private PlayerProfile playerProfile; // ScriptableObject chứa Data

    private SaveManager saveManager;

    [Inject]
    private void Construct(SaveManager saveManager)
    {
        this.saveManager = saveManager;
    }

    private ProfileData editingData; // Class chứa bản copy đang chỉnh sửa — không phải SaveManager.Data.profile.
    private ProfileData savedSnapshot; // Class chứa bản Snapshot lúc mở popup — dùng để so sánh dirty.
    private ProfileTab activeTab = ProfileTab.Avatar;

    // Danh sách item đã spawn — dùng để SetSelected và unsubscribe OnClicked
    private readonly List<AvatarItemUI> avatarItems = new();
    private readonly List<FrameItemUI>  frameItems  = new();
    private readonly List<BadgeItemUI>  badgeItems  = new();

    // Flag: Start() đã chạy chưa — tránh OnEnable chạy trước Start
    private bool listsPopulated;
    private bool isStarted;

    private void Start()
    {
        isStarted = true;

        ValidateInspectorRefs();
        RegisterButtons();

        // Init database cho ProfilePreview — bắt buộc phải chạy trước mọi Refresh().
        // Đây là lý do Init nằm ở Start, không thể ở OnEnable:
        // OnEnable chạy trước Start, database chưa sẵn sàng → Refresh sẽ ra ảnh trắng.
        if (playerProfile != null)
        {
            profilePreview.Init(
                playerProfile.AvatarDatabase,
                playerProfile.FrameDatabase,
                playerProfile.BadgeDatabase);
        }

        // Spawn items một lần duy nhất.
        if (!listsPopulated)
        {
            PopulateAllLists();
            listsPopulated = true;
        }

        // OnEnable đã chạy trước Start và bị skip (isStarted = false lúc đó).
        // Gọi RefreshOpenState ngay bây giờ để hiển thị đúng ngay lần mở đầu tiên.
        if (gameObject.activeSelf)
            RefreshOpenState();
    }

    private void OnEnable()
    {
        // Lần đầu tiên (trước Start): bỏ qua.
        // Lý do: profilePreview.Init() chưa chạy, gọi Refresh lúc này → ảnh trắng.
        // Start() sẽ gọi RefreshOpenState() thay thế.
        if (!isStarted) return;

        RefreshOpenState();
    }

    private void OnDestroy()
    {
        UnregisterButtons();
        ClearAllLists();
    }

    // =========================================================================
    // Open state — chạy mỗi lần popup được mở
    // =========================================================================

    /// <summary>
    /// Reset editingData về trạng thái đã lưu và cập nhật toàn bộ UI.
    /// Gọi mỗi lần popup Enable, sau khi Start() đã hoàn tất.
    /// </summary>
    private void RefreshOpenState()
    {
        if (saveManager == null || playerProfile == null) return;

        // Luôn clone lại từ saved data mỗi lần mở.
        // → Bỏ toàn bộ thay đổi chưa save từ lần mở trước.
        savedSnapshot = saveManager.PlayerData.profile.Clone();
        editingData   = savedSnapshot.Clone();

        // Tab mặc định là Avatar mỗi lần mở.
        ShowTab(ProfileTab.Avatar);

        // Preview hiển thị avatar/frame/badge hiện tại đang dùng.
        profilePreview.Refresh(editingData);
    }

    // =========================================================================
    // Tab
    // =========================================================================

    private void ShowTab(ProfileTab tab)
    {
        activeTab = tab;

        avatarContent.gameObject.SetActive(tab == ProfileTab.Avatar);
        frameContent.gameObject.SetActive(tab == ProfileTab.Frame);
        badgeContent.gameObject.SetActive(tab == ProfileTab.Badge);

        // Swap content của ScrollRect theo tab đang active.
        // Giúp scroll position được quản lý đúng và ScrollRect biết content nào cần scroll.
        if (scrollRect != null)
        {
            scrollRect.content = tab switch
            {
                ProfileTab.Avatar => avatarContent as RectTransform,
                ProfileTab.Frame  => frameContent  as RectTransform,
                ProfileTab.Badge  => badgeContent  as RectTransform,
                _                 => scrollRect.content,
            };

            // Reset scroll về đầu mỗi khi đổi tab
            scrollRect.verticalNormalizedPosition = 1f;
        }

        RefreshSelectionHighlights();
    }

    // =========================================================================
    // Populate lists — chỉ chạy một lần trong Start()
    // =========================================================================

    private void PopulateAllLists()
    {
        if (playerProfile.AvatarDatabase != null)
        {
            foreach (AvatarDefinition def in playerProfile.AvatarDatabase.Avatars)
            {
                AvatarItemUI item = Instantiate(avatarItemPrefab, avatarContent);
                item.Setup(def, def.UnlockedByDefault);
                item.OnClicked += OnAvatarItemClicked;
                avatarItems.Add(item);
            }
        }

        if (playerProfile.FrameDatabase != null)
        {
            foreach (FrameDefinition def in playerProfile.FrameDatabase.Frames)
            {
                FrameItemUI item = Instantiate(frameItemPrefab, frameContent);
                item.Setup(def, def.UnlockedByDefault);
                item.OnClicked += OnFrameItemClicked;
                frameItems.Add(item);
            }
        }

        if (playerProfile.BadgeDatabase != null)
        {
            foreach (BadgeDefinition def in playerProfile.BadgeDatabase.Badges)
            {
                BadgeItemUI item = Instantiate(badgeItemPrefab, badgeContent);
                item.Setup(def, def.UnlockedByDefault);
                item.OnClicked += OnBadgeItemClicked;
                badgeItems.Add(item);
            }
        }
    }

    // =========================================================================
    // Selection handlers
    // =========================================================================

    private void OnAvatarItemClicked(string avatarId)
    {
        editingData.selectedAvatarId = avatarId;
        profilePreview.SetAvatar(avatarId);
        RefreshSelectionHighlights();
    }

    private void OnFrameItemClicked(string frameId)
    {
        editingData.selectedFrameId = frameId;
        profilePreview.SetFrame(frameId);
        RefreshSelectionHighlights();
    }

    private void OnBadgeItemClicked(string badgeId)
    {
        editingData.selectedBadgeId = badgeId;
        profilePreview.SetBadge(badgeId);
        RefreshSelectionHighlights();
    }

    private void RefreshSelectionHighlights()
    {
        foreach (AvatarItemUI item in avatarItems)
            item.SetSelected(item.ItemId == editingData.selectedAvatarId);

        foreach (FrameItemUI item in frameItems)
            item.SetSelected(item.ItemId == editingData.selectedFrameId);

        foreach (BadgeItemUI item in badgeItems)
            item.SetSelected(item.ItemId == editingData.selectedBadgeId);
    }

    // =========================================================================
    // Save
    // =========================================================================

    private void OnSaveClicked()
    {
        if (saveManager?.PlayerData == null)
        {
            Debug.LogWarning("[EditProfilePopup] SaveManager.Data is null. Cannot save.");
            return;
        }

        saveManager.PlayerData.profile.selectedAvatarId = editingData.selectedAvatarId;
        saveManager.PlayerData.profile.selectedFrameId  = editingData.selectedFrameId;
        saveManager.PlayerData.profile.selectedBadgeId  = editingData.selectedBadgeId;

        saveManager.Save().Forget();

        savedSnapshot = editingData.Clone();

        UIManager?.GetWindow<ProfilePopup>()?.RefreshPreview();
        UIManager?.GetWindow<MainmenuPanel>()?.RefreshPreview();

        Debug.Log("[EditProfilePopup] Profile đã được lưu.");
    }

    // =========================================================================
    // Close
    // =========================================================================

    private void OnCloseClicked()
    {
        bool isDirty = editingData != null && !editingData.Equals(savedSnapshot);

        if (isDirty)
            Debug.Log("Chưa save");

        UIManager?.Close<EditProfilePopup>();
    }

    // =========================================================================
    // Tab handlers
    // =========================================================================

    private void OnAvatarTabClicked() => ShowTab(ProfileTab.Avatar);
    private void OnFrameTabClicked()  => ShowTab(ProfileTab.Frame);
    private void OnBadgeTabClicked()  => ShowTab(ProfileTab.Badge);

    // =========================================================================
    // Register / Unregister
    // =========================================================================

    private void RegisterButtons()
    {
        if (closeButton     != null) closeButton.onClick.AddListener(OnCloseClicked);
        if (saveButton      != null) saveButton.onClick.AddListener(OnSaveClicked);
        if (avatarTabButton != null) avatarTabButton.onClick.AddListener(OnAvatarTabClicked);
        if (frameTabButton  != null) frameTabButton.onClick.AddListener(OnFrameTabClicked);
        if (badgeTabButton  != null) badgeTabButton.onClick.AddListener(OnBadgeTabClicked);
    }

    private void UnregisterButtons()
    {
        if (closeButton     != null) closeButton.onClick.RemoveListener(OnCloseClicked);
        if (saveButton      != null) saveButton.onClick.RemoveListener(OnSaveClicked);
        if (avatarTabButton != null) avatarTabButton.onClick.RemoveListener(OnAvatarTabClicked);
        if (frameTabButton  != null) frameTabButton.onClick.RemoveListener(OnFrameTabClicked);
        if (badgeTabButton  != null) badgeTabButton.onClick.RemoveListener(OnBadgeTabClicked);
    }

    // =========================================================================
    // Cleanup
    // =========================================================================

    private void ClearAllLists()
    {
        foreach (AvatarItemUI item in avatarItems)
            if (item != null) item.OnClicked -= OnAvatarItemClicked;
        avatarItems.Clear();

        foreach (FrameItemUI item in frameItems)
            if (item != null) item.OnClicked -= OnFrameItemClicked;
        frameItems.Clear();

        foreach (BadgeItemUI item in badgeItems)
            if (item != null) item.OnClicked -= OnBadgeItemClicked;
        badgeItems.Clear();
    }

    private void ValidateInspectorRefs()
    {
        if (closeButton      == null) Debug.LogWarning("[EditProfilePopup] closeButton is not assigned.");
        if (saveButton       == null) Debug.LogWarning("[EditProfilePopup] saveButton is not assigned.");
        if (profilePreview   == null) Debug.LogWarning("[EditProfilePopup] profilePreview is not assigned.");
        if (avatarTabButton  == null) Debug.LogWarning("[EditProfilePopup] avatarTabButton is not assigned.");
        if (frameTabButton   == null) Debug.LogWarning("[EditProfilePopup] frameTabButton is not assigned.");
        if (badgeTabButton   == null) Debug.LogWarning("[EditProfilePopup] badgeTabButton is not assigned.");
        if (avatarContent    == null) Debug.LogWarning("[EditProfilePopup] avatarContent is not assigned.");
        if (frameContent     == null) Debug.LogWarning("[EditProfilePopup] frameContent is not assigned.");
        if (badgeContent     == null) Debug.LogWarning("[EditProfilePopup] badgeContent is not assigned.");
        if (avatarItemPrefab == null) Debug.LogWarning("[EditProfilePopup] avatarItemPrefab is not assigned.");
        if (frameItemPrefab  == null) Debug.LogWarning("[EditProfilePopup] frameItemPrefab is not assigned.");
        if (badgeItemPrefab  == null) Debug.LogWarning("[EditProfilePopup] badgeItemPrefab is not assigned.");
        if (playerProfile    == null) Debug.LogWarning("[EditProfilePopup] playerProfile is not assigned.");
        if (scrollRect       == null) Debug.LogWarning("[EditProfilePopup] scrollRect is not assigned — content sẽ không được swap khi đổi tab.");
    }
}
