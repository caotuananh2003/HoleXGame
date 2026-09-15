using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RankPanel : UIWindow
{

    private enum RankTab { Weekly, Player, Clan }
    private RankTab activeTab = RankTab.Weekly;

    [Header("Tab Buttons")]
    [SerializeField] private TabButton _weeklyTabButton;
    [SerializeField] private TabButton _playerTabButton;
    [SerializeField] private TabButton _clanTabButton;

    [Header("ScrollView Roots")]
    [SerializeField] private GameObject _weeklyContent;
    [SerializeField] private GameObject _playerContent;
    [SerializeField] private GameObject _clanContent;

    private bool _buttonRegistered = false;
    private void Start()
    {
        RegisterButtons();
        ValidateInspectorRefs(); // Log ra đảm bảo không có SerializeField Object bị null
        ShowTab(activeTab);
    }

    private void OnEnable()
    {
        if (_buttonRegistered)
        {
            ShowTab(activeTab);
        }
    }

    private void RegisterButtons()
    {
        _weeklyTabButton?.Initialize(OnWeeklyTabClicked);
        _playerTabButton?.Initialize(OnPlayerTabClicked);
        _clanTabButton?.Initialize(OnClanTabClicked);
        _buttonRegistered = true;
    }

    private void OnWeeklyTabClicked() => ShowTab(RankTab.Weekly);
    private void OnPlayerTabClicked() => ShowTab(RankTab.Player);
    private void OnClanTabClicked() => ShowTab(RankTab.Clan);

    private void ShowTab(RankTab tab)
    {
        activeTab = tab;
        _playerContent.SetActive(tab == RankTab.Player);
        _clanContent.SetActive(tab == RankTab.Clan);
        _weeklyContent.SetActive(tab == RankTab.Weekly);

        _playerTabButton?.SetSelected(tab == RankTab.Player);
        _clanTabButton?.SetSelected(tab == RankTab.Clan);
        _weeklyTabButton?.SetSelected(tab == RankTab.Weekly);
    }

    // Hàm kiểm tra xem đã kéo ref đủ trong inspector chưa
    private void ValidateInspectorRefs()
    {
        if (_weeklyTabButton == null) Debug.LogWarning("[RankPanel] _weeklyTabButton is not assigned.");
        if (_playerTabButton == null) Debug.LogWarning("[RankPanel] _playerTabButton is not assigned.");
        if (_clanTabButton == null) Debug.LogWarning("[RankPanel] _clanTabButton is not assigned.");
        if (_weeklyContent == null) Debug.LogWarning("[RankPanel] _weeklyContent is not assigned.");
        if (_playerContent == null) Debug.LogWarning("[RankPanel] _playerContent is not assigned.");
        if (_clanContent == null) Debug.LogWarning("[RankPanel] _clanContent is not assigned.");
        //if (bundleItemContent == null) Debug.LogWarning("[ClanPanel] bundleItemContent is not assigned.");
        //if (holeSkinItemContent == null) Debug.LogWarning("[ClanPanel] holeSkinItemContent is not assigned.");
    }
}
