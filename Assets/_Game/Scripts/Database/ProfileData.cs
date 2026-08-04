using System;

/// <summary>
/// Dữ liệu profile runtime của người chơi.
/// Được nhúng vào PlayerData và serialize/deserialize cùng với nó.
///
/// Chỉ lưu ID — không lưu Sprite, Image, hay bất kỳ UnityEngine.Object nào.
/// Sprite được resolve tại runtime qua Database (AvatarDatabase, FrameDatabase, BadgeDatabase).
/// </summary>
[Serializable]
public class ProfileData
{
    /// <summary>ID của Avatar đang được chọn.</summary>
    public string selectedAvatarId = "";

    /// <summary>ID của Frame đang được chọn.</summary>
    public string selectedFrameId = "";

    /// <summary>ID của Badge đang được chọn.</summary>
    public string selectedBadgeId = "";

    /// <summary>
    /// Clone — tạo bản copy độc lập để dùng làm editing data trong EditProfilePopup.
    /// Thay đổi trên bản clone không ảnh hưởng đến dữ liệu đã lưu cho đến khi Save().
    /// </summary>
    public ProfileData Clone()
    {
        return new ProfileData
        {
            selectedAvatarId = selectedAvatarId,
            selectedFrameId  = selectedFrameId,
            selectedBadgeId  = selectedBadgeId,
        };
    }

    /// <summary>
    /// So sánh nội dung — dùng để kiểm tra dirty flag khi Close EditProfilePopup.
    /// </summary>
    public bool Equals(ProfileData other)
    {
        if (other == null) return false;

        return selectedAvatarId == other.selectedAvatarId
            && selectedFrameId  == other.selectedFrameId
            && selectedBadgeId  == other.selectedBadgeId;
    }
}
