using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MainMenuLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<UISceneRoot>();
        builder.RegisterComponentInHierarchy<MainmenuNavigator>();
        builder.RegisterComponentInHierarchy<MainmenuPanel>();
    }
}
