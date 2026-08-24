using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class ProfilePopup : PopupWindow
{
    [Header("Navigation")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button editButton;

    [Header("Name")]
    [SerializeField] private TMP_Text nameText;

    [Header("Preview")]
    [SerializeField] private ProfilePreview profilePreview; // ChildObject

    [Header("Data")]
    [SerializeField] private PlayerProfile playerProfile; // ScriptableObject chứa Data

    private SaveManager saveManager;

    [Inject]
    private void Construct(SaveManager saveManager)
    {
        this.saveManager = saveManager;
    }

    public override void Open()
    {
        base.Open();
        UIManager?.PlaySFX(AudioID.SFX.UiPopup);
    }

    private void OnEnable() // UIWindow — sync preview mỗi lần popup mở
    {
        RefreshPreview();
    }

    private void Start()
    {
        ValidateInspectorRefs();
        RegisterButtons();

        profilePreview.Init(
            playerProfile.AvatarDatabase,
            playerProfile.FrameDatabase,
            playerProfile.BadgeDatabase);

        RefreshPreview();
    }

    public void RefreshPreview() // Refresh preview theo dữ liệu hiện tại trong SaveManager.
    {
        if (saveManager?.PlayerData == null)
        {
            Debug.LogWarning("[ProfilePopup] SaveManager.Data is null. Cannot refresh preview.");
            return;
        }

        ProfileData profile = saveManager.PlayerData.profileData;
        profilePreview?.Refresh(profile);

        if (nameText != null)
            nameText.text = string.IsNullOrWhiteSpace(profile.playerName) ? "Player" : profile.playerName;
    }

    private void OnCloseClicked()
    {
        UIManager?.Close<ProfilePopup>();
    }

    private void OnEditClicked()
    {
        UIManager?.Open<EditProfilePopup>();
    }

    private void RegisterButtons()
    {
        closeButton.onClick.AddListener(OnCloseClicked);
        editButton.onClick.AddListener(OnEditClicked);
    }

    private void UnregisterButtons()
    {
        closeButton.onClick.RemoveListener(OnCloseClicked);
        editButton.onClick.RemoveListener(OnEditClicked);
    }

    private void ValidateInspectorRefs() // Kiểm tra xem có bị null cái ref nào trên inspector không
    {
        if (closeButton    == null) Debug.LogWarning("[ProfilePopup] closeButton is not assigned.");
        if (editButton     == null) Debug.LogWarning("[ProfilePopup] editButton is not assigned.");
        if (nameText       == null) Debug.LogWarning("[ProfilePopup] nameText is not assigned.");
        if (profilePreview == null) Debug.LogWarning("[ProfilePopup] profilePreview is not assigned.");
        if (playerProfile  == null) Debug.LogWarning("[ProfilePopup] playerProfile is not assigned.");
    }
    private void OnDestroy()
    {
        UnregisterButtons();
    }
}
