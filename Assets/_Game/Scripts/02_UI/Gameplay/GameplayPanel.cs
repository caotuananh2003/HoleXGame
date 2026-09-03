using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Item Bar")]
    [SerializeField] private Transform gameItemsContainer;  // GameplayCanvas/GameplayPanel/GameItems
    [SerializeField] private ItemSlotUI itemSlotPrefab;     // Prefab có ItemSlotUI component
    [SerializeField] private ItemDatabase itemDatabase;     // SO chứa danh sách ItemDefinition

    public ItemDatabase ItemDatabase => itemDatabase;
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;

    private HoleSizeController holeSizeController;
    private HoleController holeController;
    private GameTimer gameTimer;

    // Runtime — các slot được spawn động
    private readonly List<ItemSlotUI> spawnedSlots = new List<ItemSlotUI>();

    // Runtime
    private Dictionary<LevelObjective, ObjectiveUIItem> objectiveItemMap = new Dictionary<LevelObjective, ObjectiveUIItem>();
    private GameplayObjectiveManager _gameplayObjectiveManager;
    private ItemManager _itemManager;

    private void Awake()
    {
        holeSizeController = FindAnyObjectByType<HoleSizeController>(FindObjectsInactive.Include);
        holeController     = FindAnyObjectByType<HoleController>(FindObjectsInactive.Include);
        gameTimer          = FindAnyObjectByType<GameTimer>(FindObjectsInactive.Include);
    }

    private void Start()
    {
        // ItemManager sống ngoài gameplayGroup nên Instance đã có lúc Start() chạy
        _itemManager = ItemManager.Instance;

        if (settingButton != null)
            settingButton.onClick.AddListener(OnSettingClicked);

        InitializeItemSlots();

        if (gameTimer != null)
        {
            gameTimer.OnTick += OnTick;
        }

        if (holeSizeController != null)
        {
            holeSizeController.OnScoreAdded += OnScoreAdded;
        }

        UpdateScore(0);
    }

    private void OnScoreAdded(int delta) // Khi holeSizeController fire OnScoreAdded (của chính nó) thì sẽ gọi hàm này
    {
        // Lấy tổng điểm từ HoleController
        if (holeController != null)
            UpdateScore(holeController.Score);
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    private void OnTick(float remaining)
    {
        if (timerText == null) return;

        if (remaining <= 0f)
        {
            timerText.text = "00:00";
            return;
        }

        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining - minutes * 60f);
        timerText.text = $"{minutes:0}:{seconds:00}";
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

        if (_itemManager == null)
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

            // Lấy unlock state từ ItemManager
            bool unlocked = _itemManager.IsItemUnlocked(itemDef);

            // Spawn prefab
            ItemSlotUI slot = Instantiate(itemSlotPrefab, gameItemsContainer);
            slot.Setup(itemDef, quantity, unlocked);
            slot.Initialize(_itemManager); // Inject ItemManager để subscribe OnItemEffectStarted
            slot.OnClicked += OnItemSlotClicked;

            spawnedSlots.Add(slot);
        }

        // Subscribe ItemManager events để refresh UI khi quantity / unlock thay đổi
        _itemManager.OnItemUsed      += OnItemUsedHandler;
        _itemManager.OnItemUseFailed += OnItemUseFailedHandler;
        _itemManager.OnItemUnlocked  += OnItemUnlockedHandler;

        // Check và unlock những item đủ điều kiện dựa trên số màn đã vượt qua
        _itemManager.CheckAndUnlockItems(itemDatabase);
    }

    /// <summary>
    /// Lấy quantity hiện tại từ save data.
    /// Nếu item chưa từng được save (lần đầu), khởi tạo bằng DefaultAmount.
    /// </summary>
    private int GetOrInitQuantity(ItemDefinition itemDef)
    {
        int quantity = _itemManager.GetQuantity(itemDef.ItemId);

        // GetQuantity trả về 0 khi chưa có entry — cần phân biệt "đã save = 0" và "chưa từng save"
        // Dùng SaveManager.PlayerData.itemQuantities.ContainsKey để check
        if (quantity == 0 && !_itemManager.HasQuantityEntry(itemDef.ItemId))
        {
            quantity = itemDef.DefaultAmount;
            _itemManager.SetQuantity(itemDef.ItemId, quantity);
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
        // GameplayObjectiveManager nằm trong gameplayGroup (inactive lúc Start) →
        // Instance chỉ có sau khi gameplayGroup.SetActive(true), resolve lazy ở đây.
        _gameplayObjectiveManager ??= GameplayObjectiveManager.Instance;

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

        if (_gameplayObjectiveManager != null)
        {
            _gameplayObjectiveManager.OnObjectiveUpdated   += OnObjectiveUpdated;
            _gameplayObjectiveManager.OnObjectiveCompleted += OnObjectiveCompleted;
        }
        else
        {
            Debug.LogWarning("[GameplayPanel] gameplayObjectiveManager is null — objective events not subscribed.");
        }

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

    // ── Item Slot Handlers ────────────────────────────────────────────────────

    private void OnItemSlotClicked(ItemDefinition item)
    {
        if (_itemManager == null || item == null) return;

        bool success = _itemManager.UseItem(item);

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

    /// <summary>
    /// Callback khi ItemManager fire OnItemUnlocked.
    /// Tìm đúng slot và gọi RefreshUnlockState(true).
    /// </summary>
    private void OnItemUnlockedHandler(string itemId)
    {
        foreach (ItemSlotUI slot in spawnedSlots)
        {
            if (slot == null) continue;
            if (slot.ItemId != itemId) continue;

            slot.RefreshUnlockState(true);
            Debug.Log($"[GameplayPanel] Slot '{itemId}' unlocked — UI refreshed.");
            break;
        }
    }

    private void RefreshItemSlotQuantity(string itemId)
    {
        if (_itemManager == null) return;

        foreach (ItemSlotUI slot in spawnedSlots)
        {
            if (slot == null) continue;

            // ItemSlotUI expose ItemId qua property để tìm đúng slot
            if (slot.ItemId == itemId)
            {
                slot.UpdateQuantity(_itemManager.GetQuantity(itemId));
                break;
            }
        }
    }

    private void OnSettingClicked()
    {
        UIManager?.PlaySFX(AudioID.SFX.UiClick);
        UIManager?.Open<SettingPopup>();
    }

    private void OnDestroy()
    {
        if (settingButton != null)
            settingButton.onClick.RemoveListener(OnSettingClicked);

        if (_gameplayObjectiveManager != null)
        {
            _gameplayObjectiveManager.OnObjectiveUpdated   -= OnObjectiveUpdated;
            _gameplayObjectiveManager.OnObjectiveCompleted -= OnObjectiveCompleted;
        }

        // Unsubscribe và clear spawned item slots
        ClearItemSlots();

        if (_itemManager != null)
        {
            _itemManager.OnItemUsed      -= OnItemUsedHandler;
            _itemManager.OnItemUseFailed -= OnItemUseFailedHandler;
            _itemManager.OnItemUnlocked  -= OnItemUnlockedHandler;
        }

        if (gameTimer != null)
            gameTimer.OnTick -= OnTick;

        if (holeSizeController != null)
            holeSizeController.OnScoreAdded -= OnScoreAdded;
    }
}
