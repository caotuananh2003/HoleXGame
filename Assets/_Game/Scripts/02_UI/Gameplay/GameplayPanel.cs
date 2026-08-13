using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using VContainer;

/// <summary>
/// Panel chính trong gameplay. Mode = Persistent (luôn hiện trong suốt ván chơi).
/// Layout:
///   - Góc trên phải : nút Setting mở SettingPopup
///   - Giữa trên     : ObjectivesContainer (danh sách objective items)
///   - Dưới cùng     : hàng ItemSlot để dùng item
///
/// Wire tất cả references qua Inspector.
/// </summary>
public class GameplayPanel : UIWindow
{
    [Header("Buttons")]
    [SerializeField] private Button settingButton;

    [Header("Objectives")]
    [SerializeField] private Transform objectivesContainer;
    [SerializeField] private ObjectiveUIItem objectiveItemPrefab;

    [Header("Legacy - Obstacle Counter")]
    [SerializeField] private ObstacleCounter obstacleCounter; // Deprecated: Sẽ xóa sau

    [Header("Item Bar")]
    [SerializeField] private Transform gameItemsContainer;  // GameplayCanvas/GameplayPanel/GameItems
    [SerializeField] private ItemSlotUI itemSlotPrefab;     // Prefab có ItemSlotUI component
    [SerializeField] private ItemDatabase itemDatabase;     // SO chứa danh sách ItemDefinition

    // Runtime — các slot được spawn động
    private readonly List<ItemSlotUI> spawnedSlots = new List<ItemSlotUI>();

    // Runtime
    private Dictionary<LevelObjective, ObjectiveUIItem> objectiveItemMap = new Dictionary<LevelObjective, ObjectiveUIItem>();
    private GameplayObjectiveManager gameplayObjectiveManager;
    private ItemManager itemManager;

    [Inject]
    private void Construct(GameplayObjectiveManager gameplayObjectiveManager, ItemManager itemManager)
    {
        this.gameplayObjectiveManager = gameplayObjectiveManager;
        this.itemManager = itemManager;
    }

