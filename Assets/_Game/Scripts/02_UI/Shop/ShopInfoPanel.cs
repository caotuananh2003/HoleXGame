using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hiển thị thông tin item đang được chọn trong HoleSkinScrollView / MapThemeScrollView.
/// Gắn vào GameObject "Info" con của mỗi ScrollView.
///
/// Hierarchy của Info:
///   Info                         ← gắn ShopInfoPanel
///   ├── Icon                     (Image — sprite item đang chọn)
///   ├── Name                     (TMP_Text — tên item)
///   └── ButtonObject
///       ├── BuyButton            (Button — hiện khi chưa sở hữu)
///       ├── EquipButton          (Button — hiện khi đã sở hữu nhưng chưa trang bị)
///       └── EquippedText         (GameObject — hiện khi đang trang bị, không phải Button)
///
/// Logic hiển thị theo ItemActionState:
///   NotOwned  → BuyButton enable,   EquipButton disable, EquippedText ẩn
///   Owned     → BuyButton disable,  EquipButton enable,  EquippedText ẩn
///   Equipped  → BuyButton disable,  EquipButton disable, EquippedText hiện
/// </summary>
public class ShopInfoPanel : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [Header("Display")]
    [SerializeField] private Image    iconImage;
    [SerializeField] private TMP_Text nameText;

    [Header("Button Object Children")]
    [SerializeField] private Button     buyButton;
    [SerializeField] private Button     equipButton;
    [SerializeField] private GameObject equippedText; // Text "Đang trang bị" — không phải Button

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private string currentItemId;

    // -------------------------------------------------------------------------
    // Events — ShopPanel lắng nghe
    // -------------------------------------------------------------------------

    /// <summary>Người chơi nhấn Buy (xem ads để mua). Tham số: itemId.</summary>
    public System.Action<string> OnBuyClicked;

    /// <summary>Người chơi nhấn Equip (trang bị item đã sở hữu). Tham số: itemId.</summary>
    public System.Action<string> OnEquipClicked;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        if (buyButton   != null) buyButton.onClick.AddListener(HandleBuyClick);
        if (equipButton != null) equipButton.onClick.AddListener(HandleEquipClick);
    }

    private void OnDestroy()
    {
        if (buyButton   != null) buyButton.onClick.RemoveListener(HandleBuyClick);
        if (equipButton != null) equipButton.onClick.RemoveListener(HandleEquipClick);

        OnBuyClicked   = null;
        OnEquipClicked = null;
    }

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>
    /// Cập nhật Info theo item vừa được chọn trên scrollview.
    /// Gọi từ ShopPanel mỗi khi OnHoleSkinItemClicked / OnMapThemeItemClicked.
    /// </summary>
    public void Show(string itemId, Sprite icon, string displayName, ItemActionState state)
    {
        currentItemId = itemId;

        if (iconImage != null)
        {
            iconImage.sprite  = icon;
            iconImage.enabled = icon != null;
        }

        if (nameText != null)
            nameText.text = displayName;

        ApplyState(state);
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private void ApplyState(ItemActionState state)
    {
        // Tắt hết trước, rồi bật đúng cái
        SetBuyButtonActive(false);
        SetEquipButtonActive(false);
        SetEquippedTextActive(false);

        switch (state)
        {
            case ItemActionState.NotOwned:
                SetBuyButtonActive(true);
                break;

            case ItemActionState.Owned:
                SetEquipButtonActive(true);
                break;

            case ItemActionState.Equipped:
                SetEquippedTextActive(true);
                break;
        }
    }

    private void SetBuyButtonActive(bool active)
    {
        if (buyButton != null)
            buyButton.gameObject.SetActive(active);
    }

    private void SetEquipButtonActive(bool active)
    {
        if (equipButton != null)
            equipButton.gameObject.SetActive(active);
    }

    private void SetEquippedTextActive(bool active)
    {
        if (equippedText != null)
            equippedText.SetActive(active);
    }

    private void HandleBuyClick()
    {
        OnBuyClicked?.Invoke(currentItemId);
    }

    private void HandleEquipClick()
    {
        OnEquipClicked?.Invoke(currentItemId);
    }

    // =========================================================================
    // Validation
    // =========================================================================

    private void OnValidate()
    {
        if (iconImage     == null) Debug.LogWarning("[ShopInfoPanel] iconImage is not assigned.",     this);
        if (nameText      == null) Debug.LogWarning("[ShopInfoPanel] nameText is not assigned.",      this);
        if (buyButton     == null) Debug.LogWarning("[ShopInfoPanel] buyButton is not assigned.",     this);
        if (equipButton   == null) Debug.LogWarning("[ShopInfoPanel] equipButton is not assigned.",   this);
        if (equippedText  == null) Debug.LogWarning("[ShopInfoPanel] equippedText is not assigned.",  this);
    }
}
