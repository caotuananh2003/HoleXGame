using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup mua Remove Ads.
/// Mode = Popup trong Inspector.
///
/// Hiện tại: mở / đóng popup.
/// Có thể mở rộng sau: gọi Purchase(), RestorePurchase() thông qua IAP service.
/// </summary>
public class RemoveAdsPopup : PopupWindow
{
    // -------------------------------------------------------------------------
    // Navigation
    // -------------------------------------------------------------------------

    [Header("Navigation")]
    [SerializeField] private Button closeButton;

    // -------------------------------------------------------------------------
    // IAP Buttons — mở rộng sau
    // -------------------------------------------------------------------------

    // [Header("IAP")]
    // [SerializeField] private Button purchaseButton;
    // [SerializeField] private Button restoreButton;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    public override void Open()
    {
        base.Open();
        UIManager?.PlaySFX(AudioID.SFX.UiPopup);
    }

    private void Start()
    {
        if (closeButton == null)
        {
            Debug.LogWarning("[RemoveAdsPopup] closeButton is not assigned in Inspector.");
            return;
        }

        closeButton.onClick.AddListener(OnCloseClicked);

        // Mở rộng sau:
        // if (purchaseButton != null) purchaseButton.onClick.AddListener(OnPurchaseClicked);
        // if (restoreButton  != null) restoreButton.onClick.AddListener(OnRestoreClicked);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseClicked);

        // Mở rộng sau:
        // if (purchaseButton != null) purchaseButton.onClick.RemoveListener(OnPurchaseClicked);
        // if (restoreButton  != null) restoreButton.onClick.RemoveListener(OnRestoreClicked);
    }

    // =========================================================================
    // Handlers
    // =========================================================================

    private void OnCloseClicked()
    {
        UIManager?.Close<RemoveAdsPopup>();
    }

    // Mở rộng sau — inject IAPService vào đây khi có:
    // private void OnPurchaseClicked()      { /* iapService.Purchase(productId); */ }
    // private void OnRestoreClicked()       { /* iapService.RestorePurchase();   */ }
}