    private void Start()
    {
        if (settingButton != null)
            settingButton.onClick.AddListener(OnSettingClicked);

        InitializeItemSlots();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Spawn một ItemSlotUI prefab cho mỗi ItemDefinition trong ItemDatabase,
    /// gắn vào gameItemsContainer, rồi gọi Setup() với quantity hiện tại.
    ///
    /// Quantity lấy từ SaveManager qua ItemManager.GetQuantity().
    /// Nếu item chưa có trong save (lần đầu chơi), dùng ItemDefinition.DefaultAmount
    /// và ghi ngay vào PlayerData để save sau này đúng.
    /// </summary>
    private void InitializeItemSlots()
    {
        if (gameItemsContainer == null)
        {
            Debug.LogWarning("[GameplayPanel] gameItemsContainer chưa được gán — không thể spawn item slots.");
            return;
        }

        if (itemSlotPrefab == null)
        {
            Debug.LogWarning("[GameplayPanel] itemSlotPrefab chưa được gán — không thể spawn item slots.");
            return;
        }

        if (itemDatabase == null)
        {
            Debug.LogWarning("[GameplayPanel] itemDatabase chưa được gán — không thể spawn item slots.");
            return;
        }

        if (itemManager == null)
        {
            Debug.LogWarning("[GameplayPanel] ItemManager chưa được inject — không thể spawn item slots.");
            return;
        }

        // Xóa slot cũ nếu gọi lại (ví dụ: scene reload)
        ClearItemSlots();

        foreach (ItemDefinition itemDef in itemDatabase.Items)
        {
            if (itemDef == null) continue;

            // Lấy quantity — nếu chưa có trong save thì dùng defaultAmount
            int quantity = GetOrInitQuantity(itemDef);

            // Spawn prefab
            ItemSlotUI slot = Instantiate(itemSlotPrefab, gameItemsContainer);
            slot.Setup(itemDef, quantity);
            slot.OnClicked += OnItemSlotClicked;

            spawnedSlots.Add(slot);
        }

        // Subscribe ItemManager events để refresh UI khi quantity thay đổi
        itemManager.OnItemUsed += OnItemUsedHandler;
        itemManager.OnItemUseFailed += OnItemUseFailedHandler;
    }

    /// <summary>
    /// Lấy quantity hiện tại từ save data.
    /// Nếu item chưa từng được save (lần đầu), khởi tạo bằng DefaultAmount.
    /// </summary>
    private int GetOrInitQuantity(ItemDefinition itemDef)
    {
        int quantity = itemManager.GetQuantity(itemDef.ItemId);

        // GetQuantity trả về 0 khi chưa có entry — cần phân biệt "đã save = 0" và "chưa từng save"
        // Dùng SaveManager.PlayerData.itemQuantities.ContainsKey để check
        if (quantity == 0 && !itemManager.HasQuantityEntry(itemDef.ItemId))
        {
            quantity = itemDef.DefaultAmount;
            itemManager.SetQuantity(itemDef.ItemId, quantity);
        }

        return quantity;
    }

    /// <summary>
    /// Hủy tất cả slot đã spawn và clear list.
    /// </summary>
    private void ClearItemSlots()
    {
        foreach (ItemSlotUI slot in spawnedSlots)
        {
            if (slot == null) continue;
            slot.OnClicked -= OnItemSlotClicked;
            Destroy(slot.gameObject);
        }
        spawnedSlots.Clear();
    }

    /// <summary>
    /// Setup objectives UI. Gọi từ GameplayController.
    /// </summary>
    public void SetupObjectives(List<LevelObjective> objectives)
    {
        if (objectives == null || objectivesContainer == null || objectiveItemPrefab == null)
        {
            Debug.LogWarning("[GameplayPanel] Cannot setup objectives. Missing references.");
            return;
        }

        // Clear old items
        foreach (var item in objectiveItemMap.Values)
        {
            if (item != null) Destroy(item.gameObject);
        }
        objectiveItemMap.Clear();

        // Spawn UI items cho từng objective
        foreach (var objective in objectives)
        {
            ObjectiveUIItem item = Instantiate(objectiveItemPrefab, objectivesContainer);
            item.Initialize(objective);
            objectiveItemMap[objective] = item;
        }

        gameplayObjectiveManager.OnObjectiveUpdated   += OnObjectiveUpdated;
        gameplayObjectiveManager.OnObjectiveCompleted += OnObjectiveCompleted;

        Debug.Log($"[GameplayPanel] Setup {objectiveItemMap.Count} objective items.");
    }

    private void OnObjectiveUpdated(LevelObjective objective)
    {
        if (objectiveItemMap.TryGetValue(objective, out ObjectiveUIItem item))
            item.UpdateProgress();
    }

    private void OnObjectiveCompleted(LevelObjective objective)
    {
        if (!objectiveItemMap.TryGetValue(objective, out ObjectiveUIItem item)) return;

        // Xóa khỏi map ngay để tránh UpdateProgress gọi lại sau khi đã destroy
        objectiveItemMap.Remove(objective);

        // Chạy animation — ContentSizeFitter tự recalculate sau khi Destroy xong
        item.PlayCompleteAnimation();
    }

    /// <summary>
    /// [LEGACY] Gọi từ GameplayController sau khi khởi tạo, truyền target obstacle count.
    /// Deprecated: Sẽ thay bằng SetupObjectives.
    /// </summary>
    public void Setup(int targetObstacleCount)
    {
        obstacleCounter?.Setup(targetObstacleCount);
    }

    /// <summary>
    /// [LEGACY] Gọi mỗi khi hole ăn được 1 object.
    /// Deprecated: ObjectiveManager sẽ tự update UI.
    /// </summary>
    public void OnObjectSwallowed()
    {
        obstacleCounter?.IncrementEaten();
    }

    // ── Item Slot Handlers ────────────────────────────────────────────────────

    private void OnItemSlotClicked(ItemDefinition item)
    {
        if (itemManager == null || item == null) return;

        bool success = itemManager.UseItem(item);

        if (!success)
        {
            Debug.Log($"[GameplayPanel] Failed to use item '{item.ItemId}'.");
            // TODO: Hiển thị feedback UI (ví dụ: shake slot, show toast)
        }
    }

    private void OnItemUsedHandler(string itemId)
    {
        // Refresh quantity text trên slot tương ứng
        RefreshItemSlotQuantity(itemId);
    }

    private void OnItemUseFailedHandler(string itemId, string reason)
    {
        Debug.Log($"[GameplayPanel] Item '{itemId}' use failed: {reason}");
        // TODO: Show feedback UI
    }

    private void RefreshItemSlotQuantity(string itemId)
    {
        if (itemManager == null) return;

        foreach (ItemSlotUI slot in spawnedSlots)
        {
            if (slot == null) continue;

            // ItemSlotUI expose ItemId qua property để tìm đúng slot
            if (slot.ItemId == itemId)
            {
                slot.UpdateQuantity(itemManager.GetQuantity(itemId));
                break;
            }
        }
    }

    ///// <summary>
    ///// Thiết lập dữ liệu cho từng item slot theo index.
    ///// </summary>
    //public void SetupItem(int index, Sprite icon, string label)
    //{
    //    if (itemSlots == null || index < 0 || index >= itemSlots.Length) return;
    //    itemSlots[index].Setup(icon, label);
    //}

    // ── Private ───────────────────────────────────────────────────────────────

    private void OnSettingClicked()
    {
        UIManager?.PlaySFX("sfx_ui_click");
        UIManager?.Open<SettingPopup>();
    }

    private void OnDestroy()
    {
        if (settingButton != null)
            settingButton.onClick.RemoveListener(OnSettingClicked);

        if (gameplayObjectiveManager != null)
        {
            gameplayObjectiveManager.OnObjectiveUpdated   -= OnObjectiveUpdated;
            gameplayObjectiveManager.OnObjectiveCompleted -= OnObjectiveCompleted;
        }

        // Unsubscribe và clear spawned item slots
        ClearItemSlots();

        if (itemManager != null)
        {
            itemManager.OnItemUsed -= OnItemUsedHandler;
            itemManager.OnItemUseFailed -= OnItemUseFailedHandler;
        }
    }
}
