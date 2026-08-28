using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SceneManagerService : MonoBehaviour
{
    public static SceneManagerService Instance { get; private set; }

    private const float MinLoadingDuration = 1f;

    [SerializeField] private LoadingOverlay loadingOverlay;

    private bool  isTransitioning;
    private float _loadStartTime;

    public bool IsTransitioning => isTransitioning;

    private void Awake()
    {
        Instance = this;
        if (loadingOverlay == null)
            Debug.LogWarning("[SceneManagerService] loadingOverlay is not assigned.");
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public async UniTask ShowOverlayAsync()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        _loadStartTime  = Time.realtimeSinceStartup;
        if (loadingOverlay != null) await loadingOverlay.ShowAsync();
    }

    public async UniTask HideOverlayAsync()
    {
        float elapsed = Time.realtimeSinceStartup - _loadStartTime;
        if (elapsed < MinLoadingDuration)
            await UniTask.Delay(TimeSpan.FromSeconds(MinLoadingDuration - elapsed), ignoreTimeScale: true);
        if (loadingOverlay != null) await loadingOverlay.HideAsync();
        isTransitioning = false;
    }
}
