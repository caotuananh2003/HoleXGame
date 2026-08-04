using UnityEngine;

/// <summary>
/// Component gắn vào mỗi prefab obstacle.
/// Chỉ chứa reference tới ObstacleDefinition, không chứa gameplay logic.
/// Single Responsibility: Chỉ là data holder.
/// </summary>
public class Obstacle : MonoBehaviour
{
    [SerializeField] private ObstacleDefinition obstacleDefinition;

    public ObstacleDefinition ObstacleDefinition => obstacleDefinition;

    private void OnValidate()
    {
        if (obstacleDefinition == null)
        {
            Debug.LogWarning($"[Obstacle] {gameObject.name} chưa gán ObstacleDefinition!", this);
        }
    }
}
