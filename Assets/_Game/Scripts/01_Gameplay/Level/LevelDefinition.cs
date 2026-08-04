using System.Collections.Generic;
using UnityEngine;

    /// <summary>
    /// ScriptableObject định nghĩa một level.
    /// Không lưu danh sách object để spawn (vì object đã tồn tại sẵn trong Scene).
    /// Chỉ lưu objectives và gameplay config.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelDefinition_", menuName = "Definition/Level Definition")]
    public class LevelDefinition : ScriptableObject
    {
        [Header("Level Info")]
        [SerializeField] private int levelIndex;
        [SerializeField] private string levelName;

        [Header("Gameplay Config")]
        [SerializeField] private float timeLimit = 60f;

        [Header("Objectives")]
        [SerializeField] private List<LevelObjective> levelObjectives = new List<LevelObjective>();

        public int LevelIndex => levelIndex;
        public string LevelName => levelName;
        public float TimeLimit => timeLimit;
        public List<LevelObjective> LevelObjectives => levelObjectives;

        private void OnValidate()
        {
            // Auto-generate level name từ file name nếu chưa có
            if (string.IsNullOrEmpty(levelName))
            {
                levelName = name.Replace("LevelDefinition_", "Level ");
            }
        }
    }
