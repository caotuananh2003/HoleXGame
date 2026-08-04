using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// Panel Shop — 3 tab: Main (Bundle), HoleSkin, MapTheme.
/// Mode = Screen trong Inspector.
///
/// Hierarchy:
///   ShopPanel
///   ├── Navigation
///   │   ├── MainTabButton
///   │   ├── HoleSkinTabButton
///   │   └── MapThemeTabButton
///   │
///   ├── ItemScrollView                  ← root, toggle SetActive
///   │   └── Viewport
///   │       └── Content                ← bundleItemContent (parent prefab)
///   │
///   ├── HoleSkinScrollView              ← root, toggle SetActive
///   │   ├── Info                        ← ShopInfoPanel component
///   │   │   ├── Icon
///   │   │   ├── Name
///   │   │   └── ButtonObject
///   │   │       ├── BuyButton
///   │   │       ├── EquipButton
///   │   │       └── EquippedText
///   │   └── Board
///   │       ├── (Text "Mời lựa chọn")
///   │       └── ItemViewport
///   │           └── Content             ← holeSkinItemContent (parent prefab)
///   │
///   └── MapThemeScrollView              ← root, toggle SetActive
///       ├── Info                        ← ShopInfoPanel component
///       │   ├── Icon
///       │   ├── Name
///       │   └── ButtonObject
///       │       ├── BuyButton
///       │       ├── EquipButton
///       │       └── EquippedText
///       └── Board
///           ├── (Text "Mời lựa chọn")
///           └── ItemViewport
///               └── Content             ← mapThemeItemContent (parent prefab)
///
/// Prefab spawn vào Content ngay khi Start() — listsPopulated đảm bảo chỉ chạy 1 lần.
/// </summary>
public class ShopPanel : UIWindow
{
    private enum ShopTab { Main, HoleSkin, MapTheme }
    private ShopTab activeTab = ShopTab.Main;

    [Header("Tab Buttons")]
    [SerializeField] private Button mainTabButton;
    [SerializeField] private Button holeSkinTabButton;
    [SerializeField] private Button mapThemeTabButton;

    [Header("ScrollView Roots")]
    [SerializeField] private GameObject itemScrollView;
    [SerializeField] private GameObject holeSkinScrollView;
    [SerializeField] private GameObject mapThemeScrollView;

    [Header("Item Contents")]
    [SerializeField] private Transform bundleItemContent;
    [SerializeField] private Transform holeSkinItemContent;
    [SerializeField] private Transform mapThemeItemContent;

    // ── Info Panels — hiển thị item đang chọn ─────────────────────────────────
    [Header("Info Panels")]
    [SerializeField] private ShopInfoPanel holeSkinInfoPanel;
    [SerializeField] private ShopInfoPanel mapThemeInfoPanel;

    [Header("Item Prefabs")]
    [SerializeField] private ShopBundleItemUI   bundleItemPrefab;
    [SerializeField] private ShopHoleSkinItemUI holeSkinItemPrefab;
    [SerializeField] private ShopMapThemeItemUI mapThemeItemPrefab;

    [Header("Data")]
    [SerializeField] private PlayerProfile playerProfile;

    // ── Dependency Injection ───────────────────────────────────────────────────
    private SaveManager saveManager;

    [Inject]
    private void Construct(SaveManager saveManager)
    {
        this.saveManager = saveManager;
    }

    // Danh sách item đã spawn — dùng để SetSelected và unsubscribe OnClicked
    private readonly List<ShopBundleItemUI>   bundleItems   = new();
    private readonly List<ShopHoleSkinItemUI> holeSkinItems = new();
    private readonly List<ShopMapThemeItemUI> mapThemeItems = new();

    private bool listsPopulated;

    private void Start()
    {
        ValidateInspectorRefs(); // Log ra đảm bảo không có SerializeField Object bị null

        // TODO: hãy để profilePreview được init tại đây. Parameter truyền vào là các database của playerProfile
        // TODO: Spawn items 1 lần duy nhất cho các scrollview/content.

        // Spawn items một lần duy nhất.
        if (!listsPopulated)
        {
            PopulateAllLists();
            listsPopulated = true;
        }
    }

    private void OnEnable()
    {
        RegisterButtons();
        RegisterInfoPanelEvents();
        ShowTab(activeTab);
    }

