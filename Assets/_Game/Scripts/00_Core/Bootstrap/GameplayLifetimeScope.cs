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

        // ── Gameplay ──────────────────────────────────────────────────────────
        // GameplayController: inject GameManager, SaveManager, UIManager,
        //                             SceneManagerService, LevelManager
        builder.RegisterComponentInHierarchy<GameplayController>();

        // HoleController: inject bởi InputManager
        builder.RegisterComponentInHierarchy<HoleController>();

        // ── Level ─────────────────────────────────────────────────────────────
        builder.RegisterComponentInHierarchy<LevelManager>();

        // ── Objectives ────────────────────────────────────────────────────────
        builder.RegisterComponentInHierarchy<GameplayObjectiveManager>();

        // ── Input ─────────────────────────────────────────────────────────────
        // TouchJoystickInput đăng ký như IInputProvider + concrete type
        builder.RegisterComponentInHierarchy<TouchJoystickInput>();

        // ── InputManager ─────────────────────────────────────────────────────────────
        builder.RegisterComponentInHierarchy<InputManager>();
    }
}
