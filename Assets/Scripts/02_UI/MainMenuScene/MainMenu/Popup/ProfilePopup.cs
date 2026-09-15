using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePopup : PopupWindow
{
    [SerializeField] private Button         closeButton;
    [SerializeField] private Button         editButton;
    [SerializeField] private TMP_Text       nameText;
    [SerializeField] private ProfilePreview profilePreview;
    [SerializeField] private PlayerProfile  playerProfile;
    private SaveManager _saveManager;

    public override void Open()
    {
        base.Open();
        UIManager?.PlaySFX(AudioID.SFX.UiPopup);
    }

    private void OnEnable() => RefreshPreview();

    private void Start()
    {
        _saveManager = SaveManager.Instance;

        closeButton.onClick.AddListener(OnCloseClicked);
        editButton.onClick.AddListener(OnEditClicked);

        profilePreview.Init(
            playerProfile.AvatarDatabase,
            playerProfile.FrameDatabase,
            playerProfile.BadgeDatabase);

        RefreshPreview();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        closeButton.onClick.RemoveListener(OnCloseClicked);
        editButton.onClick.RemoveListener(OnEditClicked);
    }

    public void RefreshPreview()
    {
        if (_saveManager?.PlayerData == null) return;
        ProfileData profile = _saveManager.PlayerData.profileData;
        profilePreview?.Refresh(profile);
        if (nameText != null)
            nameText.text = string.IsNullOrWhiteSpace(profile.playerName) ? "Player" : profile.playerName;
    }

    private void OnCloseClicked() => UIManager?.Close<ProfilePopup>();
    private void OnEditClicked()  => UIManager?.Open<EditProfilePopup>();
}
