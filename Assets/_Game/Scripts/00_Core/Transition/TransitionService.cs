using Cysharp.Threading.Tasks;
using UnityEngine;

public class TransitionService : MonoBehaviour
{
    public static TransitionService Instance { get; private set; }

    private void Awake()     { Instance = this; }
    private void OnDestroy() { if (Instance == this) Instance = null; }

    [Header("MainMenu UI")]
    [SerializeField] private GameObject[] mainMenuObjects;

    [Header("Gameplay UI")]
    [SerializeField] private GameObject[] gameplayUIObjects;

    [Header("Gameplay World")]
    [SerializeField] private GameObject gameplayGroup;

    private void Start()
    {
        Validate();
    }

    public void ShowMainMenuImmediate()
    {
        SetGroupActive(mainMenuObjects,   true);
        SetGroupActive(gameplayUIObjects, false);
        if (gameplayGroup != null) gameplayGroup.SetActive(false);
        GameManager.Instance.ChangeState(GameState.MainMenu);
    }

    public async UniTaskVoid TransitionToGameplayAsync()
    {
        if (GameManager.Instance.IsState(GameState.Loading)) return;
        GameManager.Instance.ChangeState(GameState.Loading);

        await SceneManagerService.Instance.ShowOverlayAsync();

        SetGroupActive(mainMenuObjects,   false);
        SetGroupActive(gameplayUIObjects, true);
        if (gameplayGroup != null) gameplayGroup.SetActive(true);

        GameplayController.Instance?.StartLevel();

        await SceneManagerService.Instance.HideOverlayAsync();
    }

    public async UniTaskVoid TransitionToMainMenuAsync()
    {
        if (GameManager.Instance.IsState(GameState.Loading)) return;
        GameManager.Instance.ChangeState(GameState.Loading);

        await SceneManagerService.Instance.ShowOverlayAsync();

        GameplayController.Instance?.Cleanup();

        SetGroupActive(gameplayUIObjects, false);
        if (gameplayGroup != null) gameplayGroup.SetActive(false);
        SetGroupActive(mainMenuObjects,   true);

        GameManager.Instance.ChangeState(GameState.MainMenu);

        await SceneManagerService.Instance.HideOverlayAsync();
    }

    private static void SetGroupActive(GameObject[] group, bool active)
    {
        if (group == null) return;
        foreach (GameObject go in group)
            if (go != null) go.SetActive(active);
    }

    private void Validate()
    {
        if (mainMenuObjects == null || mainMenuObjects.Length == 0) Debug.LogWarning("[TransitionService] mainMenuObjects is empty.", this);
        if (gameplayUIObjects == null || gameplayUIObjects.Length == 0) Debug.LogWarning("[TransitionService] gameplayUIObjects is empty.", this);
        if (gameplayGroup == null) Debug.LogWarning("[TransitionService] gameplayGroup is not assigned.", this);
    }
}
