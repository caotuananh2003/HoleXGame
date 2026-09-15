using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Editor-only cheat controller để test item system trong gameplay.
/// Phím tắt:
///   1 — Use item slot 0
///   2 — Use item slot 1
///   3 — Use item slot 2
///   4 — Use item slot 3
///   Q — Add 1000 currency
///   E — Reset item quantities về defaultAmount
///
/// Chỉ hoạt động trong Unity Editor và khi đang ở GameplayScene.
/// </summary>
public class EditorItemCheatController : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Test Items")]
    [SerializeField] private ItemDefinition[] testItems = new ItemDefinition[4];

    private ItemManager itemManager;
    private SaveManager saveManager;

    private void Awake()
    {
        itemManager = FindAnyObjectByType<ItemManager>();
        saveManager = FindAnyObjectByType<SaveManager>();

        if (itemManager == null)
            Debug.LogWarning("[EditorItemCheatController] ItemManager not found in scene.");

        if (saveManager == null)
            Debug.LogWarning("[EditorItemCheatController] SaveManager not found in scene.");
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        if (itemManager == null || saveManager == null) return;

        // Slot 1-4: Use items
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            UseItemSlot(0);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            UseItemSlot(1);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            UseItemSlot(2);

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
            UseItemSlot(3);

        // Q: Add currency
        if (Keyboard.current.qKey.wasPressedThisFrame)
            AddCurrency();

        // E: Reset item quantities
        if (Keyboard.current.eKey.wasPressedThisFrame)
            ResetItemQuantities();
    }

    private void UseItemSlot(int index)
    {
        if (testItems == null || index < 0 || index >= testItems.Length)
        {
            Debug.LogWarning($"[EditorItemCheat] Slot {index} không hợp lệ.");
            return;
        }

        ItemDefinition item = testItems[index];
        if (item == null)
        {
            Debug.LogWarning($"[EditorItemCheat] Slot {index} chưa gán ItemDefinition.");
            return;
        }

        bool success = itemManager.UseItem(item);
        if (success)
            Debug.Log($"[EditorItemCheat] [Key {index + 1}] Used '{item.ItemName}' successfully.");
        else
            Debug.Log($"[EditorItemCheat] [Key {index + 1}] Failed to use '{item.ItemName}'.");
    }

    private void AddCurrency()
    {
        if (saveManager?.PlayerData == null) return;

        saveManager.PlayerData.currency += 1000;
        saveManager.Save().Forget();

        Debug.Log($"[EditorItemCheat] [Q] Added 1000 currency. Total: {saveManager.PlayerData.currency}");
    }

    private void ResetItemQuantities()
    {
        if (saveManager?.PlayerData == null || testItems == null) return;

        foreach (ItemDefinition item in testItems)
        {
            if (item == null) continue;
            itemManager.SetQuantity(item.ItemId, item.DefaultAmount);
        }

        Debug.Log("[EditorItemCheat] [E] Reset all item quantities to defaultAmount.");
    }

    private void OnValidate()
    {
        if (testItems == null || testItems.Length != 4)
        {
            Debug.LogWarning("[EditorItemCheatController] testItems array phải có đúng 4 phần tử.", this);
        }
    }
#endif
}
