using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public class ClanPanel : UIWindow {

    private enum ClanTab { Join, Find, Create }
    private ClanTab activeTab = ClanTab.Join;

    [Header("Tab Buttons")]
    [SerializeField] private TabButton _joinTabButton;
    [SerializeField] private TabButton _findTabButton;
    [SerializeField] private TabButton _createTabButton;

    [Header("ScrollView Roots")]
    [SerializeField] private GameObject _joinContent;
    [SerializeField] private GameObject _findContent;
    [SerializeField] private GameObject _createContent;

    private void Start()
    {
        ValidateInspectorRefs(); // Log ra đảm bảo không có SerializeField Object bị null
    }

    private void OnEnable()
    {
        RegisterButtons();
        //RegisterInfoPanelEvents(); // Làm phần này sau
        ShowTab(activeTab);
    }

    private void OnDisable()
    {
        // Hủy đăng ký info panel events khi panel tắt để tránh callback rác
        //UnregisterInfoPanelEvents(); // Làm phần này sau khi hoàn thiện hàm RegisterInfoPanelEvents();
    }

    private void OnDestroy()
    {
        //UnregisterInfoPanelEvents(); // Làm phần này sau khi hoàn thiện hàm RegisterInfoPanelEvents();
    }

    private void RegisterButtons()
    {
        _joinTabButton?.Initialize(OnJoinTabClicked);
        _findTabButton?.Initialize(OnFindTabClicked);
        _createTabButton?.Initialize(OnCreateTabClicked);
    }

    private void OnJoinTabClicked() => ShowTab(ClanTab.Join);
    private void OnFindTabClicked() => ShowTab(ClanTab.Find);
    private void OnCreateTabClicked() => ShowTab(ClanTab.Create);

    private void ShowTab(ClanTab tab)
    {
        _findContent.SetActive(tab == ClanTab.Find);
        _createContent.SetActive(tab == ClanTab.Create);
        _joinContent.SetActive(tab == ClanTab.Join);
    }
    // Hàm kiểm tra xem đã kéo ref đủ trong inspector chưa
    private void ValidateInspectorRefs()
    {
        if (_joinTabButton == null) Debug.LogWarning("[ClanPanel] _joinTabButton is not assigned.");
        if (_findTabButton == null) Debug.LogWarning("[ClanPanel] holeSkinTabButton is not assigned.");
        if (_createTabButton == null) Debug.LogWarning("[ClanPanel] mapThemeTabButton is not assigned.");
        if (_joinContent == null) Debug.LogWarning("[ClanPanel] itemScrollView is not assigned.");
        if (_findContent == null) Debug.LogWarning("[ClanPanel] holeSkinScrollView is not assigned.");
        if (_createContent == null) Debug.LogWarning("[ClanPanel] mapThemeScrollView is not assigned.");
        //if (bundleItemContent == null) Debug.LogWarning("[ClanPanel] bundleItemContent is not assigned.");
        //if (holeSkinItemContent == null) Debug.LogWarning("[ClanPanel] holeSkinItemContent is not assigned.");
        }
}
