using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gắn lên Button bất kỳ trong một UIWindow để tự động phát SFX khi click.
/// Lấy UIManager qua UIWindow gần nhất trong parent hierarchy — không cần inject,
/// không cần FindAnyObjectByType.
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonSFX : MonoBehaviour
{
    [Tooltip("De trong se dung 'sfx_ui_click' mac dinh")]
    [SerializeField] private string sfxId = "sfx_ui_click";

    private Button button;
    private UIWindow parentWindow;

    private void Awake()
    {
        button = GetComponent<Button>();

        // UIWindow la ancestor cua button nay trong hierarchy cua scene.
        // UIWindow.UIManager duoc set khi UIManager.Initialize() goi window.Initialize().
        parentWindow = GetComponentInParent<UIWindow>(true);

        if (parentWindow == null)
            Debug.LogWarning($"[UIButtonSFX] No UIWindow found in parent hierarchy of '{name}'. SFX will not play.");
    }

    private void OnEnable()
    {
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        parentWindow?.UIManager?.PlaySFX(sfxId);
    }
}
