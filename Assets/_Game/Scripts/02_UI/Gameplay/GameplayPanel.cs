using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel chính trong gameplay. Mode = Persistent (luôn hiện trong suốt ván chơi).
/// Layout:
///   - Góc trên phải : nút Setting mở SettingPopup
///   - Giữa trên     : ObstacleCounter (số object cần ăn)
///   - Dưới cùng     : hàng ItemSlot để dùng item
///
/// Wire tất cả references qua Inspector.
/// </summary>
public class GameplayPanel : UIWindow
{
    [Header("Buttons")]
    [SerializeField] private Button settingButton;

    [Header("Obstacle Counter")]
    [SerializeField] private ObstacleCounter obstacleCounter;

    [Header("Item Bar")]
    [SerializeField] private ItemSlotUI[] itemSlots;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        if (settingButton != null)
            settingButton.onClick.AddListener(OnSettingClicked);
    }

    private void OnDestroy()
    {
        if (settingButton != null)
            settingButton.onClick.RemoveListener(OnSettingClicked);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Gọi từ GameplayController sau khi khởi tạo, truyền target obstacle count.
    /// </summary>
    public void Setup(int targetObstacleCount)
    {
        obstacleCounter?.Setup(targetObstacleCount);
    }

    /// <summary>
    /// Gọi mỗi khi hole ăn được 1 object.
    /// </summary>
    public void OnObjectSwallowed()
    {
        obstacleCounter?.IncrementEaten();
    }

    /// <summary>
    /// Thiết lập dữ liệu cho từng item slot theo index.
    /// </summary>
    public void SetupItem(int index, Sprite icon, string label)
    {
        if (itemSlots == null || index < 0 || index >= itemSlots.Length) return;
        itemSlots[index].Setup(icon, label);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void OnSettingClicked()
    {
        UIManager?.PlaySFX("sfx_ui_click");
        UIManager?.Open<SettingPopup>();
    }
}
