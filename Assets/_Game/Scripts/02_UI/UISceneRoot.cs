using UnityEngine;
using VContainer;

/// <summary>
/// Gắn vào root Canvas của mỗi scene.
/// Đăng ký scene với UIManager khi load, hủy đăng ký khi unload.
///
/// Persistent windows (BottomPanel, TopBar, HUD...):
///   - Tự mở trong Awake/Start của chính chúng, hoặc bật sẵn trong hierarchy.
///   - UIManager sẽ đảm bảo chúng luôn active khi Initialize() được gọi.
///
/// Screen/Popup windows:
///   - Mặc định tắt trong hierarchy.
///   - Gọi uiManager.Open<T>() từ code để mở.
/// </summary>
public class UISceneRoot : MonoBehaviour
{
    private UIManager uiManager;

    [Inject]
    private void Construct(UIManager uiManager)
    {
        this.uiManager = uiManager;
    }

    private void Start()
    {
        if (uiManager == null)
        {
            Debug.LogWarning("[UISceneRoot] UIManager not injected. Is GameLifetimeScope loaded?");
            return;
        }

        uiManager.RegisterSceneUI(transform);
    }

    private void OnDestroy()
    {
        uiManager?.UnregisterSceneUI();
    }
}
