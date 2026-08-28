using UnityEngine;

public class UISceneRoot : MonoBehaviour
{
    public static UISceneRoot Instance { get; private set; }

    private void Awake()     { Instance = this; }
    private void OnDestroy() { if (Instance == this) Instance = null; }

    public void RegisterAll()
    {
        UIManager.Instance.RegisterSceneUI(transform);
    }
}
