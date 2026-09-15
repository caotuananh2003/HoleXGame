using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base class cho Avatar / Frame / Badge / Shop item trong ScrollView.
///
/// Hierarchy của prefab:
///   BaseItemUI
///   ├── IconImage        — sprite của item
///   ├── Description
///   │   ├── UnLocked     — hiện khi item đã unlock
///   │   └── Locked       — hiện khi item chưa unlock
///   ├── SelectedOverlay  — highlight khi đang được chọn (ẩn mặc định)
///   └── Button           — bắt sự kiện click (luôn interactable)
///
/// Click luôn được fire dù locked hay unlocked.
/// ShopPanel / EditProfilePopup tự quyết định hành vi theo ItemId.
/// </summary>
public abstract class BaseItemUI<TDefinition> : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image      iconImage;
    [SerializeField] private GameObject selectedOverlay;
    [SerializeField] private GameObject unlockedObject;  // child "UnLocked" trong Description
    [SerializeField] private GameObject lockedObject;    // child "Locked"   trong Description
    [SerializeField] private Button     button;

    private TDefinition definition;
    private bool        isUnlocked;

    /// <summary>ID của item — dùng để so sánh selected state và định danh trong callback.</summary>
    public string ItemId { get; private set; }

    /// <summary>
    /// Callback khi người chơi click item.
    /// Tham số: ItemId. Fire cả khi locked — caller tự xử lý.
    /// </summary>
    public event Action<string> OnClicked;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

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
    public void Setup(TDefinition def, bool unlocked)
    {
        definition = def;
        isUnlocked = unlocked;
        ItemId     = GetId(def);

        Sprite sprite     = GetSprite(def);
        iconImage.sprite  = sprite;
        iconImage.enabled = sprite != null;

        RefreshLockVisual();

        // Button luôn interactable — click locked item sẽ show buy options
        if (button != null)
            button.interactable = true;
    }

    /// <summary>Cập nhật trạng thái unlock và refresh visual.</summary>
    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
        RefreshLockVisual();
    }

    /// <summary>Cập nhật trạng thái selected (highlight).</summary>
    public void SetSelected(bool selected)
    {
        if (selectedOverlay != null)
            selectedOverlay.SetActive(selected);
    }

    // =========================================================================
    // Abstracts
    // =========================================================================

    protected abstract string GetId(TDefinition def);
    protected abstract Sprite GetSprite(TDefinition def);

    // =========================================================================
    // Internal
    // =========================================================================

    private void RefreshLockVisual()
    {
        if (unlockedObject != null) unlockedObject.SetActive(isUnlocked);
        if (lockedObject   != null) lockedObject.SetActive(!isUnlocked);
    }

    private void HandleClick()
    {
        // Fire cho cả locked item — caller (ShopPanel) tự xử lý
        OnClicked?.Invoke(ItemId);
    }
}
