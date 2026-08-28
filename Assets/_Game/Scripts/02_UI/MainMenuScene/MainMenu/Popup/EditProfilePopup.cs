using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditProfilePopup : PopupWindow
{
    private enum ProfileTab { Avatar, Frame, Badge }
    private ProfileTab activeTab = ProfileTab.Avatar;

    [Header("Navigation")]
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Button   editNameButton;
    [SerializeField] private ProfilePreview profilePreview;

    [Header("Tab Buttons")]
    [SerializeField] private TabButton _avatarTabButton;
    [SerializeField] private TabButton _frameTabButton;
    [SerializeField] private TabButton _badgeTabButton;

    [Header("Scroll Contents")]
    [SerializeField] private Transform  avatarContent;
    [SerializeField] private Transform  frameContent;
    [SerializeField] private Transform  badgeContent;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Item Prefabs")]
    [SerializeField] private AvatarItemUI avatarItemPrefab;
    [SerializeField] private FrameItemUI  frameItemPrefab;
    [SerializeField] private BadgeItemUI  badgeItemPrefab;

    [SerializeField] private Button        saveButton;
    [SerializeField] private PlayerProfile playerProfile;

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
        RegisterButtons();

        if (playerProfile != null)
            profilePreview.Init(playerProfile.AvatarDatabase, playerProfile.FrameDatabase, playerProfile.BadgeDatabase);

        if (!listsPopulated) { PopulateAllLists(); listsPopulated = true; }
        if (gameObject.activeSelf) RefreshOpenState();
    }

    private void OnEnable()  { if (!isStarted) return; RefreshOpenState(); }
    private void OnDestroy() { UnregisterButtons(); ClearAllLists(); }

    private void RefreshOpenState()
    {
        if (SaveManager.Instance?.PlayerData == null || playerProfile == null) return;
        savedSnapshot = SaveManager.Instance.PlayerData.profileData.Clone();
        editingData   = savedSnapshot.Clone();
        ShowTab(ProfileTab.Avatar);
        RefreshNameText();
        profilePreview.Refresh(editingData);
    }

    public void ApplyNameFromPopup(string newName) { editingData.playerName = newName; profilePreview.SetName(newName); RefreshNameText(); }
    public string CurrentEditingName => editingData?.playerName ?? string.Empty;

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
            scrollRect.content = tab switch { ProfileTab.Avatar => avatarContent as RectTransform, ProfileTab.Frame => frameContent as RectTransform, ProfileTab.Badge => badgeContent as RectTransform, _ => scrollRect.content };
            scrollRect.verticalNormalizedPosition = 1f;
        }
        RefreshSelectionHighlights();
    }

    private void PopulateAllLists()
    {
        if (playerProfile.AvatarDatabase != null)
            foreach (var def in playerProfile.AvatarDatabase.Avatars)
            { var item = Instantiate(avatarItemPrefab, avatarContent); item.Setup(def, def.UnlockedByDefault); item.OnClicked += OnAvatarItemClicked; avatarItems.Add(item); }

        if (playerProfile.FrameDatabase != null)
            foreach (var def in playerProfile.FrameDatabase.Frames)
            { var item = Instantiate(frameItemPrefab, frameContent); item.Setup(def, def.UnlockedByDefault); item.OnClicked += OnFrameItemClicked; frameItems.Add(item); }

        if (playerProfile.BadgeDatabase != null)
            foreach (var def in playerProfile.BadgeDatabase.Badges)
            { var item = Instantiate(badgeItemPrefab, badgeContent); item.Setup(def, def.UnlockedByDefault); item.OnClicked += OnBadgeItemClicked; badgeItems.Add(item); }
    }

    private void OnAvatarItemClicked(string id) { editingData.selectedAvatarId = id; profilePreview.SetAvatar(id); RefreshSelectionHighlights(); }
    private void OnFrameItemClicked(string id)  { editingData.selectedFrameId  = id; profilePreview.SetFrame(id);  RefreshSelectionHighlights(); }
    private void OnBadgeItemClicked(string id)  { editingData.selectedBadgeId  = id; profilePreview.SetBadge(id);  RefreshSelectionHighlights(); }

    private void RefreshSelectionHighlights()
    {
        foreach (var item in avatarItems) item.SetSelected(item.ItemId == editingData.selectedAvatarId);
        foreach (var item in frameItems)  item.SetSelected(item.ItemId == editingData.selectedFrameId);
        foreach (var item in badgeItems)  item.SetSelected(item.ItemId == editingData.selectedBadgeId);
    }

    private void RefreshNameText()
    {
        if (nameText == null) return;
        nameText.text = string.IsNullOrWhiteSpace(editingData?.playerName) ? "Player" : editingData.playerName;
    }

    private void OnEditNameClicked() => UIManager?.Open<EditNamePopup>();

    private void OnSaveClicked()
    {
        if (SaveManager.Instance?.PlayerData == null) { Debug.LogWarning("[EditProfilePopup] PlayerData is null."); return; }
        var p = SaveManager.Instance.PlayerData.profileData;
        p.selectedAvatarId = editingData.selectedAvatarId;
        p.selectedFrameId  = editingData.selectedFrameId;
        p.selectedBadgeId  = editingData.selectedBadgeId;
        p.playerName       = editingData.playerName;
        SaveManager.Instance.Save().Forget();
        savedSnapshot = editingData.Clone();
        UIManager?.GetWindow<ProfilePopup>()?.RefreshPreview();
        UIManager?.GetWindow<MainmenuPanel>()?.RefreshPreview();
    }

    private void OnCloseClicked() => UIManager?.Close<EditProfilePopup>();
    private void OnAvatarTabClicked() => ShowTab(ProfileTab.Avatar);
    private void OnFrameTabClicked()  => ShowTab(ProfileTab.Frame);
    private void OnBadgeTabClicked()  => ShowTab(ProfileTab.Badge);

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

    private void ClearAllLists()
    {
        foreach (var item in avatarItems) if (item != null) item.OnClicked -= OnAvatarItemClicked; avatarItems.Clear();
        foreach (var item in frameItems)  if (item != null) item.OnClicked -= OnFrameItemClicked;  frameItems.Clear();
        foreach (var item in badgeItems)  if (item != null) item.OnClicked -= OnBadgeItemClicked;  badgeItems.Clear();
    }
}
