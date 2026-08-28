using UnityEngine;

public class MainmenuNavigator : MonoBehaviour
{
    public void GoToGameplay() => TransitionService.Instance?.TransitionToGameplayAsync().Forget();

    public static void GoToGameplayStatic() => TransitionService.Instance?.TransitionToGameplayAsync().Forget();
}
