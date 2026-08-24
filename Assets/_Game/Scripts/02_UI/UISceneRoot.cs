using UnityEngine;
using VContainer;

/// <summary>
/// Gắn vào root Canvas của mỗi scene.
/// Đăng ký scene với UIManager khi load, hủy đăng ký khi unload.
/// Báo hiệu SceneManagerService khi scene đã sẵn sàng (LoadingOverlay có thể ẩn).
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
    private SceneManagerService sceneManagerService;

    [Inject]
    private void Construct(UIManager uiManager, SceneManagerService sceneManagerService)
    {
        this.uiManager            = uiManager;
        this.sceneManagerService  = sceneManagerService;
    }

    private void Start()
    {
        if (uiManager == null)
        {
            Debug.LogWarning("[UISceneRoot] UIManager not injected. Is GameLifetimeScope loaded?");
            return;
        }

        uiManager.RegisterSceneUI(transform);

        // Scene đã init xong — LoadingOverlay có thể bắt đầu ẩn
        sceneManagerService?.NotifySceneReady();
    }

    private void OnDestroy()
    {
        uiManager?.UnregisterSceneUI();
    }
}
