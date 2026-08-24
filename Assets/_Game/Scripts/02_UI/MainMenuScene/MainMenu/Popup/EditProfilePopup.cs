using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class EditProfilePopup : PopupWindow
{
    private enum ProfileTab { Avatar, Frame, Badge }
    private ProfileTab activeTab = ProfileTab.Avatar;

    [Header("Navigation")]
    [SerializeField] private Button closeButton;

    [Header("Name")]
    [SerializeField] private TMP_Text nameText;       // Hiển thị tên hiện tại
    [SerializeField] private Button   editNameButton; // Mở EditNamePopup

    [Header("Preview")]
    [SerializeField] private ProfilePreview profilePreview;

    [Header("Tab Buttons")]
    [SerializeField] private TabButton _avatarTabButton;
    [SerializeField] private TabButton _frameTabButton;
    [SerializeField] private TabButton _badgeTabButton;

    [Header("Scroll Contents")]
    [SerializeField] private Transform avatarContent;
    [SerializeField] private Transform frameContent;
    [SerializeField] private Transform badgeContent;

    [Header("ScrollRect")]
    [SerializeField] private ScrollRect scrollRect;

    [Header("Item Prefabs")]
    [SerializeField] private AvatarItemUI avatarItemPrefab;
    [SerializeField] private FrameItemUI  frameItemPrefab;
    [SerializeField] private BadgeItemUI  badgeItemPrefab;

    [Header("Bottom")]
    [SerializeField] private Button saveButton;

    [Header("Data")]
    [SerializeField] private PlayerProfile playerProfile;

    private SaveManager saveManager;

    [Inject]
    private void Construct(SaveManager saveManager)
    {
        this.saveManager = saveManager;
    }

    private ProfileData editingData;
    private ProfileData savedSnapshot;

    private readonly List<AvatarItemUI> avatarItems = new();
    private readonly List<FrameItemUI>  frameItems  = new();
    private readonly List<BadgeItemUI>  badgeItems  = new();

    private bool listsPopulated;
    private bool isStarted;

    private void Start()
    {
        isStarted = true;

        ValidateInspectorRefs();
        RegisterButtons();

        if (playerProfile != null)
        {
            profilePreview.Init(
                playerProfile.AvatarDatabase,
                playerProfile.FrameDatabase,
                playerProfile.BadgeDatabase);
        }

        if (!listsPopulated)
        {
            PopulateAllLists();
            listsPopulated = true;
        }

        if (gameObject.activeSelf)
            RefreshOpenState();
    }

    private void OnEnable()
    {
        if (!isStarted) return;
        RefreshOpenState();
    }

    private void OnDestroy()
    {
        UnregisterButtons();
        ClearAllLists();
    }

    // =========================================================================
    // Open state
    // =========================================================================

    private void RefreshOpenState()
    {
        if (saveManager == null || playerProfile == null) return;

        savedSnapshot = saveManager.PlayerData.profileData.Clone();
        editingData   = savedSnapshot.Clone();

        ShowTab(ProfileTab.Avatar);

        // Hiển thị name hiện tại lên nameText
        RefreshNameText();

        profilePreview.Refresh(editingData);
    }

    // =========================================================================
    // Public API — gọi từ EditNamePopup khi nhấn ContinueButton
    // =========================================================================

    /// <summary>
    /// Cập nhật tên vào editingData và nameText.
    /// Chưa save — chỉ save khi nhấn SaveButton.
    /// </summary>
    public void ApplyNameFromPopup(string newName)
    {
        editingData.playerName = newName;
        profilePreview.SetName(newName);
        RefreshNameText();
    }

    /// <summary>Tên hiện tại đang được edit — EditNamePopup đọc để hiển thị ban đầu.</summary>
    public string CurrentEditingName => editingData?.playerName ?? string.Empty;

    // =========================================================================
    // Tab
    // =========================================================================

    private void ShowTab(ProfileTab tab)
    {
        activeTab = tab;

        if (avatarContent != null) avatarContent.gameObject.SetActive(tab == ProfileTab.Avatar);
        if (frameContent  != null) frameContent.gameObject.SetActive(tab == ProfileTab.Frame);
        if (badgeContent  != null) badgeContent.gameObject.SetActive(tab == ProfileTab.Badge);

        _avatarTabButton?.SetSelected(tab == ProfileTab.Avatar);
        _frameTabButton?.SetSelected(tab == ProfileTab.Frame);
        _badgeTabButton?.SetSelected(tab == ProfileTab.Badge);

        if (scrollRect != null)
        {
            scrollRect.content = tab switch
            {
                ProfileTab.Avatar => avatarContent as RectTransform,
                ProfileTab.Frame  => frameContent  as RectTransform,
                ProfileTab.Badge  => badgeContent  as RectTransform,
                _                 => scrollRect.content,
            };

            scrollRect.verticalNormalizedPosition = 1f;
        }

        RefreshSelectionHighlights();
    }

    // =========================================================================
    // Populate
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
    // Name
    // =========================================================================

    private void RefreshNameText()
    {
        if (nameText == null) return;
        nameText.text = string.IsNullOrWhiteSpace(editingData?.playerName)
            ? "Player"
            : editingData.playerName;
    }

    private void OnEditNameClicked()
    {
        UIManager?.Open<EditNamePopup>();
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

        saveManager.PlayerData.profileData.selectedAvatarId = editingData.selectedAvatarId;
        saveManager.PlayerData.profileData.selectedFrameId  = editingData.selectedFrameId;
        saveManager.PlayerData.profileData.selectedBadgeId  = editingData.selectedBadgeId;
        saveManager.PlayerData.profileData.playerName       = editingData.playerName;

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
            Debug.Log("[EditProfilePopup] Chưa save.");

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
        if (closeButton    != null) closeButton.onClick.AddListener(OnCloseClicked);
        if (saveButton     != null) saveButton.onClick.AddListener(OnSaveClicked);
        if (editNameButton != null) editNameButton.onClick.AddListener(OnEditNameClicked);

        _avatarTabButton?.Initialize(OnAvatarTabClicked);
        _frameTabButton?.Initialize(OnFrameTabClicked);
        _badgeTabButton?.Initialize(OnBadgeTabClicked);
    }

    private void UnregisterButtons()
    {
        if (closeButton    != null) closeButton.onClick.RemoveListener(OnCloseClicked);
        if (saveButton     != null) saveButton.onClick.RemoveListener(OnSaveClicked);
        if (editNameButton != null) editNameButton.onClick.RemoveListener(OnEditNameClicked);
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
        // Inspector refs
        if (closeButton      == null) Debug.LogError("[EditProfilePopup] closeButton is not assigned.",      this);
        if (saveButton       == null) Debug.LogError("[EditProfilePopup] saveButton is not assigned.",       this);
        if (nameText         == null) Debug.LogError("[EditProfilePopup] nameText is not assigned.",         this);
        if (editNameButton   == null) Debug.LogError("[EditProfilePopup] editNameButton is not assigned.",   this);
        if (profilePreview   == null) Debug.LogError("[EditProfilePopup] profilePreview is not assigned.",   this);
        if (_avatarTabButton == null) Debug.LogError("[EditProfilePopup] avatarTabButton is not assigned.",  this);
        if (_frameTabButton  == null) Debug.LogError("[EditProfilePopup] frameTabButton is not assigned.",   this);
        if (_badgeTabButton  == null) Debug.LogError("[EditProfilePopup] badgeTabButton is not assigned.",   this);
        if (avatarContent    == null) Debug.LogError("[EditProfilePopup] avatarContent is not assigned.",    this);
        if (frameContent     == null) Debug.LogError("[EditProfilePopup] frameContent is not assigned.",     this);
        if (badgeContent     == null) Debug.LogError("[EditProfilePopup] badgeContent is not assigned.",     this);
        if (avatarItemPrefab == null) Debug.LogError("[EditProfilePopup] avatarItemPrefab is not assigned.", this);
        if (frameItemPrefab  == null) Debug.LogError("[EditProfilePopup] frameItemPrefab is not assigned.",  this);
        if (badgeItemPrefab  == null) Debug.LogError("[EditProfilePopup] badgeItemPrefab is not assigned.",  this);
        if (playerProfile    == null) Debug.LogError("[EditProfilePopup] playerProfile is not assigned.",    this);
        if (scrollRect       == null) Debug.LogError("[EditProfilePopup] scrollRect is not assigned.",       this);

        // Injected dependencies
        if (saveManager              == null) Debug.LogError("[EditProfilePopup] saveManager is null. Check VContainer registration.",   this);
        if (saveManager?.PlayerData  == null) Debug.LogError("[EditProfilePopup] saveManager.PlayerData is null.",                       this);
    }
}
