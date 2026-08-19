using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup xem ads để nhận thêm mạng — Mode = Popup trong Inspector.
/// Mở bằng UIManager.Open<LifePopup>() từ MainmenuPanel.
/// </summary>
public class LifePopup : PopupWindow
{
    [Header("Navigation")]
    [SerializeField] private Button closeButton;

    private void Start()
    {
        if (closeButton == null)
        {
            Debug.LogWarning("[LifePopup] closeButton is not assigned in Inspector.");
            return;
        }

        closeButton.onClick.AddListener(OnCloseClicked);
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseClicked);
    }

    private void OnCloseClicked()
    {
        UIManager?.Close<LifePopup>();
    }
}
