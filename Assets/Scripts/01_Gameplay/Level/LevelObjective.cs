using System;
using UnityEngine;


/// <summary>
/// Một objective trong level. Ví dụ: "Ăn 5 Tree", "Ăn 2 Car".
/// Serializable để lưu trong LevelDefinition.
/// </summary>
[Serializable]
public class LevelObjective
{
    [SerializeField] private ObstacleDefinition obstacleDefinition;
    [SerializeField] private int requiredCount;

    public ObstacleDefinition ObstacleDefinition => obstacleDefinition;
    public int RequiredCount => requiredCount;

    public int CurrentCount { get; set; } // Số lượng hiện tại đã được swallow (Cần reset mỗi khi vào màn mới)

    public bool IsCompleted => CurrentCount >= RequiredCount;

    public float Progress => requiredCount > 0 ? (float)CurrentCount / requiredCount : 0f;

    public void Reset()
    {
        CurrentCount = 0;
    }
}
