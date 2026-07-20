using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// Root DI container — tồn tại xuyên scene (DontDestroyOnLoad).
/// Chỉ đăng ký global services: không có gì thuộc về một scene cụ thể.
/// Scene-specific components đăng ký trong LifetimeScope riêng của scene đó.
/// </summary>
public class GameLifetimeScope : LifetimeScope
{
    protected override void Awake()
    {
        // DontDestroyOnLoad truoc khi base.Awake() build container
        // de dam bao scope ton tai xuyen scene truoc khi child scope tim thay no.
        DontDestroyOnLoad(transform.root.gameObject);
        base.Awake();
    }

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(GetRequiredComponentInChildren<GameManager>());

        builder.RegisterComponent(GetRequiredComponentInChildren<SaveManager>());

        builder.RegisterComponent(GetRequiredComponentInChildren<AudioManager>());

        builder.RegisterComponent(GetRequiredComponentInChildren<UIManager>());

        builder.RegisterComponent(GetRequiredComponentInChildren<SceneManagerService>());

        builder.RegisterComponent(GetRequiredComponentInChildren<BootstrapLoader>());
    }

    private T GetRequiredComponentInChildren<T>() where T : Component
    {
        T component = GetComponentInParent<Transform>().root.GetComponentInChildren<T>(true);

        if (component == null)
            throw new MissingComponentException($"{typeof(T).Name} is required under {name}.");

        return component;
    }
}
