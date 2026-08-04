using UnityEngine;
using VContainer;

/// <summary>
/// Quản lý vòng đời của level trong GameplayScene.
/// Load LevelDefinition từ LevelsDatabase và expose cho các system khác dùng.
/// Objects đã được designer đặt sẵn trong Scene — không spawn runtime.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Level Config")]
    [SerializeField] private LevelsDatabase levelsDatabase;

    // ── Public state ──────────────────────────────────────────────────────────

    /// <summary>LevelDefinition của level đang chạy hiện tại.</summary>
    public LevelDefinition CurrentLevelDefinition { get; private set; }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Load LevelDefinition theo index từ LevelsDatabase.
    /// Gọi từ GameplayController.Start() trước khi StartGameplay().
    /// </summary>
    public void LoadLevel(int levelIndex = 0)
    {
        if (levelsDatabase == null)
        {
            Debug.LogError("[LevelManager] LevelsDatabase chưa được gán trong Inspector!");
            return;
        }

        CurrentLevelDefinition = levelsDatabase.GetLevel(levelIndex);

        if (CurrentLevelDefinition == null)
        {
            Debug.LogError($"[LevelManager] Không tìm thấy level index {levelIndex} trong LevelsDatabase.");
            return;
        }

        Debug.Log($"[LevelManager] Loaded: {CurrentLevelDefinition.LevelName}");
    }
}
