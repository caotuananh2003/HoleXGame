using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI item hiển thị 1 objective: icon + progress text (3/5).
/// Được spawn runtime từ GameplayPanel dựa trên danh sách objectives.
/// </summary>
public class ObjectiveUIItem : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI progressText;

    private LevelObjective objective;

    public void Initialize(LevelObjective objective)
    {
        this.objective = objective;

        if (objective?.ObstacleDefinition != null)
        {
            // Set icon
            if (iconImage != null && objective.ObstacleDefinition.Icon != null)
            {
                iconImage.sprite = objective.ObstacleDefinition.Icon;
            }

            // Set text ban đầu
            UpdateProgress();
        }
    }

    public void UpdateProgress()
    {
        if (objective == null || progressText == null) return;

        progressText.text = $"{objective.CurrentCount}/{objective.RequiredCount}";

        // Optional: Đổi màu khi complete
        if (objective.IsCompleted)
        {
            progressText.color = Color.green;
        }
    }
}

