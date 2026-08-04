using UnityEngine;

/// <summary>
/// Item hiển thị một Avatar trong ScrollView của EditProfilePopup.
/// Gắn vào Avatar item prefab.
///
/// Cấu trúc prefab:
///   AvatarItemUI
///   ├── IconImage       (Image — sprite avatar)
///   ├── SelectedOverlay (GameObject — highlight viền)
///   ├── LockOverlay     (GameObject — khóa)
///   └── Button
/// </summary>
public class AvatarItemUI : BaseItemUI<AvatarDefinition>
{
    protected override string GetId(AvatarDefinition def)     => def.Id;
    protected override Sprite GetSprite(AvatarDefinition def) => def.Avatar;
}
