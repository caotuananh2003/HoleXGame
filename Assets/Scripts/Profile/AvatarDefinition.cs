using UnityEngine;

/// <summary>
/// Định nghĩa một Avatar.
/// Tạo asset: Scriptable Objects → Avatar Definition.
/// </summary>
[CreateAssetMenu(
    fileName = "AvatarDefinition",
    menuName = "Definition/Avatar Definition")]
public class AvatarDefinition : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private Sprite avatar;
    [SerializeField] private string displayName;
    [SerializeField] private bool unlockedByDefault = true;

    /// <summary>Định danh duy nhất — dùng để lưu vào SaveData.</summary>
    public string Id => id;

    /// <summary>Sprite hiển thị trong ScrollView và Preview.</summary>
    public Sprite Avatar => avatar;

    /// <summary>Tên hiển thị cho người chơi.</summary>
    public string DisplayName => displayName;

    /// <summary>Mặc định mở khóa hay cần điều kiện unlock.</summary>
    public bool UnlockedByDefault => unlockedByDefault;
}
