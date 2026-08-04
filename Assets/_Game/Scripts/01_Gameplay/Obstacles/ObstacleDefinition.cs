using UnityEngine;

/// <summary>
/// ScriptableObject định nghĩa gameplay data cho một loại obstacle.
/// Data-driven: Designer tạo asset cho từng loại (Tree, Car, House...)
/// </summary>
[CreateAssetMenu(fileName = "ObstacleDefinition_", menuName = "Definition/Obstacle Definition")]
public class ObstacleDefinition : ScriptableObject
{
    [Header("Identification")]
    [SerializeField] private string obstacleID;
    [SerializeField] private string displayName;

    [Header("Gameplay")]
    [SerializeField] private int scoreValue = 1;

    [Header("UI")]
    [SerializeField] private Sprite icon;

    public string ObstacleID => obstacleID;
    public string DisplayName => displayName;
    public int ScoreValue => scoreValue;
    public Sprite Icon => icon;

    private void OnValidate()
    {
        // Auto-generate ID từ file name nếu chưa có
        if (string.IsNullOrEmpty(obstacleID))
        {
            obstacleID = name.Replace("ObstacleDefinition_", "");
        }
    }
}
