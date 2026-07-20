using TMPro;
using UnityEngine;

/// <summary>
/// HUD hiển thị score và timer trong gameplay.
/// Mode = Persistent — luôn bật trong suốt gameplay.
/// Wire HoleSizeController và GameTimer vào Inspector.
/// Score cập nhật qua event, không polling Update.
/// </summary>
public class GameplayHUD : UIWindow
{
    [Header("References")]
    [SerializeField] private HoleSizeController sizeController;
    [SerializeField] private HoleController     holeController;
    [SerializeField] private GameTimer          gameTimer;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        if (gameTimer != null)
            gameTimer.OnTick += OnTick;

        if (sizeController != null)
            sizeController.OnScoreAdded += OnScoreAdded;

        UpdateScore(0);
    }

    private void OnDestroy()
    {
        if (gameTimer != null)
            gameTimer.OnTick -= OnTick;

        if (sizeController != null)
            sizeController.OnScoreAdded -= OnScoreAdded;
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void OnScoreAdded(int delta)
    {
        // Lấy tổng điểm từ HoleController (source of truth)
        if (holeController != null)
            UpdateScore(holeController.Score);
    }

    private void OnTick(float remaining)
    {
        if (timerText == null) return;

        if (remaining <= 0f)
        {
            timerText.text = "00:00";
            return;
        }

        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining - minutes * 60f);
        timerText.text = $"{minutes:0}:{seconds:00}";
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }
}
