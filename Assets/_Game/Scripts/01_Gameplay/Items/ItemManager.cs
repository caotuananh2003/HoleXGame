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
///   5. Unlock: check PlayerData.currentLevelIndex khi khởi tạo, unlock item đủ điều kiện
///
/// Unlock logic (dựa trên màn chơi, không phải hole level trong gameplay):
///   - IsLockedByDefault = false  → luôn unlock
///   - IsLockedByDefault = true   → cần currentLevelIndex >= item.UnlockAtLevel
///                                  VÀ có trong PlayerData.unlockedItemIds
///   - CheckAndUnlockItems() được gọi từ GameplayPanel.InitializeItemSlots() 
///     để unlock ngay những item player đã đủ điều kiện khi vào màn chơi.
///   - Không subscribe event nào trong gameplay — unlock chỉ xảy ra một lần
///     khi bắt đầu màn dựa trên save data.
///
/// Single Responsibility: điều phối flow use/unlock item, không biết detail từng effect.
/// Open/Closed: thêm effect mới không cần sửa ItemManager.
/// </summary>
public class ItemManager : MonoBehaviour
{
    // ── Dependency Injection ──────────────────────────────────────────────────
    private SaveManager  saveManager;

    [Inject]
    private void Construct(SaveManager saveManager)
    {
        this.saveManager = saveManager;
    }

    // ── Runtime refs ──────────────────────────────────────────────────────────
    private HoleController holeController;
    private GameTimer      gameTimer;

    // ── Cooldown tracking ─────────────────────────────────────────────────────
    private float       lastUseTime  = -999f;
    private const float ItemCooldown = 2f;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fire khi item được sử dụng thành công. Tham số: itemId.</summary>
    public event Action<string> OnItemUsed;

    /// <summary>Fire khi item không thể sử dụng. Tham số: itemId, reason.</summary>
    public event Action<string, string> OnItemUseFailed;

    /// <summary>
    /// Fire khi timed effect bắt đầu chạy.
    /// Tham số: itemId, ITimedEffect (null nếu instant effect).
    /// </summary>
    public event Action<string, ITimedEffect> OnItemEffectStarted;

    /// <summary>
    /// Fire khi một item được unlock.
    /// GameplayPanel subscribe để refresh slot UI tương ứng.
    /// Tham số: itemId vừa được unlock.
    /// </summary>
    public event Action<string> OnItemUnlocked;

    // =========================================================================
    // Public API — Unlock
    // =========================================================================

    /// <summary>
    /// Scan toàn bộ ItemDatabase, unlock những item đủ điều kiện dựa trên
    /// số màn chơi player đã vượt qua (PlayerData.currentLevelIndex).
    ///
    /// Gọi từ GameplayPanel.InitializeItemSlots() một lần khi vào màn.
    /// </summary>
    public void CheckAndUnlockItems(ItemDatabase itemDatabase)
    {
        if (itemDatabase == null)
        {
            Debug.LogWarning("[ItemManager] CheckAndUnlockItems: itemDatabase is null.");
            return;
        }

        if (saveManager?.PlayerData == null)
        {
            Debug.LogWarning("[ItemManager] CheckAndUnlockItems: PlayerData is null.");
            return;
        }

        // currentLevelIndex = số màn đã WIN (0 = chưa thắng màn nào)
        // Khi vào màn đầu tiên (index 0): currentLevelIndex = -1 → clamp về 0
        int completedLevels = Mathf.Max(0, saveManager.PlayerData.currentLevelIndex);

        bool anythingUnlocked = false;

        foreach (ItemDefinition item in itemDatabase.Items)
        {
            if (item == null)              continue;
            if (!item.IsLockedByDefault)   continue; // Không bao giờ bị lock

            // Đã unlock rồi thì bỏ qua
            if (saveManager.PlayerData.unlockedItemIds.Contains(item.ItemId)) continue;

            // Chưa đủ màn thì bỏ qua
            if (completedLevels < item.UnlockAtLevel) continue;

            // Đủ điều kiện — unlock
            saveManager.PlayerData.unlockedItemIds.Add(item.ItemId);
            anythingUnlocked = true;

            OnItemUnlocked?.Invoke(item.ItemId);

            Debug.Log($"[ItemManager] Unlocked '{item.ItemId}' — completedLevels={completedLevels}, required={item.UnlockAtLevel}.");
        }

        if (anythingUnlocked)
            saveManager.Save().Forget();
    }

