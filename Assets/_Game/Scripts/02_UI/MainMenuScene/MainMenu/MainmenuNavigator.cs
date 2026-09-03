using UnityEngine;

public class MainmenuNavigator : MonoBehaviour
{    
    public static MainmenuNavigator Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void GoToGameplay() => TransitionService.Instance?.TransitionToGameplayAsync().Forget();
}
