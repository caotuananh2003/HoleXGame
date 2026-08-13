using UnityEngine;
using VContainer;

/// <summary>
/// Quản lý vòng đời của level trong GameplayScene.
///
/// Trách nhiệm:
///   1. Load LevelDefinition từ LevelsDatabase theo index.
///   2. Điều phối LevelSpawner: spawn prefab khi bắt đầu, cleanup khi kết thúc.
///   3. Tính index level tiếp theo (mod % để quay vòng khi vượt quá danh sách).
///
/// GameplayController chỉ cần gọi:
///   - LoadAndSpawnLevel(index)  → load data + spawn prefab
///   - CleanupLevel()            → destroy prefab hiện tại
///   - GetNextLevelIndex(current)→ tính index kế tiếp, tự quay vòng
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Level Config")]
    [SerializeField] private LevelsDatabase levelsDatabase;

    // ── Dependencies ──────────────────────────────────────────────────────────
    private LevelSpawner levelSpawner;

    [Inject]
    private void Construct(LevelSpawner levelSpawner)
    {
        this.levelSpawner = levelSpawner;
    }

    // ── Public state ──────────────────────────────────────────────────────────

    /// <summary>LevelDefinition của level đang chạy hiện tại.</summary>
    public LevelDefinition CurrentLevelDefinition { get; private set; }

    /// <summary>Tổng số level trong database. 0 nếu chưa gán database.</summary>
    public int TotalLevels => levelsDatabase != null ? levelsDatabase.TotalLevels : 0;

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>
    /// Load LevelDefinition theo index rồi spawn level prefab.
    /// Gọi từ GameplayController.Start().
    /// </summary>
    public void LoadAndSpawnLevel(int levelIndex)
    {
        if (!TryLoadDefinition(levelIndex)) return;

        levelSpawner.SpawnLevel(CurrentLevelDefinition);
    }

    /// <summary>
    /// Destroy level prefab hiện tại.
    /// Gọi khi kết thúc level, restart, hoặc về MainMenu.
    /// </summary>
    public void CleanupLevel()
    {
        levelSpawner.Cleanup();
    }

    /// <summary>
    /// Tính index level tiếp theo.
    /// Dùng mod % để quay vòng về 0 khi vượt quá tổng số level.
    /// Ví dụ: current=4, total=5 → next=0 (quay vòng).
    /// </summary>
    public int GetNextLevelIndex(int currentIndex)
    {
        if (TotalLevels <= 0)
        {
            Debug.LogWarning("[LevelManager] TotalLevels = 0 — không thể tính next level.");
            return 0;
        }

        return (currentIndex + 1) % TotalLevels;
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private bool TryLoadDefinition(int levelIndex)
    {
        if (levelsDatabase == null)
        {
            Debug.LogError("[LevelManager] LevelsDatabase chưa được gán trong Inspector!");
            return false;
        }

        // Clamp an toàn: nếu index lệch do save cũ thì mod về range hợp lệ
        int safeIndex = TotalLevels > 0 ? levelIndex % TotalLevels : 0;

        CurrentLevelDefinition = levelsDatabase.GetLevel(safeIndex);

        if (CurrentLevelDefinition == null)
        {
            Debug.LogError($"[LevelManager] Không tìm thấy level index {safeIndex} trong LevelsDatabase.");
            return false;
        }

        Debug.Log($"[LevelManager] Loaded: '{CurrentLevelDefinition.LevelName}' (index {safeIndex}).");
        return true;
    }
}
