using System.Collections.Generic;
using UnityEngine;

    /// <summary>
    /// ScriptableObject định nghĩa một level.
    /// LevelPrefab chứa toàn bộ map + obstacle đã được designer xếp sẵn.
    /// LevelSpawner sẽ Instantiate prefab này vào LevelRoot khi bắt đầu level.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelDefinition_", menuName = "Definition/Level Definition")]
    public class LevelDefinition : ScriptableObject
    {
        [Header("Level Info")]
        [SerializeField] private int levelIndex;
        [SerializeField] private string levelName;

        [Header("Level Prefab")]
        [Tooltip("Prefab chứa Map + Obstacles đã được xếp sẵn bởi designer.\nLevelSpawner sẽ Instantiate prefab này tại LevelRoot.")]
        [SerializeField] private GameObject levelPrefab;

        [Header("Gameplay Config")]
        [SerializeField] private float timeLimit = 120f;

        [Tooltip("Lượng currency thưởng khi hoàn thành level.")]
        [SerializeField] private int currencyReward = 50;

        [Header("Objectives")]
        [SerializeField] private List<LevelObjective> levelObjectives = new List<LevelObjective>();

        public int LevelIndex => levelIndex;
        public string LevelName => levelName;
        public GameObject LevelPrefab => levelPrefab;
        public float TimeLimit => timeLimit;
        public int CurrencyReward => currencyReward;
        public List<LevelObjective> LevelObjectives => levelObjectives;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(levelName))
                levelName = name.Replace("LevelDefinition_", "Level ");

            if (levelPrefab == null)
                Debug.LogWarning($"[LevelDefinition] '{name}' chưa gán LevelPrefab!", this);
        }
    }
