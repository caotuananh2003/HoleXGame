using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShopPanel : UIWindow
{
    private enum ShopTab { Main, HoleSkin, MapTheme }
    private ShopTab activeTab = ShopTab.Main;

    [SerializeField] private TMP_Text currencyText;

    [Header("Tab Buttons")]
    [SerializeField] private TabButton mainTabButton;
    [SerializeField] private TabButton holeSkinTabButton;
    [SerializeField] private TabButton mapThemeTabButton;

    [Header("ScrollView Roots")]
    [SerializeField] private GameObject itemScrollView;
    [SerializeField] private GameObject holeSkinScrollView;
    [SerializeField] private GameObject mapThemeScrollView;

    [Header("Item Contents")]
    [SerializeField] private Transform bundleItemContent;
    [SerializeField] private Transform holeSkinItemContent;
    [SerializeField] private Transform mapThemeItemContent;

    [Header("Info Panels")]
    [SerializeField] private ShopInfoPanel holeSkinInfoPanel;
    [SerializeField] private ShopInfoPanel mapThemeInfoPanel;

    [Header("Item Prefabs")]
    [SerializeField] private ShopBundleItemUI   bundleItemPrefab;
    [SerializeField] private ShopHoleSkinItemUI holeSkinItemPrefab;
    [SerializeField] private ShopMapThemeItemUI mapThemeItemPrefab;

    [SerializeField] private PlayerProfile playerProfile;

    private readonly List<ShopBundleItemUI>   bundleItems   = new();
    private readonly List<ShopHoleSkinItemUI> holeSkinItems = new();
    private readonly List<ShopMapThemeItemUI> mapThemeItems = new();
    private bool listsPopulated;

    private void Start()
    {
        if (!listsPopulated) { PopulateAllLists(); listsPopulated = true; }
    }

    private void OnEnable()  { RefreshCurrency(); RegisterButtons(); RegisterInfoPanelEvents(); ShowTab(activeTab); }
    private void OnDisable() { UnregisterInfoPanelEvents(); }
    private void OnDestroy() { UnregisterInfoPanelEvents(); ClearAllLists(); }

    private void RefreshCurrency()
    {
        if (SaveManager.Instance?.PlayerData == null || currencyText == null) return;
        currencyText.text = SaveManager.Instance.PlayerData.currency.ToString();
    }

    private void RegisterInfoPanelEvents()
    {
        if (holeSkinInfoPanel != null) { holeSkinInfoPanel.OnBuyByCurrencyClicked += OnHoleSkinBuyByCurrencyClicked; holeSkinInfoPanel.OnBuyByAdsClicked += OnHoleSkinBuyByAdsClicked; holeSkinInfoPanel.OnEquipClicked += OnHoleSkinEquipClicked; }
        if (mapThemeInfoPanel != null) { mapThemeInfoPanel.OnBuyByCurrencyClicked += OnMapThemeBuyByCurrencyClicked; mapThemeInfoPanel.OnBuyByAdsClicked += OnMapThemeBuyByAdsClicked; mapThemeInfoPanel.OnEquipClicked += OnMapThemeEquipClicked; }
    }

    private void UnregisterInfoPanelEvents()
    {
        if (holeSkinInfoPanel != null) { holeSkinInfoPanel.OnBuyByCurrencyClicked -= OnHoleSkinBuyByCurrencyClicked; holeSkinInfoPanel.OnBuyByAdsClicked -= OnHoleSkinBuyByAdsClicked; holeSkinInfoPanel.OnEquipClicked -= OnHoleSkinEquipClicked; }
        if (mapThemeInfoPanel != null) { mapThemeInfoPanel.OnBuyByCurrencyClicked -= OnMapThemeBuyByCurrencyClicked; mapThemeInfoPanel.OnBuyByAdsClicked -= OnMapThemeBuyByAdsClicked; mapThemeInfoPanel.OnEquipClicked -= OnMapThemeEquipClicked; }
    }

    private void PopulateAllLists()
    {
        if (playerProfile == null) { Debug.LogWarning("[ShopPanel] playerProfile is not assigned."); return; }
        SyncDefaultUnlocks();

        if (playerProfile.HoleSkinDatabase != null)
            foreach (var def in playerProfile.HoleSkinDatabase.HoleDefinition)
            { var item = Instantiate(holeSkinItemPrefab, holeSkinItemContent); bool u = IsHoleSkinUnlocked(def.Id); item.Setup(def, u); item.SetShopText(def.DisplayName, u ? "OWNED" : def.Price.ToString()); item.OnClicked += OnHoleSkinItemClicked; holeSkinItems.Add(item); }

        if (playerProfile.MapThemeDatabase != null)
            foreach (var def in playerProfile.MapThemeDatabase.MapThemeDefinition)
            { var item = Instantiate(mapThemeItemPrefab, mapThemeItemContent); bool u = IsMapThemeUnlocked(def.Id); item.Setup(def, u); item.SetShopText(def.DisplayName, u ? "OWNED" : def.Price.ToString()); item.OnClicked += OnMapThemeItemClicked; mapThemeItems.Add(item); }
    }

    private void OnBundleItemClicked(string id) { RefreshCurrency(); }

    private void OnHoleSkinItemClicked(string id)
    {
        var def = playerProfile.HoleSkinDatabase?.GetById(id); if (def == null) return;
        holeSkinInfoPanel?.Show(id, def.Icon, def.DisplayName, GetHoleSkinState(id), def.Price);
        foreach (var item in holeSkinItems) item.SetSelected(item.ItemId == id);
    }

    private void OnMapThemeItemClicked(string id)
    {
        var def = playerProfile.MapThemeDatabase?.GetById(id); if (def == null) return;
        mapThemeInfoPanel?.Show(id, def.EnableIcon, def.DisplayName, GetMapThemeState(id), def.Price);
        foreach (var item in mapThemeItems) item.SetSelected(item.ItemId == id);
    }

    private void SyncDefaultUnlocks()
    {
        if (SaveManager.Instance?.PlayerData == null) return;
        bool dirty = false;
        if (playerProfile.HoleSkinDatabase != null)
            foreach (var def in playerProfile.HoleSkinDatabase.HoleDefinition)
                if (def.UnlockedByDefault && !SaveManager.Instance.PlayerData.unlockedHoleSkinIds.Contains(def.Id)) { SaveManager.Instance.PlayerData.unlockedHoleSkinIds.Add(def.Id); dirty = true; }
        if (playerProfile.MapThemeDatabase != null)
            foreach (var def in playerProfile.MapThemeDatabase.MapThemeDefinition)
                if (def.UnlockedByDefault && !SaveManager.Instance.PlayerData.unlockedMapThemeIds.Contains(def.Id)) { SaveManager.Instance.PlayerData.unlockedMapThemeIds.Add(def.Id); dirty = true; }
        if (dirty) SaveManager.Instance.Save().Forget();
    }

    private bool IsHoleSkinUnlocked(string id) => SaveManager.Instance?.PlayerData != null && SaveManager.Instance.PlayerData.unlockedHoleSkinIds.Contains(id);
    private bool IsMapThemeUnlocked(string id)  => SaveManager.Instance?.PlayerData != null && SaveManager.Instance.PlayerData.unlockedMapThemeIds.Contains(id);

    private ItemActionState GetHoleSkinState(string id)
    {
        if (SaveManager.Instance?.PlayerData == null) return ItemActionState.NotOwned;
        if (SaveManager.Instance.PlayerData.equippedHoleSkinId == id) return ItemActionState.Equipped;
        return IsHoleSkinUnlocked(id) ? ItemActionState.Owned : ItemActionState.NotOwned;
    }

    private ItemActionState GetMapThemeState(string id)
    {
        if (SaveManager.Instance?.PlayerData == null) return ItemActionState.NotOwned;
        if (SaveManager.Instance.PlayerData.equippedMapThemeId == id) return ItemActionState.Equipped;
        return IsMapThemeUnlocked(id) ? ItemActionState.Owned : ItemActionState.NotOwned;
    }

    private void OnHoleSkinBuyByCurrencyClicked(string id) { var def = playerProfile.HoleSkinDatabase?.GetById(id); if (def != null && RemoveCurrency(def.Price)) UnlockHoleSkin(id); }
    private void OnHoleSkinBuyByAdsClicked(string id)      { UnlockHoleSkin(id); }
    private void OnHoleSkinEquipClicked(string id)         { if (SaveManager.Instance?.PlayerData == null) return; SaveManager.Instance.PlayerData.equippedHoleSkinId = id; SaveManager.Instance.Save().Forget(); OnHoleSkinItemClicked(id); }

    private void UnlockHoleSkin(string id)
    {
        if (SaveManager.Instance?.PlayerData == null) return;
        if (!SaveManager.Instance.PlayerData.unlockedHoleSkinIds.Contains(id)) SaveManager.Instance.PlayerData.unlockedHoleSkinIds.Add(id);
        SaveManager.Instance.Save().Forget();
        holeSkinItems.Find(x => x.ItemId == id)?.SetUnlocked(true);
        OnHoleSkinItemClicked(id);
    }

    private void OnMapThemeBuyByCurrencyClicked(string id) { var def = playerProfile.MapThemeDatabase?.GetById(id); if (def != null && RemoveCurrency(def.Price)) UnlockMapTheme(id); }
    private void OnMapThemeBuyByAdsClicked(string id)      { UnlockMapTheme(id); }
    private void OnMapThemeEquipClicked(string id)         { if (SaveManager.Instance?.PlayerData == null) return; SaveManager.Instance.PlayerData.equippedMapThemeId = id; SaveManager.Instance.Save().Forget(); OnMapThemeItemClicked(id); }

    private void UnlockMapTheme(string id)
    {
        if (SaveManager.Instance?.PlayerData == null) return;
        if (!SaveManager.Instance.PlayerData.unlockedMapThemeIds.Contains(id)) SaveManager.Instance.PlayerData.unlockedMapThemeIds.Add(id);
        SaveManager.Instance.Save().Forget();
        mapThemeItems.Find(x => x.ItemId == id)?.SetUnlocked(true);
        OnMapThemeItemClicked(id);
    }

    private void ShowTab(ShopTab tab)
    {
        activeTab = tab;
        if (itemScrollView     != null) itemScrollView.SetActive(tab == ShopTab.Main);
        if (holeSkinScrollView != null) holeSkinScrollView.SetActive(tab == ShopTab.HoleSkin);
        if (mapThemeScrollView != null) mapThemeScrollView.SetActive(tab == ShopTab.MapTheme);
        mainTabButton?.SetSelected(activeTab == ShopTab.Main);
        holeSkinTabButton?.SetSelected(activeTab == ShopTab.HoleSkin);
        mapThemeTabButton?.SetSelected(activeTab == ShopTab.MapTheme);
    }

    private void RegisterButtons()
    {
        mainTabButton?.Initialize(OnMainTabClicked);
        holeSkinTabButton?.Initialize(OnHoleSkinTabClicked);
        mapThemeTabButton?.Initialize(OnMapThemeTabClicked);
    }

    private void OnMainTabClicked()     => ShowTab(ShopTab.Main);
    private void OnHoleSkinTabClicked() => ShowTab(ShopTab.HoleSkin);
    private void OnMapThemeTabClicked() => ShowTab(ShopTab.MapTheme);

    private void ClearAllLists()
    {
        foreach (var item in holeSkinItems) if (item != null) item.OnClicked -= OnHoleSkinItemClicked; holeSkinItems.Clear();
        foreach (var item in mapThemeItems) if (item != null) item.OnClicked -= OnMapThemeItemClicked; mapThemeItems.Clear();
    }

    private void AddCurrency(int amount)    { if (SaveManager.Instance?.PlayerData == null) return; SaveManager.Instance.PlayerData.currency += amount; SaveManager.Instance.Save().Forget(); RefreshCurrency(); }
    private bool RemoveCurrency(int amount) { if (SaveManager.Instance?.PlayerData == null) return false; if (SaveManager.Instance.PlayerData.currency < amount) return false; SaveManager.Instance.PlayerData.currency -= amount; SaveManager.Instance.Save().Forget(); RefreshCurrency(); return true; }

    private void Update()
    {
#if UNITY_EDITOR
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) AddCurrency(1000);
#endif
    }
}
