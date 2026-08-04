using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Database chứa tất cả BadgeDefinition.
/// Tạo asset: Scriptable Objects → Badge Database.
/// Gán vào PlayerProfile.BadgeDatabase trong Inspector.
/// </summary>
[CreateAssetMenu(
    fileName = "BadgeDatabase",
    menuName = "Database/Badge Database")]
public class BadgeDatabase : ScriptableObject
{
    [SerializeField] private List<BadgeDefinition> badges = new();

    public IReadOnlyList<BadgeDefinition> Badges => badges;

    /// <summary>Tìm BadgeDefinition theo id. Trả null nếu không tìm thấy.</summary>
    public BadgeDefinition GetById(string id)
    {
        return badges.Find(x => x.Id == id);
    }
}
