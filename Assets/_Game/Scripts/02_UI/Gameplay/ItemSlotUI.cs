using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI slot cho một item trong gameplay.
/// Hierarchy (tên child phải đúng để auto-resolve):
///   Item (root, có Button component)
///   ├── Icon          (Image)
///   ├── CountText     (TMP_Text)
///   └── LockedOverlay (GameObject)
///
/// Các reference được tự động tìm trong Awake() theo tên child.
/// Có thể override bằng cách kéo tay vào SerializeField trong Inspector.
///
/// Fire OnClicked khi button pressed — GameplayPanel sẽ gọi ItemManager.UseItem().
/// </summary>
public class ItemSlotUI : MonoBehaviour
{
    [Header("UI References (tự động tìm nếu để trống)")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private Button button;

    // ── State ─────────────────────────────────────────────────────────────────
    private ItemDefinition itemDefinition;

    /// <summary>ItemId của item đang hiển thị. Dùng để GameplayPanel tìm đúng slot khi refresh.</summary>
    public string ItemId => itemDefinition != null ? itemDefinition.ItemId : string.Empty;

    // ── Events ────────────────────────────────────────────────────────────────
    public System.Action<ItemDefinition> OnClicked;

    // =========================================================================
    // Unity Lifecycle
    // =========================================================================

    private void Awake()
    {
        ResolveReferences();

        if (button != null)
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
    /// Setup slot với ItemDefinition và quantity hiện tại.
    /// Gọi từ GameplayPanel khi khởi tạo hoặc khi quantity thay đổi.
    /// </summary>
    public void Setup(ItemDefinition item, int quantity)
    {
        itemDefinition = item;

        if (item == null)
        {
            SetEmpty();
            return;
        }

        // Icon
        if (iconImage != null)
        {
            iconImage.sprite = item.Icon;
            iconImage.enabled = item.Icon != null;
        }

        // Quantity
        if (quantityText != null)
            quantityText.text = quantity.ToString();

        // Locked state
        bool isLocked = item.IsLocked || quantity <= 0;
        if (lockedOverlay != null)
            lockedOverlay.SetActive(isLocked);

        // Button interactable
        if (button != null)
            button.interactable = !isLocked;
    }

    /// <summary>
    /// Refresh quantity text và locked state (gọi sau khi UseItem thành công).
    /// </summary>
    public void UpdateQuantity(int quantity)
    {
        if (quantityText != null)
            quantityText.text = quantity.ToString();

        bool isEmpty = quantity <= 0;
        bool isLocked = isEmpty || (itemDefinition != null && itemDefinition.IsLocked);

        if (lockedOverlay != null)
            lockedOverlay.SetActive(isLocked);

        if (button != null)
            button.interactable = !isLocked;
    }

    // =========================================================================
    // Internal
    // =========================================================================

    /// <summary>
    /// Tự động resolve các reference theo tên child trong Hierarchy.
    /// Chỉ gán nếu SerializeField chưa được kéo tay trong Inspector.
    /// </summary>
    private void ResolveReferences()
    {
        // Button nằm trên chính root GameObject
        if (button == null)
            button = GetComponent<Button>();

        // Icon: child tên "Icon", lấy Image component
        if (iconImage == null)
        {
            Transform iconTransform = transform.Find("Icon");
            if (iconTransform != null)
                iconImage = iconTransform.GetComponent<Image>();
        }

        // CountText: child tên "CountText", lấy TMP_Text component
        if (quantityText == null)
        {
            Transform countTransform = transform.Find("CountText");
            if (countTransform != null)
                quantityText = countTransform.GetComponent<TMP_Text>();
        }

        // LockedOverlay: child tên "LockedOverlay", lấy GameObject
        if (lockedOverlay == null)
        {
            Transform overlayTransform = transform.Find("LockedOverlay");
            if (overlayTransform != null)
                lockedOverlay = overlayTransform.gameObject;
        }

        // Log cảnh báo nếu vẫn thiếu sau khi auto-resolve
        if (button == null)
            Debug.LogWarning("[ItemSlotUI] Button không tìm thấy trên root GameObject.", this);

        if (iconImage == null)
            Debug.LogWarning("[ItemSlotUI] Không tìm thấy child 'Icon' có Image component.", this);

        if (quantityText == null)
            Debug.LogWarning("[ItemSlotUI] Không tìm thấy child 'CountText' có TMP_Text component.", this);
    }

    private void SetEmpty()
    {
        if (iconImage != null)
            iconImage.enabled = false;

        if (quantityText != null)
            quantityText.text = "";

        if (lockedOverlay != null)
            lockedOverlay.SetActive(true);

        if (button != null)
            button.interactable = false;
    }

    private void HandleClick()
    {
        if (itemDefinition == null) return;

        OnClicked?.Invoke(itemDefinition);
    }
}
