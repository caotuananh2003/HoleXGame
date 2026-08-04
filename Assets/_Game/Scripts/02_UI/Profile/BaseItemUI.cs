using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base class cho Avatar / Frame / Badge item trong ScrollView.
///
/// Mỗi item prefab có cấu trúc:
///   BaseItemUI
///   ├── IconImage        — sprite của item
///   ├── SelectedOverlay  — highlight khi đang được chọn (ẩn mặc định)
///   ├── LockOverlay      — hiển thị khi item bị khóa (ẩn mặc định)
///   └── Button           — bắt sự kiện click
///
/// Subclass chỉ cần khai báo TDefinition là kiểu Definition tương ứng.
/// </summary>
public abstract class BaseItemUI<TDefinition> : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject selectedOverlay;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private Button button;

    private TDefinition definition;
    private bool isUnlocked;

    public string ItemId { get; private set; } //ID của item này — dùng để so sánh selected state.

    public event Action<string> OnClicked; // Callback khi người chơi click item đã unlock. Tham số là ItemId.

    private void Awake()
    {
        if (button == null)
            Debug.LogWarning($"[{GetType().Name}] button is not assigned in Inspector.");
        else
            button.onClick.AddListener(HandleClick);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);

        OnClicked = null;
    }

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>
    /// Thiết lập item. Gọi ngay sau Instantiate.
    /// </summary>
    /// <param name="def">Definition của item.</param>
    /// <param name="unlocked">Item đã được mở khóa chưa.</param>
    public void Setup(TDefinition def, bool unlocked)
    {
        definition = def;
        isUnlocked = unlocked;
        ItemId     = GetId(def);

        Sprite sprite = GetSprite(def);
        iconImage.sprite  = sprite;
        iconImage.enabled = sprite != null;

        if (lockOverlay != null)
            lockOverlay.SetActive(!isUnlocked);

        // Button chỉ tương tác nếu đã unlock
        if (button != null)
            button.interactable = isUnlocked;
    }

    /// <summary>
    /// Cập nhật trạng thái selected (highlight hay không).
    /// Gọi từ EditProfilePopup mỗi khi selection thay đổi.
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (selectedOverlay != null)
            selectedOverlay.SetActive(selected);
    }

    // =========================================================================
    // Abstracts — subclass bắt buộc phải override để trả về id / sprite phù hợp
    // =========================================================================

    // Trả về id string của definition.
    protected abstract string GetId(TDefinition def);

    // Trả về sprite hiển thị của definition.
    protected abstract Sprite GetSprite(TDefinition def);

    private void HandleClick()
    {
        if (!isUnlocked) return;

        OnClicked?.Invoke(ItemId);
    }
}
