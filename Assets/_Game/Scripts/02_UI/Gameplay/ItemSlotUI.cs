using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI slot cho một item trong gameplay.
/// Hierarchy (tên child phải đúng để auto-resolve):
///   Item (root, có Button component)
///   ├── Icon          (Image)
///   ├── CountText     (TMP_Text)
///   ├── LockedOverlay (GameObject)
///   └── Timer         (Image, type = Filled, method = Vertical)
///
/// Các reference được tự động tìm trong Awake() theo tên child.
/// Có thể override bằng cách kéo tay vào SerializeField trong Inspector.
///
/// Fire OnClicked khi button pressed — GameplayPanel sẽ gọi ItemManager.UseItem().
/// 
/// Timer Image sẽ hiển thị thời gian còn lại của effect:
/// - fillAmount = 1 (100%) khi vừa dùng
/// - fillAmount giảm dần về 0 theo thời gian
/// - Tự động ẩn khi effect hết hoặc không có effect
/// </summary>
public class ItemSlotUI : MonoBehaviour
{
    [Header("UI References (tự động tìm nếu để trống)")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private Button button;
    [SerializeField] private Image timerImage;

    // ── State ─────────────────────────────────────────────────────────────────
    private ItemDefinition itemDefinition;
    private ITimedEffect activeEffect;
    private ItemManager itemManager;

    // Cache unlock state để RefreshUnlockState() không cần gọi lại ItemManager
    private bool isUnlocked;

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

        // Ẩn timer ban đầu
        if (timerImage != null)
            timerImage.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);

        UnsubscribeFromItemManager();

