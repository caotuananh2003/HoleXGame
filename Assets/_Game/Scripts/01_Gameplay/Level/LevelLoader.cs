using UnityEngine;

/// <summary>
/// Pure helper class — resolve LevelData từ array hoặc Resources folder.
/// Không kế thừa MonoBehaviour.
/// LevelManager dùng class này để load LevelData theo level index.
/// </summary>
public class LevelLoader
{
    private readonly LevelData[] levels;
    private readonly string      resourcesPath;

    /// <param name="levels">Array LevelData assign qua Inspector của LevelManager.</param>
    /// <param name="resourcesPath">Fallback path trong Resources nếu array rỗng.</param>
    public LevelLoader(LevelData[] levels, string resourcesPath = "Levels/Level_{0}")
    {
        this.levels        = levels;
        this.resourcesPath = resourcesPath;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Load LevelData theo index (0-based).
    /// Ưu tiên array trước, fallback về Resources.
    /// </summary>
    public LevelData Load(int levelIndex)
    {
        // 1. Từ array (ưu tiên)
        if (levels != null && levels.Length > 0)
        {
            int clamped = Mathf.Clamp(levelIndex, 0, levels.Length - 1);
            if (levels[clamped] != null)
                return levels[clamped];
        }

        // 2. Fallback: Resources
        string path  = string.Format(resourcesPath, levelIndex + 1);
        LevelData res = Resources.Load<LevelData>(path);

        if (res == null)
            Debug.LogWarning($"[LevelLoader] LevelData not found at index {levelIndex} or Resources path '{path}'.");

        return res;
    }

    /// <summary>Số level có trong array.</summary>
    public int Count => levels != null ? levels.Length : 0;
}
