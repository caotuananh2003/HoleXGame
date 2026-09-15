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
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;

    private HoleController holeController;
    private GameTimer gameTimer;
    private void Awake()
    {
        holeController = FindAnyObjectByType<HoleController>();
        gameTimer = FindAnyObjectByType<GameTimer>();
    }
    private void Start()
    {
        if (gameTimer != null)
            gameTimer.OnTick += OnTick;

        if (holeController != null)
            holeController.OnProgressChanged += OnProgressChanged;

        UpdateScore(0);
    }

    private void OnProgressChanged(int level, float progress)
    {
        if (holeController != null)
            UpdateScore(holeController.Score);
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
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

    private void OnDestroy()
    {
        if (gameTimer != null)
            gameTimer.OnTick -= OnTick;

        if (holeController != null)
            holeController.OnProgressChanged -= OnProgressChanged;    }
}
