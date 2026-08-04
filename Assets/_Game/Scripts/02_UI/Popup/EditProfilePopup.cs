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

    private bool listsPopulated;

    private void OnEnable()
    {
        InitPopupState();
    }

    private void Start()
    {
        ValidateInspectorRefs();
        RegisterButtons();

        profilePreview.Init(
            playerProfile.AvatarDatabase,
            playerProfile.FrameDatabase,
            playerProfile.BadgeDatabase);

        InitPopupState();
    }

    private void InitPopupState()
    {
        if (saveManager == null || playerProfile == null) return;

        // Clone dữ liệu đã lưu làm editing data.
        savedSnapshot = saveManager.Data.profile.Clone();
        editingData   = savedSnapshot.Clone();

        // Spawn items một lần duy nhất.
        if (!listsPopulated)
        {
            PopulateAllLists();
            listsPopulated = true;
        }

        // Hiển thị tab Avatar mặc định mỗi lần mở.
        ShowTab(ProfileTab.Avatar);

        // Refresh preview theo dữ liệu đã lưu.
        profilePreview.Refresh(editingData);
    }

    // =========================================================================
    // Tab logic
    // =========================================================================

    private void ShowTab(ProfileTab tab)
    {
        activeTab = tab;

        SetContentActive(avatarContent, tab == ProfileTab.Avatar);
        SetContentActive(frameContent,  tab == ProfileTab.Frame);
        SetContentActive(badgeContent,  tab == ProfileTab.Badge);

        RefreshSelectionHighlights();
    }

    private static void SetContentActive(Transform content, bool active)
    {
        if (content != null)
            content.gameObject.SetActive(active);
    }

    // =========================================================================
    // Populate lists
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
        if (saveManager?.Data == null)
        {
            Debug.LogWarning("[EditProfilePopup] SaveManager.Data is null. Cannot save.");
            return;
        }

        // Ghi editing data vào PlayerData.
        saveManager.Data.profile.selectedAvatarId = editingData.selectedAvatarId;
        saveManager.Data.profile.selectedFrameId  = editingData.selectedFrameId;
        saveManager.Data.profile.selectedBadgeId  = editingData.selectedBadgeId;

        saveManager.Save().Forget();

        // Cập nhật snapshot sau khi save — dirty flag sẽ false.
        savedSnapshot = editingData.Clone();

        // Refresh ProfilePopup nếu đang mở bên dưới.
        UIManager?.GetWindow<ProfilePopup>()?.RefreshPreview();

        Debug.Log("[EditProfilePopup] Profile đã được lưu.");
    }

    // =========================================================================
    // Close
    // =========================================================================

    private void OnCloseClicked()
    {
        bool isDirty = !editingData.Equals(savedSnapshot);

        if (isDirty)
            Debug.Log("Chưa save");

        UIManager?.Close<EditProfilePopup>();
    }

    // =========================================================================
    // Tab button handlers
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

    private void OnDestroy()
    {
        UnregisterButtons();
        ClearAllLists();
    }

    private void ValidateInspectorRefs() // Kiểm tra xem có bị null cái ref nào trên inspector không
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
    }
}
