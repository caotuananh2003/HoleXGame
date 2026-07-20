using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gắn vào BottomPanel. Quản lý tab selection và điều hướng ContentNavigator.
/// Kéo các TabButton vào danh sách tabs theo đúng thứ tự khớp với panels trong ContentNavigator:
///   [0] Shop  [1] Collection  [2] Mainmenu  [3] Clan  [4] Rank
/// Kéo ContentNavigator vào field contentNavigator.
/// defaultTabIndex = 2 để mở MainmenuPanel đầu tiên.
/// </summary>
public class BottomNavigationController : MonoBehaviour
{
    [SerializeField] private ContentNavigator contentNavigator;
    [SerializeField] private List<TabButton> tabs;
    [SerializeField] private int defaultTabIndex = 2;

    private int currentTabIndex = -1;

    private void Start()
    {
        if (contentNavigator == null)
        {
            Debug.LogError("[BottomNavigationController] contentNavigator is not assigned in Inspector!");
            return;
        }

        if (tabs == null || tabs.Count == 0)
        {
            Debug.LogError("[BottomNavigationController] tabs list is empty!");
            return;
        }

        // Init ContentNavigator trước để đảm bảo panelWidth đã được tính
        contentNavigator.Init();

        for (int i = 0; i < tabs.Count; i++)
        {
            if (tabs[i] == null)
            {
                Debug.LogError($"[BottomNavigationController] tabs[{i}] is null!");
                continue;
            }
            int index = i; // capture for closure
            tabs[i].Initialize(() => SelectTab(index));
        }

        SelectTab(defaultTabIndex);
    }

    private void SelectTab(int index)
    {
        Debug.Log($"[BottomNavigationController] SelectTab({index}), currentTabIndex={currentTabIndex}");
        if (index == currentTabIndex) return;

        // Cập nhật visual state của các tab
        for (int i = 0; i < tabs.Count; i++)
            tabs[i].SetSelected(i == index);

        currentTabIndex = index;
        contentNavigator.NavigateTo(index);
    }
}
