using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

// -----------------------------------------------------------------------------
// Window mode — khai bao tren moi UIWindow subclass de UIManager biet cach xu ly
// -----------------------------------------------------------------------------

public enum WindowMode
{
    /// <summary>
    /// Luon hien thi, UIManager khong bao gio tu dong dong.
    /// Vi du: BottomPanel, TopBar, HUD.
    /// </summary>
    Persistent,

    /// <summary>
    /// Man hinh chinh. Mo cai moi se dong cai Screen dang mo.
    /// UIManager track bang screenStack.
    /// Vi du: MainmenuPanel, ShopPanel, CollectionPanel.
    /// </summary>
    Screen,

    /// <summary>
    /// Overlay de len tat ca. Khong dong bat ky window nao khi mo.
    /// Back() se dong popup va tra ve trang thai truoc.
    /// Vi du: SettingPanel, ConfirmDialog, RewardPopup.
    /// </summary>
    Popup,
}

public class UIManager : MonoBehaviour
{
    [Header("Loading Overlay")]
    [SerializeField] private GameObject loadingOverlay;

    [Header("Dynamic Loading (fallback)")]
    [SerializeField] private string resourcesFolder = "UI";

    // Cache tat ca UIWindow theo Type
    private readonly Dictionary<Type, UIWindow> windowsByType = new();

    // Stack cho Screen windows (history de Back() hoat dong)
    private readonly Stack<Type> screenStack = new();

    // Stack cho Popup windows (LIFO, moi popup de len popup truoc)
    private readonly Stack<Type> popupStack = new();

    private Transform currentSceneUIRoot;

    private AudioManager audioManager;

    [Inject]
    private void Construct(AudioManager audioManager)
    {
        this.audioManager = audioManager;
    }

    public void Initialize()
    {
        if (loadingOverlay == null)
            Debug.LogWarning("[UIManager] LoadingOverlay is not assigned in Inspector.");
    }

    #region Dang ky / huy dang ky scene UI (goi boi UISceneRoot)

    public void RegisterSceneUI(Transform sceneRoot)
    {
        currentSceneUIRoot = sceneRoot;
        CacheSceneWindows(sceneRoot);
    }

    private void CacheSceneWindows(Transform root)
    {
        // Xoa toan bo cache cu truoc khi load scene moi
        windowsByType.Clear();
        screenStack.Clear();
        popupStack.Clear();
        currentSceneUIRoot = null;

        UIWindow[] windows = root.GetComponentsInChildren<UIWindow>(true);

        foreach (UIWindow window in windows)
        {
            Type windowType = window.GetType();

            if (windowsByType.ContainsKey(windowType))
            {
                Debug.LogWarning($"[UIManager] Duplicate UIWindow type: {windowType.Name}. First instance kept.");
                continue;
            }

            window.Initialize(this);
            windowsByType.Add(windowType, window);
        }

        currentSceneUIRoot = root;
        Debug.Log($"[UIManager] Cached {windows.Length} windows from '{root.name}'.");
    }

    public void UnregisterSceneUI()
    {
        windowsByType.Clear();
        screenStack.Clear();
        popupStack.Clear();
        currentSceneUIRoot = null;
    }

    #endregion

    #region Cac ham dong/mo Panel
    /// <summary>
    /// Mo mot UIWindow. Hanh vi phu thuoc WindowMode cua target:
    /// - Persistent: khong lam gi (Persistent tu quan ly trang thai cua minh).
    /// - Screen: dong Screen dang mo, push target vao screenStack.
    /// - Popup: mo de len tat ca, push vao popupStack.
    /// </summary>
    public T Open<T>() where T : UIWindow
    {
        T target = GetOrLoadWindow<T>();
        if (target == null) return null;

        switch (target.Mode)
        {
            case WindowMode.Persistent:
                // Persistent tu mo trong Initialize hoac Awake — UIManager khong can lam gi
                Debug.LogWarning($"[UIManager] Open<{typeof(T).Name}>: Persistent windows tu quan ly trang thai. Dung Open() truc tiep tren component.");
                break;

            case WindowMode.Screen:
                OpenScreen(target);
                break;

            case WindowMode.Popup:
                OpenPopup(target);
                break;
        }

        return target;
    }

    private T GetOrLoadWindow<T>() where T : UIWindow
    {
        Type windowType = typeof(T);

        if (windowsByType.TryGetValue(windowType, out UIWindow cached) && cached != null)
            return (T)cached;

        T prefab = Resources.Load<T>($"{resourcesFolder}/{windowType.Name}");
        if (prefab == null)
        {
            Debug.LogWarning($"[UIManager] '{windowType.Name}' not found in scene or Resources/{resourcesFolder}.");
            return null;
        }

        Transform parent = currentSceneUIRoot != null ? currentSceneUIRoot : transform;
        T window = Instantiate(prefab, parent);
        window.Initialize(this);
        windowsByType[windowType] = window;
        return window;
    }

    // Dong screen hien tai, mo screen moi.
    private void OpenScreen(UIWindow target)
    {
        if (screenStack.Count > 0 && windowsByType.TryGetValue(screenStack.Peek(), out UIWindow current))
            current?.Close();

        screenStack.Push(target.GetType());
        target.Open();
    }

