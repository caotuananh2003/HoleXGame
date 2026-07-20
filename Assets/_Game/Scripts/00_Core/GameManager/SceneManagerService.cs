using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class SceneManagerService : MonoBehaviour
{
    private bool isLoading;
    private UIManager uiManager;

    public bool IsLoading => isLoading;

    [Inject]
    private void Construct(UIManager uiManager)
    {
        this.uiManager = uiManager;
    }

    /// <summary>
    /// Load scene moi (single mode).
    /// Tu dong ShowLoading truoc khi load va HideLoading sau khi scene moi san sang. Hien tai van chua hien thi loadingpanel duoc.
    /// </summary>
    public async UniTask LoadScene(string scene)
    {
        if (string.IsNullOrWhiteSpace(scene))
        {
            Debug.LogError("[SceneManagerService] Scene name is empty.");
            return;
        }

        if (isLoading)
            return;

        isLoading = true;
        uiManager?.ShowLoading();

        try
        {
            await UnitySceneManager.LoadSceneAsync(scene);
        }
        finally
        {
            isLoading = false;
            uiManager?.HideLoading();
        }
    }

    public async UniTask LoadAdditive(string scene)
    {
        if (string.IsNullOrWhiteSpace(scene))
        {
            Debug.LogError("[SceneManagerService] Scene name is empty.");
            return;
        }

        await UnitySceneManager.LoadSceneAsync(scene, UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }

    public async UniTask Unload(string scene)
    {
        if (string.IsNullOrWhiteSpace(scene))
        {
            Debug.LogError("[SceneManagerService] Scene name is empty.");
            return;
        }

        await UnitySceneManager.UnloadSceneAsync(scene);
    }
}
