using UnityEngine;

/// <summary>
/// Định nghĩa một Avatar Frame (viền ảnh đại diện).
/// Tạo asset: HoleXGame → Frame Definition.
/// </summary>
[CreateAssetMenu(
    fileName = "FrameDefinition",
    menuName = "Definition/Frame Definition")]
public class FrameDefinition : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private Sprite frame;
    [SerializeField] private string displayName;
    [SerializeField] private bool unlockedByDefault = true;

    /// <summary>Định danh duy nhất — dùng để lưu vào SaveData.</summary>
    public string Id => id;

    /// <summary>Sprite viền overlay lên Avatar trong Preview.</summary>
    public Sprite Frame => frame;

    /// <summary>Tên hiển thị cho người chơi.</summary>
    public string DisplayName => displayName;

    /// <summary>Mặc định mở khóa hay cần điều kiện unlock.</summary>
    public bool UnlockedByDefault => unlockedByDefault;
}
