using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Quản lý slide animation khi chuyển đổi giữa các panel trong ContentPanel.
/// Gắn vào ContentPanel GameObject.
/// Kéo 5 panel vào danh sách panels theo đúng thứ tự tab từ trái sang phải:
///   [0] ShopPanel  [1] CollectionPanel  [2] ContentMainmenuPanel  [3] ClanPanel  [4] RankPanel
/// </summary>
public class ContentNavigator : MonoBehaviour
{
    [SerializeField] private List<UIWindow> panels;

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.3f;
    [SerializeField] private Ease slideEase = Ease.OutCubic;

    // Chiều rộng ContentPanel — dùng để tính offset slide
    private float panelWidth;

    private int currentIndex = -1;
    private bool isAnimating;

    private void Awake()
    {
        // Ẩn tất cả panel, chưa mở gì cả
        foreach (UIWindow panel in panels)
        {
            if (panel != null)
                panel.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        // Đọc rect.width sau khi Canvas đã hoàn thành layout pass
        // Awake() thường trả về 0 vì layout chưa được tính
        Canvas.ForceUpdateCanvases();
        panelWidth = GetComponent<RectTransform>().rect.width;
    }

    /// <summary>
    /// Gọi bởi BottomNavigationController sau khi nó đã sẵn sàng.
    /// </summary>
    public void Init()
    {
        Canvas.ForceUpdateCanvases();
        panelWidth = GetComponent<RectTransform>().rect.width;
    }

    /// <summary>
    /// Chuyển sang panel theo index, có slide animation.
    /// Gọi bởi BottomNavigationController.
    /// </summary>
    public void NavigateTo(int targetIndex)
    {
        Debug.Log($"[ContentNavigator] NavigateTo({targetIndex}), currentIndex={currentIndex}, isAnimating={isAnimating}, panels.Count={panels?.Count}");

        if (isAnimating) { Debug.Log("[ContentNavigator] Blocked: isAnimating"); return; }
        if (targetIndex == currentIndex) { Debug.Log("[ContentNavigator] Blocked: same index"); return; }
        if (targetIndex < 0 || targetIndex >= panels.Count) { Debug.LogError($"[ContentNavigator] Blocked: index {targetIndex} out of range (count={panels.Count})"); return; }

        UIWindow incoming = panels[targetIndex];
        if (incoming == null) { Debug.LogError($"[ContentNavigator] Blocked: panels[{targetIndex}] is null"); return; }

        // Lần đầu tiên — mở thẳng không animate
        if (currentIndex < 0)
        {
            incoming.Open();
            currentIndex = targetIndex;
            return;
        }

        UIWindow outgoing = panels[currentIndex];

        // Xác định hướng slide
        float direction = targetIndex > currentIndex ? 1f : -1f;
        float width = panelWidth > 0 ? panelWidth : Screen.width;

        isAnimating = true;

        // Chuẩn bị incoming: đặt ra ngoài màn hình bên phải/trái
        RectTransform incomingRect = incoming.GetComponent<RectTransform>();
        incomingRect.anchoredPosition = new Vector2(width * direction, 0f);
        incoming.Open(); // SetActive(true), IsOpen = true

        // Slide outgoing ra ngoài
        RectTransform outgoingRect = outgoing.GetComponent<RectTransform>();
        outgoingRect.DOAnchorPosX(-width * direction, slideDuration)
            .SetEase(slideEase);

        // Slide incoming vào giữa
        incomingRect.DOAnchorPosX(0f, slideDuration)
            .SetEase(slideEase)
            .OnComplete(() =>
            {
                outgoing.Close(); // SetActive(false)
                // Reset vị trí outgoing để lần sau dùng lại đúng
                outgoingRect.anchoredPosition = Vector2.zero;
                isAnimating = false;
            });

        currentIndex = targetIndex;
        Debug.Log("ContentNavigator.NavigateTo: " + targetIndex);
    }
}
