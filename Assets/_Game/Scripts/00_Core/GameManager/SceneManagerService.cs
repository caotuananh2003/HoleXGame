using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class SceneManagerService : MonoBehaviour
{
    private const float MinLoadingDuration = 2f;

    [SerializeField] private LoadingOverlay loadingOverlayPrefab;

    private LoadingOverlay loadingOverlay;
    private bool isLoading;

    public bool IsLoading => isLoading;

    // Signal báo scene mới đã init xong (gọi từ UISceneRoot)
    private UniTaskCompletionSource sceneReadySource;

    private void Awake()
    {
        if (loadingOverlayPrefab != null)
        {
            loadingOverlay = Instantiate(loadingOverlayPrefab);
            DontDestroyOnLoad(loadingOverlay.gameObject);
        }
        else
        {
            Debug.LogWarning("[SceneManagerService] loadingOverlayPrefab is not assigned in Inspector.");
        }
    }

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>
    /// Load scene mới (single mode).
    /// Hiện LoadingOverlay → load scene → chờ scene báo ready → chờ tối thiểu 1s → ẩn overlay.
    /// </summary>
    public async UniTask LoadScene(string scene)
    {
        if (string.IsNullOrWhiteSpace(scene))
        {
            Debug.LogError("[SceneManagerService] Scene name is empty.");
            return;
        }

        if (isLoading) return;

        isLoading = true;
        sceneReadySource = new UniTaskCompletionSource();

        // Hiện overlay, chờ animation xong mới bắt đầu load
        if (loadingOverlay != null)
            await loadingOverlay.ShowAsync();

        float startTime = Time.realtimeSinceStartup;

        try
        {
            await UnitySceneManager.LoadSceneAsync(scene);

            // Chờ UISceneRoot của scene mới gọi NotifySceneReady()
            await sceneReadySource.Task;

            // Đảm bảo hiển thị tối thiểu 1s kể từ khi bắt đầu load
            float elapsed = Time.realtimeSinceStartup - startTime;
            if (elapsed < MinLoadingDuration)
                await UniTask.Delay(TimeSpan.FromSeconds(MinLoadingDuration - elapsed), ignoreTimeScale: true);
        }
        finally
        {
            isLoading = false;

            if (loadingOverlay != null)
                await loadingOverlay.HideAsync();
        }
    }

    /// <summary>
    /// Gọi từ UISceneRoot.Start() khi scene mới đã đăng ký xong với UIManager.
    /// Báo hiệu cho LoadScene() rằng scene đã sẵn sàng để ẩn overlay.
    /// </summary>
    public void NotifySceneReady()
    {
        sceneReadySource?.TrySetResult();
    }

    // =========================================================================
    // Additive / Unload (không cần overlay)
    // =========================================================================

    public async UniTask LoadAdditive(string scene)
    {
        if (string.IsNullOrWhiteSpace(scene))
        {
            Debug.LogError("[SceneManagerService] Scene name is empty.");
            return;
        }

        await UnitySceneManager.LoadSceneAsync(scene, UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }

    public async UniTask Unload(string scene)
    {
        if (string.IsNullOrWhiteSpace(scene))
        {
            Debug.LogError("[SceneManagerService] Scene name is empty.");
            return;
        }

        await UnitySceneManager.UnloadSceneAsync(scene);
    }
}
