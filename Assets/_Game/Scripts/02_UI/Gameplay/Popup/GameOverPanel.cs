using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// Panel kết quả sau khi hết giờ.
/// Mode = Popup — đè lên HUD.
/// Wire GameplayController vào Inspector.
/// </summary>
public class GameOverPanel : UIWindow
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private GameObject newBestBadge;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("References")]
    [SerializeField] private GameplayController gameplayController;

    private void Start()
    {
        //if (restartButton != null)
        //    restartButton.onClick.AddListener(() => gameplayController?.RestartAsync());

        //if (mainMenuButton != null)
        //    mainMenuButton.onClick.AddListener(() => gameplayController?.GoToMainMenuAsync());
    }

    private void OnDestroy()
    {
        restartButton?.onClick.RemoveAllListeners();
        mainMenuButton?.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Gọi từ GameplayController trước khi Open().
    /// </summary>
    public void Setup(int finalScore, int bestScore)
    {
        if (finalScoreText != null)
            finalScoreText.text = finalScore.ToString();

        bool isNewBest = finalScore > bestScore;

        if (bestScoreText != null)
            bestScoreText.text = isNewBest ? finalScore.ToString() : bestScore.ToString();

        if (newBestBadge != null)
            newBestBadge.SetActive(isNewBest);
    }
}