    /// <summary>
    /// Kiểm tra item có đang được unlock không.
    ///
    /// - IsLockedByDefault = false → luôn unlock
    /// - IsLockedByDefault = true  → cần có trong PlayerData.unlockedItemIds
    /// </summary>
    public bool IsItemUnlocked(ItemDefinition item)
    {
        if (item == null) return false;

        if (!item.IsLockedByDefault) return true;

        if (saveManager?.PlayerData?.unlockedItemIds == null) return false;

        return saveManager.PlayerData.unlockedItemIds.Contains(item.ItemId);
    }

    // =========================================================================
    // Public API — Use
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

        if (!ValidateItem(item, out string failReason))
        {
            Debug.Log($"[ItemManager] Cannot use item '{item.ItemId}': {failReason}");
            OnItemUseFailed?.Invoke(item.ItemId, failReason);
            return false;
        }

        ITimedEffect timedEffect = ApplyItemEffects(item);

        ConsumeItem(item);

        lastUseTime = Time.time;

        saveManager.Save().Forget();

        OnItemUsed?.Invoke(item.ItemId);
        OnItemEffectStarted?.Invoke(item.ItemId, timedEffect);
        Debug.Log($"[ItemManager] Used item '{item.ItemId}'. Remaining: {GetQuantity(item.ItemId)}");

        return true;
    }

    // =========================================================================
    // Public API — Quantity
    // =========================================================================

    /// <summary>Lấy số lượng item hiện tại. Trả về 0 nếu chưa có entry.</summary>
    public int GetQuantity(string itemId)
    {
        if (saveManager?.PlayerData == null) return 0;

        if (saveManager.PlayerData.itemQuantities.TryGetValue(itemId, out int quantity))
            return quantity;

        return 0;
    }

    /// <summary>Set số lượng item (dùng cho debug/reward).</summary>
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

    /// <summary>Thêm quantity (dùng cho reward system).</summary>
    public void AddQuantity(string itemId, int amount)
    {
        if (saveManager?.PlayerData == null) return;

        int current = GetQuantity(itemId);
        SetQuantity(itemId, current + amount);
    }

    // =========================================================================
    // Internal — Validation
    // =========================================================================

    private bool ValidateItem(ItemDefinition item, out string failReason)
    {
        failReason = "";

        // Check cooldown
        float timeSinceLastUse = Time.time - lastUseTime;
        if (timeSinceLastUse < ItemCooldown)
        {
            float remaining = ItemCooldown - timeSinceLastUse;
            failReason = $"Item on cooldown ({remaining:F1}s remaining)";
            return false;
        }

        // Check unlock state
        if (!IsItemUnlocked(item))
        {
            failReason = $"Item is locked (requires completing {item.UnlockAtLevel} level(s))";
            return false;
        }

        // Check quantity
        int quantity = GetQuantity(item.ItemId);
        if (quantity <= 0)
        {
            failReason = "No quantity remaining";
            return false;
        }

        // Check effects
        if (item.Effects == null || item.Effects.Length == 0)
        {
            failReason = "Item has no effects";
            return false;
        }

        return true;
    }

    // =========================================================================
    // Internal — Effects
    // =========================================================================

    private ITimedEffect ApplyItemEffects(ItemDefinition item)
    {
        if (holeController == null)
            holeController = FindAnyObjectByType<HoleController>();

        if (gameTimer == null)
            gameTimer = FindAnyObjectByType<GameTimer>();

        if (holeController == null)
        {
            Debug.LogWarning("[ItemManager] HoleController not found — cannot apply effects.");
            return null;
        }

        ItemEffectContext context = new ItemEffectContext(
            holeController,
            gameTimer,
            holeController.transform
        );

        ITimedEffect result = null;
        foreach (ItemEffectDefinition effectDef in item.Effects)
        {
            if (effectDef == null)
            {
                Debug.LogWarning($"[ItemManager] Item '{item.ItemId}' has null effect — skipping.");
                continue;
            }

            ITimedEffect timed = effectDef.ApplyEffect(context);
            if (result == null && timed != null)
                result = timed;
        }

        return result;
    }

    // =========================================================================
    // Internal — Consume
    // =========================================================================

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
        OnItemUsed          = null;
        OnItemUseFailed     = null;
        OnItemEffectStarted = null;
        OnItemUnlocked      = null;
    }
}
