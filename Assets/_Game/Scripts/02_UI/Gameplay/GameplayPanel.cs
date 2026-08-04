using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

    //[Header("Item Bar")]
    //[SerializeField] private ItemSlotUI[] itemSlots;

    // Runtime
    private Dictionary<LevelObjective, ObjectiveUIItem> objectiveItemMap = new Dictionary<LevelObjective, ObjectiveUIItem>();
    private GameplayObjectiveManager gameplayObjectiveManager;

    private void Awake()
    {
        this.gameplayObjectiveManager = FindAnyObjectByType<GameplayObjectiveManager>();
    }
    private void Start()
    {
        if (settingButton != null)
            settingButton.onClick.AddListener(OnSettingClicked);
    }

    // ── Public API ────────────────────────────────────────────────────────────

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

        gameplayObjectiveManager.OnObjectiveUpdated += OnObjectiveUpdated;

        Debug.Log($"[GameplayPanel] Setup {objectiveItemMap.Count} objective items.");
    }

    private void OnObjectiveUpdated(LevelObjective objective)
    {
        if (objectiveItemMap.TryGetValue(objective, out ObjectiveUIItem item))
            item.UpdateProgress();
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
            gameplayObjectiveManager.OnObjectiveUpdated -= OnObjectiveUpdated;
    }
}
