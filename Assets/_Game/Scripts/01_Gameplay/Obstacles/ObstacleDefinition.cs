using UnityEngine;

/// <summary>
/// Loại obstacle — Normal hoặc Bomb.
/// Bomb sẽ trigger game over khi swallow (trừ khi có BombShield active).
/// </summary>
public enum ObstacleType
{
    Normal,
    Bomb
}

/// <summary>
/// ScriptableObject định nghĩa gameplay data cho một loại obstacle.
/// Data-driven: Designer tạo asset cho từng loại (Tree, Car, House, Bomb...)
/// </summary>
[CreateAssetMenu(fileName = "ObstacleDefinition_", menuName = "Definition/Obstacle Definition")]
public class ObstacleDefinition : ScriptableObject
{
    [Header("Identification")]
    [SerializeField] private string id;
    [SerializeField] private string displayName;

    [Header("Type")]
    [SerializeField] private ObstacleType obstacleType = ObstacleType.Normal;

    [Header("Gameplay")]
    [SerializeField] private int scoreValue = 1;

    [Header("UI")]
    [SerializeField] private Sprite icon;

    public string Id => id;
    public string DisplayName => displayName;
    public ObstacleType Type => obstacleType;
    public int ScoreValue => scoreValue;
    public Sprite Icon => icon;

    private void OnValidate()
    {
        // Auto-generate ID từ file name nếu chưa có
        if (string.IsNullOrEmpty(id))
        {
            id = name.Replace("ObstacleDefinition_", "");
        }
    }
}
