using UnityEngine;

/// <summary>
/// Item hiển thị một Badge trong ScrollView của EditProfilePopup.
/// Gắn vào Badge item prefab.
///
/// Cấu trúc prefab:
///   BadgeItemUI
///   ├── IconImage       (Image — sprite badge)
///   ├── SelectedOverlay (GameObject — highlight viền)
///   ├── LockOverlay     (GameObject — khóa)
///   └── Button
/// </summary>
public class BadgeItemUI : BaseItemUI<BadgeDefinition>
{
    protected override string GetId(BadgeDefinition def)     => def.Id;
    protected override Sprite GetSprite(BadgeDefinition def) => def.Icon;
}
