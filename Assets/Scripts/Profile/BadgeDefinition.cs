using UnityEngine;

/// <summary>
/// Định nghĩa một Badge (huy hiệu hiển thị trên Profile).
/// Tạo asset: Scriptable Objects → Badge Definition.
/// </summary>
[CreateAssetMenu(
    fileName = "BadgeDefinition",
    menuName = "Definition/Badge Definition")]
public class BadgeDefinition : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private Sprite icon;
    [SerializeField] private string displayName;
    [SerializeField] private bool unlockedByDefault = true;

    /// <summary>Định danh duy nhất — dùng để lưu vào SaveData.</summary>
    public string Id => id;

    /// <summary>Sprite icon hiển thị trong ScrollView và Preview.</summary>
    public Sprite Icon => icon;

    /// <summary>Tên hiển thị cho người chơi.</summary>
    public string DisplayName => displayName;

    /// <summary>Mặc định mở khóa hay cần điều kiện unlock.</summary>
    public bool UnlockedByDefault => unlockedByDefault;
}
