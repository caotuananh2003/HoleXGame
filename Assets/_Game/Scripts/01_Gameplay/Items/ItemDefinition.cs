using UnityEngine;

/// <summary>
/// ScriptableObject định nghĩa một power-up item.
/// Data-driven: Designer tạo asset cho từng item (Mega Hole, Time Boost, Magnet, Shield...).
///
/// Một item có thể chứa nhiều effects — ví dụ:
///   Mega Hole = IncreaseSizeEffect + TimeExtensionEffect
///
/// itemId: dùng làm key trong SaveManager.PlayerData.itemQuantities
/// isLocked: item chưa unlock thì không sử dụng được (validate trong ItemManager)
/// defaultAmount: giá trị khởi tạo mặc định, không phải runtime quantity
///
/// Runtime quantity được lưu trong PlayerData.itemQuantities Dictionary.
/// </summary>
[CreateAssetMenu(fileName = "ItemDefinition_", menuName = "Items/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Identification")]
    [Tooltip("ID duy nhất của item — dùng làm key trong save data.")]
    [SerializeField] private string itemId;

    [Tooltip("Tên hiển thị trong UI.")]
    [SerializeField] private string itemName;

    [Header("UI")]
    [Tooltip("Icon hiển thị trong item slot.")]
    [SerializeField] private Sprite icon;

    [Tooltip("Mô tả hiển thị trong UI (optional).")]
    [TextArea(2, 4)]
    [SerializeField] private string description;

    [Header("Unlock & Quantity")]
    [Tooltip("Item đã được unlock chưa? Nếu false → không thể sử dụng.")]
    [SerializeField] private bool isLocked = true;

    [Tooltip("Số lượng mặc định khi khởi tạo player mới. Không phải runtime quantity.")]
    [SerializeField] private int defaultAmount = 3;

    [Header("Effects")]
    [Tooltip("Danh sách effect áp dụng khi sử dụng item. Có thể chứa nhiều effect.")]
    [SerializeField] private ItemEffectDefinition[] effects;

    // ── Public Properties ─────────────────────────────────────────────────────

    public string ItemId => itemId;
    public string ItemName => itemName;
    public Sprite Icon => icon;
    public string Description => description;
    public bool IsLocked => isLocked;
    public int DefaultAmount => defaultAmount;
    public ItemEffectDefinition[] Effects => effects;

    // ── Validation ────────────────────────────────────────────────────────────

    private void OnValidate() // Hàm chỉ gọi trong Unity, luôn gọi mỗi khi có sự thay đổi trên object
    {
        // Auto-generate ID từ file name nếu chưa có
        if (string.IsNullOrEmpty(itemId))
        {
            itemId = name.Replace("ItemDefinition_", "").ToLower();
        }

        // Validate effects không null
        if (effects != null)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i] == null)
                {
                    Debug.LogWarning($"[ItemDefinition] {name} has null effect at index {i}.", this);
                }
            }
        }
    }
}
