using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hiển thị tiến độ ăn obstacle: "Eaten / Target".
/// Gắn vào child GameObject của GameplayPanel.
/// Gọi Setup() một lần khi bắt đầu ván, IncrementEaten() mỗi lần ăn.
/// </summary>
public class ObstacleCounter : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image           progressFill;   // optional radial/linear fill

    private int eaten;
    private int target;

    // ── Public API ────────────────────────────────────────────────────────────

    public void Setup(int targetCount)
    {
        target = Mathf.Max(1, targetCount);
        eaten  = 0;
        Refresh();
    }

    public void IncrementEaten()
    {
        eaten = Mathf.Min(eaten + 1, target);
        Refresh();
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void Refresh()
    {
        if (countText != null)
            countText.text = $"{eaten} / {target}";

        if (progressFill != null)
            progressFill.fillAmount = target > 0 ? (float)eaten / target : 0f;
    }
}