    private void OnDisable()
    {
        // Hủy đăng ký info panel events khi panel tắt để tránh callback rác
        UnregisterInfoPanelEvents();
    }

    private void OnDestroy()
    {
        UnregisterButtons();
        UnregisterInfoPanelEvents();
        ClearAllLists();
    }

    // =========================================================================
    // Info Panel events — register/unregister
    // =========================================================================

    private void RegisterInfoPanelEvents()
    {
        if (holeSkinInfoPanel != null)
        {
            holeSkinInfoPanel.OnBuyClicked   += OnHoleSkinBuyClicked;
            holeSkinInfoPanel.OnEquipClicked += OnHoleSkinEquipClicked;
        }

        if (mapThemeInfoPanel != null)
        {
            mapThemeInfoPanel.OnBuyClicked   += OnMapThemeBuyClicked;
            mapThemeInfoPanel.OnEquipClicked += OnMapThemeEquipClicked;
        }
    }

    private void UnregisterInfoPanelEvents()
    {
        if (holeSkinInfoPanel != null)
        {
            holeSkinInfoPanel.OnBuyClicked   -= OnHoleSkinBuyClicked;
            holeSkinInfoPanel.OnEquipClicked -= OnHoleSkinEquipClicked;
        }

        if (mapThemeInfoPanel != null)
        {
            mapThemeInfoPanel.OnBuyClicked   -= OnMapThemeBuyClicked;
            mapThemeInfoPanel.OnEquipClicked -= OnMapThemeEquipClicked;
        }
    }

    // =========================================================================
    // Populate lists — chạy một lần trong Start()
    // =========================================================================

    private void PopulateAllLists()
    {
        if (playerProfile == null)
        {
            Debug.LogWarning("[ShopPanel] playerProfile is not assigned. Cannot populate lists.");
            return;
        }

        //// Tạm thời chưa phát triển phần này. Không được xóa đoạn code bị comment bên dưới.
        //if (playerProfile.BundleDatabase != null) // Spawn các item
        //{
        //    foreach (BundleDefinition def in playerProfile.BundleDatabase.Bundles)
        //    {
        //        ShopBundleItemUI item = Instantiate(bundleItemPrefab, bundleItemContent);
        //        item.Setup(def, unlocked: true);
        //        item.SetShopText(def.DisplayName, def.PriceLabel);
        //        item.OnClicked += OnBundleItemClicked;
        //        bundleItems.Add(item);
        //    }
        //}

        if (playerProfile.HoleSkinDatabase != null)
        {
            foreach (HoleSkinDefinition def in playerProfile.HoleSkinDatabase.HoleDefinition)
            {
                ShopHoleSkinItemUI item = Instantiate(holeSkinItemPrefab, holeSkinItemContent);
                bool unlocked = def.UnlockedByDefault;
                item.Setup(def, unlocked);
                item.SetShopText(def.DisplayName, unlocked ? "OWNED" : def.Price.ToString());
                item.OnClicked += OnHoleSkinItemClicked;
                holeSkinItems.Add(item);
            }
        }

        if (playerProfile.MapThemeDatabase != null)
        {
            foreach (MapThemeDefinition def in playerProfile.MapThemeDatabase.MapThemeDefinition)
            {
                ShopMapThemeItemUI item = Instantiate(mapThemeItemPrefab, mapThemeItemContent);
                bool unlocked = def.UnlockedByDefault;
                item.Setup(def, unlocked);
                item.SetShopText(def.DisplayName, unlocked ? "OWNED" : def.Price.ToString());
                item.OnClicked += OnMapThemeItemClicked;
                mapThemeItems.Add(item);
            }
        }
    }

    // =========================================================================
    // Item click handlers — cập nhật Info panel khi chọn item trên scrollview
    // =========================================================================

    private void OnBundleItemClicked(string bundleId)
    {
        Debug.Log($"[ShopPanel] Bundle clicked: {bundleId}");
        // TODO: gọi PurchaseService.Purchase(bundleId)
    }

