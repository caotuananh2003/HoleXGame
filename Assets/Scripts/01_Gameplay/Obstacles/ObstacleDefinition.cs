using UnityEngine;

public enum ObstacleType
{
    Normal,
    Bomb,
    Booster
}

/// <summary>
/// Loại booster — chỉ dùng khi ObstacleType = Booster.
/// </summary>
public enum BoosterType
{
    Energy, // Cộng ngay một lượng score
    Clock,  // Cộng ngay một lượng thời gian
    Speed,  // Tăng tốc độ di chuyển trong một khoảng thời gian
}

/// <summary>
/// ScriptableObject định nghĩa gameplay data cho một loại obstacle.
/// Data-driven: Designer tạo asset cho từng loại (Tree, Car, House, Bomb, Booster...)
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

    // ── Booster config — chỉ có tác dụng khi obstacleType = Booster ──────────

    [Header("Booster (chỉ dùng khi Type = Booster)")]
    [SerializeField] private BoosterType boosterType = BoosterType.Energy;

    [Tooltip("Energy: lượng score cộng thêm.\n" +
             "Clock: số giây cộng thêm.\n" +
             "Speed: hệ số nhân tốc độ (ví dụ 1.5 = tăng 50%).")]
    [SerializeField] private float boosterValue = 10f;

    [Tooltip("Speed booster: thời gian hiệu lực (giây). Không dùng cho Energy và Clock.")]
    [SerializeField] private float boosterDuration = 5f;

    // ── Properties ────────────────────────────────────────────────────────────

    public ObstacleType Type         => obstacleType;
    public int          ScoreValue   => scoreValue;
    public GameObject   ObstaclePrefab => obstaclePrefab;
    public Sprite       Icon         => icon;

    public BoosterType  BoosterType     => boosterType;
    public float        BoosterValue    => boosterValue;
    public float        BoosterDuration => boosterDuration;
}
