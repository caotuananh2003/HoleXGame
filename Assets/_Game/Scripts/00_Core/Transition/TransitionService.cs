using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Điều phối toàn bộ việc chuyển đổi giữa các game state trong single-scene.
///
/// Mọi transition đều đi qua loading overlay:
///   Show overlay → toggle UI/group → hide overlay
///
/// Public API:
///   ShowOverlayImmediate()      — Boot: che màn hình ngay khi game khởi động
///   TransitionToMainMenuAsync() — Boot hoặc Gameplay → MainMenu
///   TransitionToGameplayAsync() — MainMenu → Gameplay
/// </summary>
public class TransitionService : MonoBehaviour
{
    public static TransitionService Instance { get; private set; }

    private void Awake()
    {
        Validate();
        Instance = this;
    }
    private void OnDestroy() { if (Instance == this) Instance = null; }

    [Header("UI Groups")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject gameplayCanvas;

    [Header("Gameplay World")]
    [SerializeField] private GameObject gameplayGroup;

    [Header("Loading Overlay")]
    [SerializeField] private LoadingOverlay loadingOverlay;
    [Tooltip("Thời gian tối thiểu overlay hiển thị (giây). Tránh flash quá nhanh.")]
    [SerializeField] private float minOverlayDuration = 0.8f;

    private bool  _isTransitioning;

    // =========================================================================
    // Public API
    // =========================================================================

    public async UniTaskVoid TransitionToGameplayAsync()
    {
        if (_isTransitioning) return;

        GameManager.Instance.ChangeState(GameState.Loading);
        await ShowOverlayAsync();

        mainMenuCanvas.SetActive(false);
        gameplayCanvas.SetActive(true);
        gameplayGroup.SetActive(true);
        GameplayController.Instance?.StartLevel();

        await HideOverlayAsync();
    }

    /// <summary>
    /// Chuyển sang MainMenu với loading overlay.
    /// Dùng được từ cả BootstrapLoader (overlay đã hiện sẵn)
    /// lẫn Gameplay → MainMenu (overlay chưa hiện).
    /// </summary>
    public async UniTask TransitionToMainMenuAsync()
    {
        if (!_isTransitioning)
        {
            // Gọi từ Gameplay — overlay chưa hiện, cần show trước
            GameManager.Instance.ChangeState(GameState.Loading);
            await ShowOverlayAsync();
        }

        GameplayController.Instance?.Cleanup();
        SetMainMenuActive();
        GameManager.Instance.ChangeState(GameState.MainMenu);

        await HideOverlayAsync();
    }

    /// <summary>
    /// Hiện overlay ngay lập tức không có animation — gọi ở đầu Boot
    /// để che màn hình trong khi các hệ thống đang khởi tạo.
    /// </summary>
    public void ShowOverlayImmediate()
    {
        _isTransitioning = true;
        loadingOverlay?.ShowImmediate();
    }

    // =========================================================================
    // Overlay helpers
    // =========================================================================

    private async UniTask ShowOverlayAsync()
    {
        _isTransitioning     = true;
        if (loadingOverlay != null) await loadingOverlay.ShowAsync();
    }

    private async UniTask HideOverlayAsync()
    {
        if (loadingOverlay != null)
        {
            await loadingOverlay.HideAsync();
        }

        _isTransitioning = false;
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private void SetMainMenuActive()
    {
        mainMenuCanvas.SetActive(true);
        gameplayCanvas.SetActive(false);
        gameplayGroup.SetActive(false);
    }

    private void Validate()
    {
        if (mainMenuCanvas == null) Debug.LogWarning("[TransitionService] mainMenuCanvas is not assigned.", this);
        if (gameplayCanvas == null) Debug.LogWarning("[TransitionService] gameplayCanvas is not assigned.", this);
        if (gameplayGroup  == null) Debug.LogWarning("[TransitionService] gameplayGroup is not assigned.",  this);
        if (loadingOverlay == null) Debug.LogWarning("[TransitionService] loadingOverlay is not assigned.", this);
    }
}
