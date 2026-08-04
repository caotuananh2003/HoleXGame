using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Gắn vào BottomPanel GameObject cùng với BottomNavigationController.
/// Mode = Persistent: UIManager sẽ không bao giờ đóng panel này.
/// Đặt Mode = Persistent trong Inspector.
/// </summary>
public class BottomPanel : UIWindow
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
