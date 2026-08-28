using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    private void Awake()     { Instance = this; }
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        OnItemUsed = null; OnItemUseFailed = null;
        OnItemEffectStarted = null; OnItemUnlocked = null;
    }

    private HoleController holeController;
    private GameTimer      gameTimer;

    private float       lastUseTime  = -999f;
    private const float ItemCooldown = 2f;

    public event Action<string>              OnItemUsed;
    public event Action<string, string>      OnItemUseFailed;
    public event Action<string, ITimedEffect> OnItemEffectStarted;
    public event Action<string>              OnItemUnlocked;

    // ── Unlock ────────────────────────────────────────────────────────────────

    public void CheckAndUnlockItems(ItemDatabase itemDatabase)
    {
        if (itemDatabase == null) { Debug.LogWarning("[ItemManager] itemDatabase is null."); return; }
        if (SaveManager.Instance?.PlayerData == null) { Debug.LogWarning("[ItemManager] PlayerData is null."); return; }

        int completedLevels = Mathf.Max(0, SaveManager.Instance.PlayerData.currentLevelIndex);
        bool anythingUnlocked = false;

        foreach (ItemDefinition item in itemDatabase.Items)
        {
            if (item == null || !item.IsLockedByDefault) continue;
            if (SaveManager.Instance.PlayerData.unlockedItemIds.Contains(item.ItemId)) continue;
            if (completedLevels < item.UnlockAtLevel) continue;

            SaveManager.Instance.PlayerData.unlockedItemIds.Add(item.ItemId);
            anythingUnlocked = true;
            OnItemUnlocked?.Invoke(item.ItemId);
            Debug.Log($"[ItemManager] Unlocked '{item.ItemId}'.");
        }

        if (anythingUnlocked) SaveManager.Instance.Save().Forget();
    }

    public bool IsItemUnlocked(ItemDefinition item)
    {
        if (item == null) return false;
        if (!item.IsLockedByDefault) return true;
        if (SaveManager.Instance?.PlayerData?.unlockedItemIds == null) return false;
        return SaveManager.Instance.PlayerData.unlockedItemIds.Contains(item.ItemId);
    }

    // ── Use ───────────────────────────────────────────────────────────────────

    public bool UseItem(ItemDefinition item)
    {
        if (item == null)
        {
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
        SaveManager.Instance.Save().Forget();

        OnItemUsed?.Invoke(item.ItemId);
        OnItemEffectStarted?.Invoke(item.ItemId, timedEffect);
        Debug.Log($"[ItemManager] Used item '{item.ItemId}'. Remaining: {GetQuantity(item.ItemId)}");
        return true;
    }

    // ── Quantity ──────────────────────────────────────────────────────────────

    public int GetQuantity(string itemId)
    {
        if (SaveManager.Instance?.PlayerData == null) return 0;
        return SaveManager.Instance.PlayerData.itemQuantities.TryGetValue(itemId, out int q) ? q : 0;
    }

    public void SetQuantity(string itemId, int quantity)
    {
        if (SaveManager.Instance?.PlayerData == null) return;
        SaveManager.Instance.PlayerData.itemQuantities[itemId] = Mathf.Max(0, quantity);
        SaveManager.Instance.Save().Forget();
    }

    public bool HasQuantityEntry(string itemId)
    {
        if (SaveManager.Instance?.PlayerData == null) return false;
        return SaveManager.Instance.PlayerData.itemQuantities.ContainsKey(itemId);
    }

    public void AddQuantity(string itemId, int amount) => SetQuantity(itemId, GetQuantity(itemId) + amount);

    // ── Internal ──────────────────────────────────────────────────────────────

    private bool ValidateItem(ItemDefinition item, out string failReason)
    {
        failReason = "";
        float timeSince = Time.time - lastUseTime;
        if (timeSince < ItemCooldown) { failReason = $"Cooldown ({ItemCooldown - timeSince:F1}s)"; return false; }
        if (!IsItemUnlocked(item))   { failReason = $"Locked (need {item.UnlockAtLevel} levels)"; return false; }
        if (GetQuantity(item.ItemId) <= 0) { failReason = "No quantity"; return false; }
        if (item.Effects == null || item.Effects.Length == 0) { failReason = "No effects"; return false; }
        return true;
    }

    private ITimedEffect ApplyItemEffects(ItemDefinition item)
    {
        if (holeController == null) holeController = FindAnyObjectByType<HoleController>();
        if (gameTimer      == null) gameTimer      = FindAnyObjectByType<GameTimer>();

        if (holeController == null) { Debug.LogWarning("[ItemManager] HoleController not found."); return null; }

        var context = new ItemEffectContext(holeController, gameTimer, holeController.transform);
        ITimedEffect result = null;

        foreach (ItemEffectDefinition effectDef in item.Effects)
        {
            if (effectDef == null) continue;
            ITimedEffect timed = effectDef.ApplyEffect(context);
            if (result == null && timed != null) result = timed;
        }
        return result;
    }

    private void ConsumeItem(ItemDefinition item)
    {
        if (SaveManager.Instance?.PlayerData == null) return;
        int current = GetQuantity(item.ItemId);
        SaveManager.Instance.PlayerData.itemQuantities[item.ItemId] = Mathf.Max(0, current - 1);
    }
}
