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
    [Tooltip("Item khóa mặc định? Nếu true → cần vượt qua đủ số màn (unlockAtLevel) mới dùng được.")]
    [SerializeField] private bool isLockedByDefault = true;

    [Tooltip("Số màn chơi tối thiểu player phải đã vượt qua để unlock item này.\n" +
             "0 = unlock ngay từ đầu.\n" +
             "Ví dụ: 3 = player phải WIN ít nhất 3 màn (currentLevelIndex >= 3) mới dùng được.")]
    [SerializeField] private int unlockAtLevel = 0;

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
    /// <summary>
    /// Item có bị khóa mặc định không? Dùng để check lần đầu — runtime unlock state
    /// được quản lý bởi ItemManager (dựa trên hole level đạt UnlockAtLevel).
    /// </summary>
    public bool IsLockedByDefault => isLockedByDefault;
    /// <summary>
    /// Số màn chơi tối thiểu player phải đã vượt qua (currentLevelIndex) để unlock item.
    /// 0 = unlock ngay từ đầu game.
    /// </summary>
    public int UnlockAtLevel => unlockAtLevel;
    public int DefaultAmount => defaultAmount;
    public ItemEffectDefinition[] Effects => effects;

    // IsLocked đã bị bỏ — runtime lock state do ItemManager quyết định

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
