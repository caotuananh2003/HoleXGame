using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hiển thị thông tin item đang được chọn trong HoleSkinScrollView / MapThemeScrollView.
/// Gắn vào GameObject "Info" con của mỗi ScrollView.
///
/// Hierarchy của ButtonObject:
///   ButtonObject
///   ├── BuyByCurrencyButton  — hiện khi NotOwned (trừ currency)
///   ├── BuyByAdsButton       — hiện khi NotOwned (xem ads)
///   ├── EquipButton          — hiện khi Owned nhưng chưa trang bị
///   └── EquippedText         — hiện khi đang trang bị
///
/// Logic:
///   NotOwned  → BuyByCurrencyButton + BuyByAdsButton hiện
///   Owned     → EquipButton hiện
///   Equipped  → EquippedText hiện
/// </summary>
public class ShopInfoPanel : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private Image    iconImage;
    [SerializeField] private TMP_Text nameText;

    [Header("Buttons")]
    [SerializeField] private Button     buyByCurrencyButton;
    [SerializeField] private TMP_Text   buyByCurrencyPriceText; // Text giá trên BuyByCurrencyButton
    [SerializeField] private Button     buyByAdsButton;
    [SerializeField] private Button     equipButton;
    [SerializeField] private GameObject equippedText;

    // ── State ─────────────────────────────────────────────────────────────────
    private string currentItemId;

    // ── Events ────────────────────────────────────────────────────────────────
    public System.Action<string> OnBuyByCurrencyClicked;
    public System.Action<string> OnBuyByAdsClicked;
    public System.Action<string> OnEquipClicked;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        if (buyByCurrencyButton != null) buyByCurrencyButton.onClick.AddListener(HandleBuyByCurrencyClick);
        if (buyByAdsButton      != null) buyByAdsButton.onClick.AddListener(HandleBuyByAdsClick);
        if (equipButton         != null) equipButton.onClick.AddListener(HandleEquipClick);
    }

    private void OnDestroy()
    {
        if (buyByCurrencyButton != null) buyByCurrencyButton.onClick.RemoveListener(HandleBuyByCurrencyClick);
        if (buyByAdsButton      != null) buyByAdsButton.onClick.RemoveListener(HandleBuyByAdsClick);
        if (equipButton         != null) equipButton.onClick.RemoveListener(HandleEquipClick);

        OnBuyByCurrencyClicked = null;
        OnBuyByAdsClicked      = null;
        OnEquipClicked         = null;
    }

    // =========================================================================
    // Public API
    // =========================================================================

    public void Show(string itemId, Sprite icon, string displayName, ItemActionState state, int price = 0)
    {
        currentItemId = itemId;

        if (iconImage != null)
        {
            iconImage.sprite  = icon;
            iconImage.enabled = icon != null;
        }

        if (nameText != null)
            nameText.text = displayName;

        // Cập nhật giá trên BuyByCurrencyButton
        if (buyByCurrencyPriceText != null)
            buyByCurrencyPriceText.text = price.ToString();

        ApplyState(state);
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private void ApplyState(ItemActionState state)
    {
        // Tắt tất cả trước
        if (buyByCurrencyButton != null) buyByCurrencyButton.gameObject.SetActive(false);
        if (buyByAdsButton      != null) buyByAdsButton.gameObject.SetActive(false);
        if (equipButton         != null) equipButton.gameObject.SetActive(false);
        if (equippedText        != null) equippedText.SetActive(false);

        switch (state)
        {
            case ItemActionState.NotOwned:
                if (buyByCurrencyButton != null) buyByCurrencyButton.gameObject.SetActive(true);
                if (buyByAdsButton      != null) buyByAdsButton.gameObject.SetActive(true);
                break;

            case ItemActionState.Owned:
                if (equipButton != null) equipButton.gameObject.SetActive(true);
                break;

            case ItemActionState.Equipped:
                if (equippedText != null) equippedText.SetActive(true);
                break;
        }
    }

    private void HandleBuyByCurrencyClick() => OnBuyByCurrencyClicked?.Invoke(currentItemId);
    private void HandleBuyByAdsClick()      => OnBuyByAdsClicked?.Invoke(currentItemId);
    private void HandleEquipClick()         => OnEquipClicked?.Invoke(currentItemId);

    private void OnValidate()
    {
        if (iconImage           == null) Debug.LogWarning("[ShopInfoPanel] iconImage is not assigned.",                this);
        if (nameText            == null) Debug.LogWarning("[ShopInfoPanel] nameText is not assigned.",                 this);
        if (buyByCurrencyButton == null) Debug.LogWarning("[ShopInfoPanel] buyByCurrencyButton is not assigned.",      this);
        if (buyByAdsButton      == null) Debug.LogWarning("[ShopInfoPanel] buyByAdsButton is not assigned.",           this);
        if (equipButton         == null) Debug.LogWarning("[ShopInfoPanel] equipButton is not assigned.",              this);
        if (equippedText        == null) Debug.LogWarning("[ShopInfoPanel] equippedText is not assigned.",             this);
    }
}