    /// <summary>
    /// Đăng ký một Screen đang active sẵn vào screenStack mà không Open/Close gì.
    /// Dùng cho screen mặc định đã được bật sẵn trong hierarchy khi scene load
    /// (ví dụ: MainmenuPanel). Nếu không gọi hàm này, screenStack sẽ rỗng và
    /// OpenScreen sẽ không biết screen nào cần đóng khi chuyển sang screen mới.
    /// </summary>
    public void RegisterInitialScreen<T>() where T : UIWindow
    {
        Type windowType = typeof(T);

        if (!windowsByType.ContainsKey(windowType))
        {
            Debug.LogWarning($"[UIManager] RegisterInitialScreen<{windowType.Name}>: window chưa được cache. Gọi sau UISceneRoot.Start().");
            return;
        }

        if (screenStack.Count > 0 && screenStack.Peek() == windowType)
            return;

        screenStack.Push(windowType);
        Debug.Log($"[UIManager] RegisterInitialScreen: {windowType.Name} đã được push vào screenStack.");
    }

    // Khong dong screen hien tai, mo popup de len.
    private void OpenPopup(UIWindow target)
    {
        popupStack.Push(target.GetType());
        target.Open();
    }

    // Dong window
    public void Close<T>() where T : UIWindow
    {
        if (!windowsByType.TryGetValue(typeof(T), out UIWindow window) || window == null)
            return;

        CloseWindow(window);
    }

    private void CloseWindow(UIWindow window)
    {
        window.Close();

        Type windowType = window.GetType();

        if (window.Mode == WindowMode.Popup && popupStack.Count > 0 && popupStack.Peek() == windowType)
            popupStack.Pop();
        else if (window.Mode == WindowMode.Screen && screenStack.Count > 0 && screenStack.Peek() == windowType)
            screenStack.Pop();
    }

    /// <summary>
    /// Dong popup dang mo nhat (neu co), hoac Screen tren cung neu khong co popup.
    /// Persistent khong bi anh huong.
    /// </summary>
    public void Back()
    {
        // Uu tien dong popup truoc
        if (popupStack.Count > 0)
        {
            Type topPopup = popupStack.Pop();
            if (windowsByType.TryGetValue(topPopup, out UIWindow popup))
                popup?.Close();
            return;
        }

        // Neu chi con 1 Screen thi khong Back nua (day la root)
        if (screenStack.Count <= 1) return;

        // Dong Screen hien tai
        Type currentType = screenStack.Pop();
        if (windowsByType.TryGetValue(currentType, out UIWindow current))
            current?.Close();

        // Mo lai Screen truoc do
        Type previousType = screenStack.Peek();
        if (windowsByType.TryGetValue(previousType, out UIWindow previous))
            previous?.Open();
    }

    public void CloseAllPopups()
    {
        while (popupStack.Count > 0)
        {
            Type t = popupStack.Pop();
            if (windowsByType.TryGetValue(t, out UIWindow w))
                w?.Close();
        }
    }
    #endregion

    // -------------------------------------------------------------------------
    // Utility
    // -------------------------------------------------------------------------

    public T GetWindow<T>() where T : UIWindow
    {
        if (windowsByType.TryGetValue(typeof(T), out UIWindow window) && window != null)
            return (T)window;
        return null;
    }

    public void PlaySFX(string id) => audioManager?.PlaySFX(id);

    // -------------------------------------------------------------------------
    // Loading overlay
    // -------------------------------------------------------------------------

    public void ShowLoading() => SetLoadingOverlayVisible(true);
    public void HideLoading() => SetLoadingOverlayVisible(false);

    private void SetLoadingOverlayVisible(bool visible)
    {
        if (loadingOverlay == null) return;

        CanvasGroup cg = loadingOverlay.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = visible ? 1f : 0f;
            cg.blocksRaycasts = visible;
            cg.interactable = visible;
        }
        else
        {
            loadingOverlay.SetActive(visible);
        }
    }
}

// =============================================================================
// UIWindow base class
// =============================================================================

public abstract class UIWindow : MonoBehaviour
{
    /// <summary>
    /// Khai bao mode nay tren moi subclass de UIManager biet cach xu ly.
    /// Override bang cach override property nay, hoac de SerializeField.
    /// </summary>
    [SerializeField] private WindowMode mode;
    public WindowMode Mode => mode;

    public bool IsOpen { get; private set; }
    public UIManager UIManager { get; private set; }

    /// <summary>
    /// Cho phép subclass (PopupWindow) cập nhật IsOpen mà không gọi SetActive.
    /// </summary>
    protected void SetIsOpen(bool value) => IsOpen = value;

    public virtual void Initialize(UIManager uiManager)
    {
        UIManager = uiManager;

        // Persistent window: dam bao luon Active
        if (mode == WindowMode.Persistent)
        {
            IsOpen = true;
            gameObject.SetActive(true);
        }
        else
        {
            IsOpen = gameObject.activeSelf;
        }
    }

    public virtual void Open()
    {
        IsOpen = true;
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        // Persistent khong bao gio bi Close
        if (mode == WindowMode.Persistent) return;

        IsOpen = false;
        gameObject.SetActive(false);
    }
}
