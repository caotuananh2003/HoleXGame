using UnityEngine;

/// <summary>
/// Loại obstacle — Normal hoặc Bomb.
/// Bomb sẽ trigger game over khi swallow (trừ khi có BombShield active).
/// </summary>
public enum ObstacleType
{
    Normal,
    Bomb,
    Booster
}

/// <summary>
/// ScriptableObject định nghĩa gameplay data cho một loại obstacle.
/// Data-driven: Designer tạo asset cho từng loại (Tree, Car, House, Bomb...)
/// </summary>
[CreateAssetMenu(fileName = "ObstacleDefinition_", menuName = "Definition/Obstacle Definition")]
public class ObstacleDefinition : ScriptableObject
{
    [Header("Type")]
    [SerializeField] private ObstacleType obstacleType = ObstacleType.Normal;

    [Header("Gameplay")]
    [SerializeField] private int scoreValue = 1;

    [Tooltip("Prefab chứa model 3D của obstacle. Dùng để spawn trong level.")]
    [SerializeField] private GameObject obstaclePrefab;

    [Header("UI")]
    [SerializeField] private Sprite icon;

    public ObstacleType Type => obstacleType;
    public int ScoreValue => scoreValue;
    public GameObject ObstaclePrefab => obstaclePrefab;
    public Sprite Icon => icon;
}
