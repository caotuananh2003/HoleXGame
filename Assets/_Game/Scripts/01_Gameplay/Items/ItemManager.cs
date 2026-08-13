using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using System;
/// <summary>
/// Quản lý việc sử dụng item trong gameplay.
/// Không chứa logic cụ thể của từng effect — ủy thác cho ItemEffectDefinition.ApplyEffect().
///
/// Trách nhiệm:
///   1. Validate: item tồn tại, đã unlock, quantity > 0
///   2. Apply: gọi tất cả effects của item
///   3. Consume: trừ quantity
///   4. Save: lưu quantity mới xuống disk
///
/// Inject dependencies:
///   - SaveManager: đọc/ghi quantity
///   - HoleController, GameTimer: truyền vào ItemEffectContext
///
/// Single Responsibility: chỉ điều phối flow sử dụng item, không biết detail từng effect.
/// Open/Closed: thêm effect mới không cần sửa ItemManager.
/// </summary>
public class ItemManager : MonoBehaviour
{
    // ── Dependency Injection ──────────────────────────────────────────────────
    private SaveManager saveManager;

    [Inject]
    private void Construct(SaveManager saveManager)
    {
        this.saveManager = saveManager;
    }

    // ── Runtime refs — tìm khi cần sử dụng ────────────────────────────────────
    private HoleController holeController;
    private GameTimer gameTimer;

    // ── Events ────────────────────────────────────────────────────────────────
    /// <summary>Fire khi item được sử dụng thành công. Tham số: itemId.</summary>
    public event Action<string> OnItemUsed;

    /// <summary>Fire khi item không thể sử dụng (locked/no quantity). Tham số: itemId, reason.</summary>
    public event Action<string, string> OnItemUseFailed;

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>
    /// Sử dụng item. Validate → Apply Effects → Consume → Save.
    /// Trả về true nếu thành công, false nếu fail validation.
    /// </summary>
    public bool UseItem(ItemDefinition item)
    {
        if (item == null)
        {
            Debug.LogWarning("[ItemManager] Item is null — cannot use.");
            OnItemUseFailed?.Invoke("", "Item is null");
            return false;
        }

        // ── Validation ────────────────────────────────────────────────────────
        if (!ValidateItem(item, out string failReason))
        {
            Debug.Log($"[ItemManager] Cannot use item '{item.ItemId}': {failReason}");
            OnItemUseFailed?.Invoke(item.ItemId, failReason);
            return false;
        }

        // ── Apply Effects ─────────────────────────────────────────────────────
        ApplyItemEffects(item);

        // ── Consume Quantity ──────────────────────────────────────────────────
        ConsumeItem(item);

        // ── Save ──────────────────────────────────────────────────────────────
        saveManager.Save().Forget();

        OnItemUsed?.Invoke(item.ItemId);
        Debug.Log($"[ItemManager] Used item '{item.ItemId}'. Remaining: {GetQuantity(item.ItemId)}");

        return true;
    }

    /// <summary>
    /// Lấy số lượng item có id = itemId hiện tại từ SaveManager.
    /// Nếu item chưa có trong dictionary, trả về ItemDefinition.DefaultAmount.
    /// </summary>
    public int GetQuantity(string itemId)
    {
        if (saveManager?.PlayerData == null) return 0;

        if (saveManager.PlayerData.itemQuantities.TryGetValue(itemId, out int quantity))
            return quantity;

        // Item chưa có trong save data — trả về defaultAmount từ definition
        // (hoặc 0 nếu không tìm thấy definition — cần ItemDatabase để lookup)
        return 0;
    }

    /// <summary>
    /// Set số lượng item (dùng cho debug/cheat hoặc reward system).
    /// </summary>
    public void SetQuantity(string itemId, int quantity)
    {
        if (saveManager?.PlayerData == null) return;

        saveManager.PlayerData.itemQuantities[itemId] = Mathf.Max(0, quantity);
        saveManager.Save().Forget();

        Debug.Log($"[ItemManager] Set '{itemId}' quantity to {quantity}.");
    }

    /// <summary>
    /// Kiểm tra item đã từng được lưu vào save data chưa.
    /// Dùng để phân biệt "chưa có entry" (lần đầu chơi) với "đã save = 0".
    /// </summary>
    public bool HasQuantityEntry(string itemId)
    {
        if (saveManager?.PlayerData == null) return false;
        return saveManager.PlayerData.itemQuantities.ContainsKey(itemId);
    }

    /// <summary>
    /// Thêm quantity cho item (dùng cho reward system).
    /// </summary>
    public void AddQuantity(string itemId, int amount)
    {
        if (saveManager?.PlayerData == null) return;

        int current = GetQuantity(itemId);
        SetQuantity(itemId, current + amount);
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private bool ValidateItem(ItemDefinition item, out string failReason)
    {
        failReason = "";

        // Check locked
        if (item.IsLocked)
        {
            failReason = "Item is locked";
            return false;
        }

        // Check quantity
        int quantity = GetQuantity(item.ItemId);
        if (quantity <= 0)
        {
            failReason = "No quantity remaining";
            return false;
        }

        // Check effects not empty
        if (item.Effects == null || item.Effects.Length == 0)
        {
            failReason = "Item has no effects";
            return false;
        }

        return true;
    }

    private void ApplyItemEffects(ItemDefinition item)
    {
        // Lazy-find dependencies khi cần dùng lần đầu
        if (holeController == null)
            holeController = FindAnyObjectByType<HoleController>();

        if (gameTimer == null)
            gameTimer = FindAnyObjectByType<GameTimer>();

        if (holeController == null)
        {
            Debug.LogWarning("[ItemManager] HoleController not found — cannot apply effects.");
            return;
        }

        // Build context
        ItemEffectContext context = new ItemEffectContext(
            holeController,
            gameTimer,
            holeController.transform
        );

        // Apply tất cả effects
        foreach (ItemEffectDefinition effectDef in item.Effects)
        {
            if (effectDef == null)
            {
                Debug.LogWarning($"[ItemManager] Item '{item.ItemId}' has null effect — skipping.");
                continue;
            }

            effectDef.ApplyEffect(context);
        }
    }

    private void ConsumeItem(ItemDefinition item)
    {
        if (saveManager?.PlayerData == null) return;

        int current = GetQuantity(item.ItemId);
        saveManager.PlayerData.itemQuantities[item.ItemId] = Mathf.Max(0, current - 1);
    }

    // =========================================================================
    // Lifecycle
    // =========================================================================

    private void OnDestroy()
    {
        OnItemUsed = null;
        OnItemUseFailed = null;
    }
}
