using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    private void Awake()     { Instance = this; }
    private void OnDestroy() { if (Instance == this) Instance = null; }

    [Header("Level Config")]
    [SerializeField] private LevelsDatabase  levelsDatabase;
    [SerializeField] private LevelSpawner    levelSpawner;

    public LevelDefinition CurrentLevelDefinition { get; private set; }
    public int TotalLevels => levelsDatabase != null ? levelsDatabase.TotalLevels : 0;

    public void LoadAndSpawnLevel(int levelIndex)
    {
        if (!TryLoadDefinition(levelIndex)) return;
        levelSpawner.SpawnLevel(CurrentLevelDefinition);
    }

    public void CleanupLevel() => levelSpawner.Cleanup();

    public int GetNextLevelIndex(int currentIndex)
    {
        if (TotalLevels <= 0) { Debug.LogWarning("[LevelManager] TotalLevels = 0."); return 0; }
        return (currentIndex + 1) % TotalLevels;
    }

    private bool TryLoadDefinition(int levelIndex)
    {
        if (levelsDatabase == null) { Debug.LogError("[LevelManager] LevelsDatabase chưa được gán!"); return false; }

        int safeIndex = TotalLevels > 0 ? levelIndex % TotalLevels : 0;
        CurrentLevelDefinition = levelsDatabase.GetLevel(safeIndex);

        if (CurrentLevelDefinition == null)
        {
            Debug.LogError($"[LevelManager] Không tìm thấy level index {safeIndex}.");
            return false;
        }

        Debug.Log($"[LevelManager] Loaded: '{CurrentLevelDefinition.LevelName}' (index {safeIndex}).");
        return true;
    }
}
