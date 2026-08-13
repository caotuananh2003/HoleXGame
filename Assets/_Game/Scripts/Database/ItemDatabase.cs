using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject chứa toàn bộ danh sách ItemDefinition.
/// Designer thêm/xóa item tại đây — GameplayPanel sẽ tự spawn UI từ list này.
/// </summary>
[CreateAssetMenu(
    fileName = "ItemDatabase",
    menuName = "Database/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField]
    private List<ItemDefinition> items = new();

    public IReadOnlyList<ItemDefinition> Items => items;

    /// <summary>Tìm ItemDefinition theo itemId. Trả về null nếu không tìm thấy.</summary>
    public ItemDefinition GetById(string itemId)
    {
        return items.Find(x => x.ItemId == itemId);
    }
}
