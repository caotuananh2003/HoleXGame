using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject quản lý toàn bộ LevelDefinition.
/// Tránh phải kéo hàng trăm level vào Inspector từng cái.
/// Single source of truth cho level data.
/// </summary>
[CreateAssetMenu(fileName = "LevelsDatabase", menuName = "Database/Levels Database")]
public class LevelsDatabase : ScriptableObject
{
    [SerializeField] private List<LevelDefinition> levels = new List<LevelDefinition>();

    public int TotalLevels => levels?.Count ?? 0;

    public LevelDefinition GetLevel(int index)
    {
        if (levels == null || index < 0 || index >= levels.Count)
        {
            Debug.LogWarning($"[LevelsDatabase] Invalid level index: {index}");
            return null;
        }

        return levels[index];
    }

    public List<LevelDefinition> GetAllLevels() => levels;
}