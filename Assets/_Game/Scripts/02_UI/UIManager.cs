using System;
using System.Collections.Generic;
using UnityEngine;

public enum WindowMode
{
    Persistent,
    Screen,
    Popup,
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private void Awake()     { Instance = this; }
    private void OnDestroy() { if (Instance == this) Instance = null; }

    private readonly Dictionary<Type, UIWindow> _windowsInScene = new();
    private UIWindow _currentScreen;

    public void RegisterSceneUI(Transform root)
    {
        UIWindow[] windows = root.GetComponentsInChildren<UIWindow>(true);

        foreach (UIWindow window in windows)
        {
            Type windowType = window.GetType();
            if (_windowsInScene.ContainsKey(windowType))
            {
                Debug.LogWarning($"[UIManager] Duplicate UIWindow type: {windowType.Name}. First instance kept.");
                continue;
            }
            window.Initialize(this);
            _windowsInScene.Add(windowType, window);
        }

        Debug.Log($"[UIManager] Registered {windows.Length} windows from '{root.name}'.");
    }

    public T Open<T>() where T : UIWindow
    {
        if (!_windowsInScene.TryGetValue(typeof(T), out UIWindow cached) || cached == null)
        {
            Debug.LogWarning($"[UIManager] '{typeof(T).Name}' not found in scene.");
            return null;
        }

        T target = (T)cached;

        switch (target.Mode)
        {
            case WindowMode.Persistent:
                Debug.LogWarning($"[UIManager] Open<{typeof(T).Name}>: Persistent windows tu quan ly trang thai.");
                break;
            case WindowMode.Screen:
                _currentScreen?.Close();
                _currentScreen = target;
                target.Open();
                break;
            case WindowMode.Popup:
                target.Open();
                break;
        }

        return target;
    }

    public void Close<T>() where T : UIWindow
    {
        if (!_windowsInScene.TryGetValue(typeof(T), out UIWindow window) || window == null) return;
        window.Close();
        if (window.Mode == WindowMode.Screen && _currentScreen == window)
            _currentScreen = null;
    }

    public T GetWindow<T>() where T : UIWindow
    {
        if (_windowsInScene.TryGetValue(typeof(T), out UIWindow window) && window != null)
            return (T)window;
        return null;
    }

    public void PlaySFX(string id) => AudioManager.Instance?.PlaySFX(id);
}

public abstract class UIWindow : MonoBehaviour
{
    [SerializeField] private WindowMode mode;
    public WindowMode Mode => mode;

    public UIManager UIManager { get; private set; }

    public virtual void Initialize(UIManager uiManager)
    {
        UIManager = uiManager;
        if (mode == WindowMode.Persistent)
            gameObject.SetActive(true);
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        if (mode == WindowMode.Persistent) return;
        gameObject.SetActive(false);
    }
}
