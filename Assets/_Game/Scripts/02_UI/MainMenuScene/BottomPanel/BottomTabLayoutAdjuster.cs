using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Điều chỉnh flexibleWidth của LayoutElement trên các TabButton của BottomPanel.
///
/// Gắn vào BottomPanel (cùng GameObject với BottomPanel script).
/// BottomPanel gọi NotifyTabSelected(index) mỗi khi tab thay đổi.
///
/// Mỗi TabButton phải có LayoutElement gắn sẵn.
/// unselectedFlexibleWidth = 2 (mặc định)
/// selectedFlexibleWidth   = 3 (khi selected)
/// </summary>
public class BottomTabLayoutAdjuster : MonoBehaviour
{
    [Header("Tab LayoutElements")]
    [SerializeField] private List<LayoutElement> tabLayoutElements;

    [Header("Layout Settings")]
    [SerializeField] private float unselectedFlexibleWidth = 2f;
    [SerializeField] private float selectedFlexibleWidth   = 3f;

    // =========================================================================
    // Public API — gọi từ BottomPanel
    // =========================================================================

    /// <summary>
    /// Cập nhật flexibleWidth cho tất cả LayoutElement theo tab vừa được chọn.
    /// </summary>
    public void NotifyTabSelected(int selectedIndex)
    {
        if (tabLayoutElements == null) return;

        for (int i = 0; i < tabLayoutElements.Count; i++)
        {
            if (tabLayoutElements[i] == null)
            {
                Debug.LogWarning($"[BottomTabLayoutAdjuster] tabLayoutElements[{i}] is null.");
                continue;
            }

            tabLayoutElements[i].flexibleWidth = (i == selectedIndex)
                ? selectedFlexibleWidth
                : unselectedFlexibleWidth;
        }
    }
}
