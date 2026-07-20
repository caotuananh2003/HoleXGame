using System.Collections.Generic;
using UnityEngine;
using VContainer;

/// <summary>
/// Quản lý vòng đời của level trong GameplayScene:
///   Initialize → spawn objects → expose map info → cleanup khi restart.
///
/// Gắn vào một GameObject trong GameplayScene.
/// Đăng ký vào GameplayLifetimeScope.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Level Config")]
    [Tooltip("Danh sách LevelData theo thứ tự. Index = level number (0-based).")]
    [SerializeField] private LevelData[] levels;

    // ── Public state ──────────────────────────────────────────────────────────

    /// <summary>Bounds XZ của map hiện tại.</summary>
    public Bounds MapBounds { get; private set; }

    /// <summary>Tổng số object đã spawn (dùng cho ObstacleCounter target).</summary>
    public int TotalSpawnedCount => spawnedObjects.Count;

    // ── Private ───────────────────────────────────────────────────────────────

    private ObjectPoolService poolService;
    private LevelLoader       loader;
    private LevelSpawner      spawner;
    private List<GameObject>  spawnedObjects = new();

    // ── DI ────────────────────────────────────────────────────────────────────

    [Inject]
    private void Construct(ObjectPoolService poolService)
    {
        this.poolService = poolService;
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        loader  = new LevelLoader(levels);
        spawner = new LevelSpawner(poolService);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Load và spawn level theo index.
    /// Gọi từ GameplayController.Start() trước khi StartGameplay().
    /// </summary>
    public void LoadLevel(int levelIndex = 0)
    {
        LevelData data = loader.Load(levelIndex);
        if (data == null)
        {
            Debug.LogError($"[LevelManager] Cannot load level {levelIndex}.");
            return;
        }

        MapBounds      = data.GetMapBounds();
        spawnedObjects = spawner.Spawn(data);

        Debug.Log($"[LevelManager] Level {levelIndex} loaded. Spawned {spawnedObjects.Count} objects.");
    }

    /// <summary>
    /// Trả tất cả spawned objects về pool (gọi khi restart hoặc gameover).
    /// </summary>
    public void Cleanup()
    {
        poolService?.ReturnAll();
        spawnedObjects.Clear();
    }
}
