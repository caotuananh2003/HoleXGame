using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public enum WindowMode // Khai báo trên mỗi UIWindow để triển khai cách xử lý
{
    /// <summary>
    /// Luon hien thi, UIManager khong bao gio tu dong dong.
    /// Vi du: BottomPanel, TopBar, HUD.
    /// </summary>
    Persistent,

    /// <summary>
    /// Man hinh chinh. Mo cai moi se dong Screen dang mo (neu co), roi setActive(true) cho cai moi.
    /// Vi du: MainmenuPanel, ShopPanel, CollectionPanel.
    /// </summary>
    Screen,

    /// <summary>
    /// Overlay de len tat ca. Khong dong bat ky window nao khi mo.
    /// Vi du: SettingPanel, ConfirmDialog, RewardPopup.
    /// </summary>
    Popup,
}
public class UIManager : MonoBehaviour
{
    private readonly Dictionary<Type, UIWindow> _windowsInScene = new(); // Cache tat ca UIWindow theo Type

    private UIWindow _currentScreen; // Dùng để thực hiện close đúng screen hiện tại khi mở screen mới

    private AudioManager _audioManager;

    [Inject]
    private void Construct(AudioManager audioManager)
    {
        this._audioManager = audioManager;
    }

    #region Dang ky / huy dang ky scene UI
    /// <summary>
    /// UISceneRoot sẽ gọi RegisterSceneUI, root là Canvas tổng của Scene được gắn UISceneRoot.
    /// Xóa toàn bộ cache cũ trước khi load Scene mới.
    /// Dictionary sẽ lưu các cặp key-value là type và UIWindow tương ứng
    /// Truyền UIManager cho các window có trong Scene
    /// </summary>
    /// <param name="root"></param>
    public void RegisterSceneUI(Transform root) // 
    {
        UnregisterSceneUI();

        UIWindow[] windows = root.GetComponentsInChildren<UIWindow>(true);

        foreach (UIWindow window in windows)
        {
            Type windowType = window.GetType();

            if (_windowsInScene.ContainsKey(windowType))
            {
                Debug.LogWarning($"[UIManager] Duplicate UIWindow type: {windowType.Name}. First instance kept.");
                continue;
            }

            window.Initialize(this); // Truyền UIManager cho các window có trong Scene
            _windowsInScene.Add(windowType, window); // Thêm cặp key-value vào dict để truy xuất cho nhanh
        }

        Debug.Log($"[UIManager] Cached {windows.Length} windows from '{root.name}'.");
    }

    public void UnregisterSceneUI()
    {
        _windowsInScene.Clear();
        _currentScreen = null;
    }

    #endregion

    #region Cac ham dong/mo Panel
    public T Open<T>() where T : UIWindow // Mở UIWindow và trả về giá trị UIWindow tương ứng.
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
        if (!_windowsInScene.TryGetValue(typeof(T), out UIWindow window) || window == null)
            return;

        window.Close();

        if (window.Mode == WindowMode.Screen && _currentScreen == window)
            _currentScreen = null;
    }
    #endregion

    public T GetWindow<T>() where T : UIWindow // Trả về UIWindow để thực hiện các thao tác cập nhật.
    {
        if (_windowsInScene.TryGetValue(typeof(T), out UIWindow window) && window != null)
            return (T)window;
        return null;
    }

    public void PlaySFX(string id) => _audioManager?.PlaySFX(id);
}

#region UIWindow base class
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
#endregion