    private void OnHoleSkinItemClicked(string holeSkinId)
    {
        if (playerProfile.HoleSkinDatabase == null) return;

        HoleSkinDefinition def = playerProfile.HoleSkinDatabase.GetById(holeSkinId);
        if (def == null) return;

        // Xác định trạng thái: đang trang bị / đã sở hữu / chưa sở hữu
        ItemActionState state = GetHoleSkinState(holeSkinId, def.UnlockedByDefault);

        // Cập nhật Info panel
        holeSkinInfoPanel?.Show(holeSkinId, def.Icon, def.DisplayName, state);

        // Highlight item đang chọn trên scrollview
        foreach (ShopHoleSkinItemUI item in holeSkinItems)
            item.SetSelected(item.ItemId == holeSkinId);
    }

    private void OnMapThemeItemClicked(string mapThemeId)
    {
        if (playerProfile.MapThemeDatabase == null) return;

        MapThemeDefinition def = playerProfile.MapThemeDatabase.GetById(mapThemeId);
        if (def == null) return;

        ItemActionState state = GetMapThemeState(mapThemeId, def.UnlockedByDefault);

        mapThemeInfoPanel?.Show(mapThemeId, def.EnableIcon, def.DisplayName, state);

        foreach (ShopMapThemeItemUI item in mapThemeItems)
            item.SetSelected(item.ItemId == mapThemeId);
    }

    // =========================================================================
    // State helpers — đọc từ SaveManager
    // =========================================================================

    /// <summary>
    /// Xác định ItemActionState của một HoleSkin dựa vào SaveManager.
    /// </summary>
    private ItemActionState GetHoleSkinState(string holeSkinId, bool unlockedByDefault)
    {
        if (saveManager?.Data == null)
            return unlockedByDefault ? ItemActionState.Owned : ItemActionState.NotOwned;

        bool isEquipped = saveManager.Data.equippedHoleSkinId == holeSkinId;
        if (isEquipped) return ItemActionState.Equipped;

        // TODO: kiểm tra danh sách ownedHoleSkins khi có unlock system
        bool isOwned = unlockedByDefault;
        return isOwned ? ItemActionState.Owned : ItemActionState.NotOwned;
    }

    /// <summary>
    /// Xác định ItemActionState của một MapTheme dựa vào SaveManager.
    /// </summary>
    private ItemActionState GetMapThemeState(string mapThemeId, bool unlockedByDefault)
    {
        if (saveManager?.Data == null)
            return unlockedByDefault ? ItemActionState.Owned : ItemActionState.NotOwned;

        bool isEquipped = saveManager.Data.equippedMapThemeId == mapThemeId;
        if (isEquipped) return ItemActionState.Equipped;

        // TODO: kiểm tra danh sách ownedMapThemes khi có unlock system
        bool isOwned = unlockedByDefault;
        return isOwned ? ItemActionState.Owned : ItemActionState.NotOwned;
    }

    // =========================================================================
    // Info panel action handlers — Buy / Equip
    // =========================================================================

    private void OnHoleSkinBuyClicked(string holeSkinId)
    {
        Debug.Log($"[ShopPanel] HoleSkin Buy: {holeSkinId}");
        // TODO: show rewarded ads, sau khi xem xong gọi UnlockHoleSkin(holeSkinId)
    }

    private void OnHoleSkinEquipClicked(string holeSkinId)
    {
        if (saveManager?.Data == null) return;

        saveManager.Data.equippedHoleSkinId = holeSkinId;
        saveManager.Save().Forget();

        Debug.Log($"[ShopPanel] HoleSkin equipped: {holeSkinId}");

        // Refresh lại Info panel để hiển thị trạng thái "Đang trang bị"
        OnHoleSkinItemClicked(holeSkinId);
    }

    private void OnMapThemeBuyClicked(string mapThemeId)
    {
        Debug.Log($"[ShopPanel] MapTheme Buy: {mapThemeId}");
        // TODO: show rewarded ads, sau khi xem xong gọi UnlockMapTheme(mapThemeId)
    }

    private void OnMapThemeEquipClicked(string mapThemeId)
    {
        if (saveManager?.Data == null) return;

        saveManager.Data.equippedMapThemeId = mapThemeId;
        saveManager.Save().Forget();

        Debug.Log($"[ShopPanel] MapTheme equipped: {mapThemeId}");

        // Refresh lại Info panel để hiển thị trạng thái "Đang trang bị"
        OnMapThemeItemClicked(mapThemeId);
    }

