using VContainer;
using VContainer.Unity;

/// <summary>
/// DI container cho MainMenuScene.
/// Child scope của GameLifetimeScope — kế thừa global services:
/// GameManager, SaveManager, AudioManager, UIManager, SceneManagerService.
///
/// Quy tắc đăng ký:
/// - Chỉ đăng ký component nào nhận [Inject] từ component KHÁC.
/// - Component không cần inject thì KHÔNG đăng ký.
/// </summary>
public class MainMenuLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // ── Scene infrastructure ──────────────────────────────────────────────
        builder.RegisterComponentInHierarchy<UISceneRoot>();

        // ── Navigation ────────────────────────────────────────────────────────
        builder.RegisterComponentInHierarchy<MainmenuNavigator>();

        // ── Panels ────────────────────────────────────────────────────────────
        builder.RegisterComponentInHierarchy<MainmenuPanel>();

        // ── Popups — chỉ những popup có [Inject] ─────────────────────────────
        // SettingPopup   : inject AudioManager
        // ProfilePopup   : inject SaveManager
        // EditProfilePopup: inject SaveManager
        builder.RegisterComponentInHierarchy<SettingPopup>();
        builder.RegisterComponentInHierarchy<ProfilePopup>();
        builder.RegisterComponentInHierarchy<EditProfilePopup>();

        // ── Screens có [Inject] ───────────────────────────────────────────────
        // ShopPanel: inject SaveManager (đọc equippedHoleSkinId / equippedMapThemeId)
        builder.RegisterComponentInHierarchy<ShopPanel>();
    }
}
