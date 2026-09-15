using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Component hiển thị preview Avatar / Frame / Badge của người chơi.
/// Dùng chung giữa ProfilePopup và EditProfilePopup để tránh duplicate code.
///
/// Gắn vào GameObject "PreviewProfile" trong hierarchy của mỗi popup.
/// Các Image reference kéo trong Inspector.
///
/// Không chứa logic save/load — chỉ nhận data và set sprite.
/// </summary>
public class ProfilePreview : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image badgeImage;

    [Header("Text")]
    [SerializeField] private TMP_Text nameText;

    private AvatarDatabase avatarDatabase;
    private FrameDatabase frameDatabase;
    private BadgeDatabase badgeDatabase;

    public void Init(AvatarDatabase avatarDb, FrameDatabase frameDb, BadgeDatabase badgeDb) // Khởi tạo databases. Gọi một lần từ popup cha khi Start.
    {
        avatarDatabase = avatarDb;
        frameDatabase  = frameDb;
        badgeDatabase  = badgeDb;

        if (avatarDatabase == null) Debug.LogError("[ProfilePreview] avatarDatabase is null.", this);
        if (frameDatabase  == null) Debug.LogError("[ProfilePreview] frameDatabase is null.",  this);
        if (badgeDatabase  == null) Debug.LogError("[ProfilePreview] badgeDatabase is null.",  this);
    }

    /// <summary>
    /// Refresh toàn bộ preview theo ProfileData đầu vào.
    /// Gọi mỗi lần popup mở hoặc sau khi save.
    /// </summary>
    public void Refresh(ProfileData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[ProfilePreview] Refresh() nhận ProfileData null.");
            return;
        }

        SetAvatar(data.selectedAvatarId);
        SetFrame(data.selectedFrameId);
        SetBadge(data.selectedBadgeId);
        SetName(data.playerName);
    }

    #region Update Preview
    /// <summary>Cập nhật chỉ Avatar image — gọi khi người chơi chọn avatar mới.</summary>
    public void SetAvatar(string avatarId)
    {
        if (avatarImage == null) { Debug.LogWarning("[ProfilePreview] avatarImage is null.", this); return; }

        AvatarDefinition def = avatarDatabase != null ? avatarDatabase.GetById(avatarId) : null;
        avatarImage.sprite  = def != null ? def.Avatar : null;
    }

    /// <summary>Cập nhật chỉ Frame image — gọi khi người chơi chọn frame mới.</summary>
    public void SetFrame(string frameId)
    {
        if (frameImage == null) { Debug.LogWarning("[ProfilePreview] frameImage is null.", this); return; }

        FrameDefinition def = frameDatabase != null ? frameDatabase.GetById(frameId) : null;
        frameImage.sprite  = def != null ? def.Frame : null;
    }

    /// <summary>Cập nhật chỉ Badge image — gọi khi người chơi chọn badge mới.</summary>
    public void SetBadge(string badgeId)
    {
        if (badgeImage == null) { Debug.LogWarning("[ProfilePreview] badgeImage is null.", this); return; }

        BadgeDefinition def = badgeDatabase != null ? badgeDatabase.GetById(badgeId) : null;
        badgeImage.sprite  = def != null ? def.Icon : null;
    }

    /// <summary>Cập nhật tên hiển thị — gọi khi người chơi nhập tên mới.</summary>
    public void SetName(string name)
    {
        if (nameText == null) { Debug.LogWarning("[ProfilePreview] nameText is null.", this); return; }

        nameText.text = string.IsNullOrWhiteSpace(name) ? "Player" : name;
    }
    #endregion

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (avatarImage == null) Debug.LogError("[ProfilePreview] avatarImage is null.", this);
        if (frameImage  == null) Debug.LogError("[ProfilePreview] frameImage is null.",  this);
        if (badgeImage  == null) Debug.LogError("[ProfilePreview] badgeImage is null.",  this);
        if (nameText    == null) Debug.LogError("[ProfilePreview] nameText is null.",    this);
    }
#endif
}