    // =========================================================================
    // Tab
    // =========================================================================

    #region TabButton
    private void ShowTab(ShopTab tab)
    {
        activeTab = tab;

        if (itemScrollView     != null) itemScrollView.SetActive(tab == ShopTab.Main);
        if (holeSkinScrollView != null) holeSkinScrollView.SetActive(tab == ShopTab.HoleSkin);
        if (mapThemeScrollView != null) mapThemeScrollView.SetActive(tab == ShopTab.MapTheme);
    }

    private void OnMainTabClicked()     => ShowTab(ShopTab.Main);
    private void OnHoleSkinTabClicked() => ShowTab(ShopTab.HoleSkin);
    private void OnMapThemeTabClicked() => ShowTab(ShopTab.MapTheme);

    private void RegisterButtons()
    {
        if (mainTabButton     != null) mainTabButton.onClick.AddListener(OnMainTabClicked);
        if (holeSkinTabButton != null) holeSkinTabButton.onClick.AddListener(OnHoleSkinTabClicked);
        if (mapThemeTabButton != null) mapThemeTabButton.onClick.AddListener(OnMapThemeTabClicked);
    }

    private void UnregisterButtons()
    {
        if (mainTabButton     != null) mainTabButton.onClick.RemoveListener(OnMainTabClicked);
        if (holeSkinTabButton != null) holeSkinTabButton.onClick.RemoveListener(OnHoleSkinTabClicked);
        if (mapThemeTabButton != null) mapThemeTabButton.onClick.RemoveListener(OnMapThemeTabClicked);
    }
    #endregion

    // =========================================================================
    // Cleanup
    // =========================================================================

    private void ClearAllLists()
    {
        foreach (ShopBundleItemUI item in bundleItems)
            if (item != null) item.OnClicked -= OnBundleItemClicked;
        bundleItems.Clear();

        foreach (ShopHoleSkinItemUI item in holeSkinItems)
            if (item != null) item.OnClicked -= OnHoleSkinItemClicked;
        holeSkinItems.Clear();

        foreach (ShopMapThemeItemUI item in mapThemeItems)
            if (item != null) item.OnClicked -= OnMapThemeItemClicked;
        mapThemeItems.Clear();
    }

    // Hàm kiểm tra xem đã kéo ref đủ trong inspector chưa
    private void ValidateInspectorRefs()
    {
        if (mainTabButton        == null) Debug.LogWarning("[ShopPanel] mainTabButton is not assigned.");
        if (holeSkinTabButton    == null) Debug.LogWarning("[ShopPanel] holeSkinTabButton is not assigned.");
        if (mapThemeTabButton    == null) Debug.LogWarning("[ShopPanel] mapThemeTabButton is not assigned.");
        if (itemScrollView       == null) Debug.LogWarning("[ShopPanel] itemScrollView is not assigned.");
        if (holeSkinScrollView   == null) Debug.LogWarning("[ShopPanel] holeSkinScrollView is not assigned.");
        if (mapThemeScrollView   == null) Debug.LogWarning("[ShopPanel] mapThemeScrollView is not assigned.");
        if (bundleItemContent    == null) Debug.LogWarning("[ShopPanel] bundleItemContent is not assigned.");
        if (holeSkinItemContent  == null) Debug.LogWarning("[ShopPanel] holeSkinItemContent is not assigned.");
        if (mapThemeItemContent  == null) Debug.LogWarning("[ShopPanel] mapThemeItemContent is not assigned.");
        if (holeSkinInfoPanel    == null) Debug.LogWarning("[ShopPanel] holeSkinInfoPanel is not assigned.");
        if (mapThemeInfoPanel    == null) Debug.LogWarning("[ShopPanel] mapThemeInfoPanel is not assigned.");
        if (bundleItemPrefab     == null) Debug.LogWarning("[ShopPanel] bundleItemPrefab is not assigned.");
        if (holeSkinItemPrefab   == null) Debug.LogWarning("[ShopPanel] holeSkinItemPrefab is not assigned.");
        if (mapThemeItemPrefab   == null) Debug.LogWarning("[ShopPanel] mapThemeItemPrefab is not assigned.");
        if (playerProfile        == null) Debug.LogWarning("[ShopPanel] playerProfile is not assigned.");
    }
}
