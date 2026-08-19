using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gắn lên Button hoặc Toggle bất kỳ trong một UIWindow
/// để tự động phát SFX khi tương tác.
/// </summary>
public class UIButtonSFX : MonoBehaviour
{
    private string sfxId = AudioID.SFX.UiClick;

    private Button button;
    private Toggle toggle;
    private UIWindow parentWindow;

    private void Awake()
    {
        button = GetComponent<Button>();
        toggle = GetComponent<Toggle>();

        // UIWindow la ancestor cua button nay trong hierarchy cua scene.
        // UIWindow.UIManager duoc set khi UIManager.Initialize() goi window.Initialize().
        parentWindow = GetComponentInParent<UIWindow>(true);

        if (button == null && toggle == null)
        {
            Debug.LogWarning(
                $"[UIButtonSFX] '{name}' không có Button hoặc Toggle.",
                this);
        }

        if (parentWindow == null)
        {
            Debug.LogWarning(
                $"[UIButtonSFX] No UIWindow found in parent hierarchy of '{name}'. " +
                "SFX will not play.",
                this);
        }
    }

    private void OnEnable()
    {
        if (button != null)
            button.onClick.AddListener(OnButtonClicked);

        if (toggle != null)
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(OnButtonClicked);

        if (toggle != null)
            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
    }

    private void OnButtonClicked()
    {
        PlaySFX();
    }

    private void OnToggleValueChanged(bool isOn)
    {
        PlaySFX();
    }

    private void PlaySFX()
    {
        parentWindow?.UIManager?.PlaySFX(sfxId);
    }
}