        OnClicked = null;
        activeEffect = null;
    }

    private void Update()
    {
        UpdateTimerFillAmount();
    }

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>
    /// Setup slot với ItemDefinition và quantity hiện tại.
    /// Gọi từ GameplayPanel khi khởi tạo hoặc khi quantity thay đổi.
    /// isUnlocked phải được truyền vào từ ItemManager.IsItemUnlocked().
    /// </summary>
    public void Setup(ItemDefinition item, int quantity, bool unlocked)
    {
        itemDefinition = item;
        isUnlocked     = unlocked;

        if (item == null)
        {
            SetEmpty();
            return;
        }

        // Icon
        if (iconImage != null)
        {
            iconImage.sprite  = item.Icon;
            iconImage.enabled = item.Icon != null;
        }

        // Quantity text — luôn hiển thị, kể cả khi = 0
        if (quantityText != null)
            quantityText.text = quantity.ToString();

        // LockedOverlay: chỉ hiển thị khi item CHƯA UNLOCK
        // Khi đã unlock nhưng hết (quantity = 0) → chỉ hiện text "0", không show overlay
        if (lockedOverlay != null)
            lockedOverlay.SetActive(!isUnlocked);

        // Button: chỉ interactable khi đã unlock VÀ còn hàng
        if (button != null)
            button.interactable = isUnlocked && quantity > 0;
    }

    /// <summary>
    /// Refresh quantity text và button state sau khi UseItem thành công.
    /// Không thay đổi unlock state — dùng RefreshUnlockState() riêng nếu cần.
    /// </summary>
    public void UpdateQuantity(int quantity)
    {
        // Quantity text — luôn hiển thị, kể cả khi = 0
        if (quantityText != null)
            quantityText.text = quantity.ToString();

        // LockedOverlay: chỉ phụ thuộc vào unlock state, không phụ thuộc quantity
        // (đã được set đúng trong Setup hoặc RefreshUnlockState)

        // Button: interactable khi đã unlock VÀ còn hàng
        if (button != null)
            button.interactable = isUnlocked && quantity > 0;
    }

    /// <summary>
    /// Cập nhật unlock state của slot khi item vừa được unlock.
    /// Gọi từ GameplayPanel khi nhận event ItemManager.OnItemUnlocked.
    /// </summary>
    public void RefreshUnlockState(bool unlocked)
    {
        isUnlocked = unlocked;

        if (lockedOverlay != null)
            lockedOverlay.SetActive(!isUnlocked);

        // Re-evaluate button dựa trên quantity hiện tại
        // Parse từ text để không cần truyền lại quantity
        int currentQuantity = 0;
        if (quantityText != null)
            int.TryParse(quantityText.text, out currentQuantity);

        if (button != null)
            button.interactable = isUnlocked && currentQuantity > 0;
    }

    /// <summary>
    /// Inject ItemManager dependency để subscribe vào event OnItemEffectStarted.
    /// Gọi từ GameplayPanel sau khi Setup().
    /// </summary>
    public void Initialize(ItemManager manager)
    {
        if (manager == null)
        {
            Debug.LogWarning("[ItemSlotUI] ItemManager null — không thể subscribe event.", this);
            return;
        }

        UnsubscribeFromItemManager(); // Đảm bảo không subscribe trùng
        itemManager = manager;
        itemManager.OnItemEffectStarted += HandleEffectStarted;
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

        // Timer: child tên "Timer", lấy Image component
        if (timerImage == null)
        {
            Transform timerTransform = transform.Find("Timer");
            if (timerTransform != null)
                timerImage = timerTransform.GetComponent<Image>();
        }

        // Log cảnh báo nếu vẫn thiếu sau khi auto-resolve
        if (button == null)
            Debug.LogWarning("[ItemSlotUI] Button không tìm thấy trên root GameObject.", this);

        if (iconImage == null)
            Debug.LogWarning("[ItemSlotUI] Không tìm thấy child 'Icon' có Image component.", this);

        if (quantityText == null)
            Debug.LogWarning("[ItemSlotUI] Không tìm thấy child 'CountText' có TMP_Text component.", this);

        if (timerImage == null)
            Debug.LogWarning("[ItemSlotUI] Không tìm thấy child 'Timer' có Image component.", this);
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

        if (timerImage != null)
            timerImage.gameObject.SetActive(false);
    }

    private void HandleClick()
    {
        if (itemDefinition == null) return;

        OnClicked?.Invoke(itemDefinition);
    }

    private void UnsubscribeFromItemManager()
    {
        if (itemManager != null)
        {
            itemManager.OnItemEffectStarted -= HandleEffectStarted;
            itemManager = null;
        }
    }

    /// <summary>
    /// Callback khi ItemManager fire OnItemEffectStarted.
    /// Chỉ xử lý nếu itemId trùng với slot này.
    /// </summary>
    private void HandleEffectStarted(string itemId, ITimedEffect effect)
    {
        // Chỉ xử lý nếu effect này thuộc item của slot này
        if (itemId != ItemId)
            return;

        if (effect == null)
        {
            // Effect instant — ẩn timer
            if (timerImage != null)
                timerImage.gameObject.SetActive(false);
            activeEffect = null;
            return;
        }

        // Effect có duration — hiển thị timer
        activeEffect = effect;
        if (timerImage != null)
        {
            timerImage.gameObject.SetActive(true);
            timerImage.fillAmount = 1f;
        }

        // Subscribe vào OnExpired để ẩn timer khi effect hết
        effect.OnExpired += HandleEffectExpired;
    }

    private void HandleEffectExpired()
    {
        if (activeEffect != null)
        {
            activeEffect.OnExpired -= HandleEffectExpired;
            activeEffect = null;
        }

        if (timerImage != null)
            timerImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// Update fillAmount của timer image mỗi frame dựa trên thời gian còn lại.
    /// fillAmount = Remaining / TotalDuration
    /// </summary>
    private void UpdateTimerFillAmount()
    {
        if (activeEffect == null || timerImage == null)
            return;

        float total = activeEffect.TotalDuration;
        if (total <= 0f)
        {
            timerImage.fillAmount = 0f;
            return;
        }

        float remaining = activeEffect.Remaining;
        timerImage.fillAmount = Mathf.Clamp01(remaining / total);
    }
}
