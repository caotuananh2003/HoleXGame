using VContainer;
using VContainer.Unity;

/// <summary>
/// DI container cho GameplayScene.
/// Là child scope của GameLifetimeScope — kế thừa tất cả global services
/// (GameManager, AudioManager, SaveManager, UIManager, SceneManagerService).
///
/// Quy tắc đăng ký:
///   - Chỉ đăng ký component nào được inject bởi component KHÁC.
///   - Component tự tìm nhau bằng GetComponent/GetComponentInParent
///     thì KHÔNG cần đăng ký vào container.
/// </summary>
public class GameplayLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // ── UI ────────────────────────────────────────────────────────────────
        builder.RegisterComponentInHierarchy<UISceneRoot>();
        // GameplayPanel: inject GameplayObjectiveManager, ItemManager
        builder.RegisterComponentInHierarchy<GameplayPanel>();

        // ── Popups có [Inject] ────────────────────────────────────────────────
        // SettingPopup: inject AudioManager (mở từ GameplayPanel)
        builder.RegisterComponentInHierarchy<SettingPopup>();
        // GameOverTimeUpPopup: inject GameTimer, HoleController, SceneManagerService, GameManager, SaveManager
        builder.RegisterComponentInHierarchy<GameOverTimeUpPopup>();
        builder.RegisterComponentInHierarchy<GameTimer>();
        // GameOverBombPopup: inject SceneManagerService, GameManager
        builder.RegisterComponentInHierarchy<GameOverBombPopup>();
        // GameWinPopup: inject SaveManager, SceneManagerService, GameManager
        builder.RegisterComponentInHierarchy<GameWinPopup>();
        // TryAgainPopup: inject SceneManagerService, GameManager, LevelManager
        builder.RegisterComponentInHierarchy<TryAgainPopup>();

        // ── Skin / Theme Appliers ─────────────────────────────────────────────
        // HoleSkinApplier: inject SaveManager
        builder.RegisterComponentInHierarchy<HoleSkinApplier>();
        // MapThemeApplier: inject SaveManager
        builder.RegisterComponentInHierarchy<MapThemeApplier>();

        // ── Gameplay ──────────────────────────────────────────────────────────
        // GameplayController: inject GameManager, SaveManager, UIManager,
        //                             SceneManagerService, LevelManager
        builder.RegisterComponentInHierarchy<GameplayController>();

        // HoleController: inject bởi InputManager
        builder.RegisterComponentInHierarchy<HoleController>();

        // ── Level ─────────────────────────────────────────────────────────────
        builder.RegisterComponentInHierarchy<LevelManager>();
        // LevelSpawner: inject bởi LevelManager
        builder.RegisterComponentInHierarchy<LevelSpawner>();

        // ── Objectives ────────────────────────────────────────────────────────
        builder.RegisterComponentInHierarchy<GameplayObjectiveManager>();

        // ── Input ─────────────────────────────────────────────────────────────
        // TouchJoystickInput đăng ký như IInputProvider + concrete type
        builder.RegisterComponentInHierarchy<TouchJoystickInput>();

        // ── InputManager ─────────────────────────────────────────────────────────────
        builder.RegisterComponentInHierarchy<InputManager>();

        // ── Items ─────────────────────────────────────────────────────────────
        // ItemManager: inject SaveManager (từ parent scope GameLifetimeScope)
        builder.RegisterComponentInHierarchy<ItemManager>();
    }
}
