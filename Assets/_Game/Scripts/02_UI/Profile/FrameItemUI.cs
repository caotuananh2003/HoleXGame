using UnityEngine;

/// <summary>
/// Item hiển thị một Avatar Frame trong ScrollView của EditProfilePopup.
/// Gắn vào Frame item prefab.
///
/// Cấu trúc prefab:
///   FrameItemUI
///   ├── IconImage       (Image — sprite frame)
///   ├── SelectedOverlay (GameObject — highlight viền)
///   ├── LockOverlay     (GameObject — khóa)
///   └── Button
/// </summary>
public class FrameItemUI : BaseItemUI<FrameDefinition>
{
    protected override string GetId(FrameDefinition def)     => def.Id;
    protected override Sprite GetSprite(FrameDefinition def) => def.Frame;
}
