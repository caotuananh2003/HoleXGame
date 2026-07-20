using VContainer;
using VContainer.Unity;

/// <summary>
/// DI container cho GameplayScene.
/// Là child scope của GameLifetimeScope — kế thừa tất cả global services
/// (GameManager, AudioManager, SaveManager, UIManager, SceneManagerService).
///
/// Đăng ký ở đây: tất cả MonoBehaviour chỉ tồn tại trong GameplayScene.
/// </summary>
public class GameplayLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // ── UI ────────────────────────────────────────────────────────────────
        // UISceneRoot đăng ký scene UI canvas với UIManager khi Start()
        builder.RegisterComponentInHierarchy<UISceneRoot>();

        // ── Gameplay core ─────────────────────────────────────────────────────
        builder.RegisterComponentInHierarchy<GameplayController>();

        // HoleController đăng ký để InputManager inject được
        builder.RegisterComponentInHierarchy<HoleController>();

        // HoleMovement đăng ký để HoleController inject được
        builder.RegisterComponentInHierarchy<HoleMovement>();

        // HoleSizeController đăng ký để HoleController inject được
        builder.RegisterComponentInHierarchy<HoleSizeController>();

        // HoleDetector đăng ký để HoleSizeController inject được
        builder.RegisterComponentInHierarchy<HoleDetector>();

        // SwallowHandler đăng ký để HoleSizeController inject được
        builder.RegisterComponentInHierarchy<SwallowHandler>();

        // ── Level ─────────────────────────────────────────────────────────────
        // LevelManager thuộc về scene này, không phải global
        builder.RegisterComponentInHierarchy<LevelManager>();

        // ── Object Pool ───────────────────────────────────────────────────────
        builder.RegisterComponentInHierarchy<ObjectPoolService>();

        // ── Input ─────────────────────────────────────────────────────────────
        // Đăng ký TouchJoystickInput theo cả concrete type lẫn interface IInputProvider
        // AsImplementedInterfaces() + AsSelf() để resolve được cả hai cách
        builder.RegisterComponentInHierarchy<TouchJoystickInput>()
               .AsImplementedInterfaces()
               .AsSelf();

        builder.RegisterComponentInHierarchy<InputManager>();
    }
}
