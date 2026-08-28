using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manager chịu trách nhiệm kiểm soát các object cần ăn trong 1 level trong GameplayScene:
/// 
///   - Subscribe SwallowHandler event
///   - Cập nhật tiến độ từng objective khi obstacle bị nuốt
///   - Fire event khi objective complete
///   - Fire event khi tất cả objectives complete (Win)
/// 
/// Managed by VContainer, injected vào GameplayController.
/// </summary>
public class GameplayObjectiveManager : MonoBehaviour
{
    public static GameplayObjectiveManager Instance { get; private set; }

    private void Awake()
    {
        Instance      = this;
        swallowHandler = FindAnyObjectByType<SwallowHandler>();
    }

    private void OnDestroy() { if (Instance == this) Instance = null; }
    public event Action<LevelObjective> OnObjectiveUpdated;
    public event Action<LevelObjective> OnObjectiveCompleted;
    public event Action OnAllObjectivesCompleted;

    private LevelDefinition currentLevelDefinition;
    private SwallowHandler swallowHandler;

    private List<LevelObjective> activeObjectives = new List<LevelObjective>();

    public bool IsAllCompleted => activeObjectives.All(obj => obj.IsCompleted);
    /// <summary>
    /// Khởi tạo objectives cho level hiện tại.
    /// Gọi từ GameplayController khi bắt đầu level.
    /// </summary>
    public void InitializeLevel(LevelDefinition levelDefinition)
    {
        if (levelDefinition == null)
        {
            Debug.LogError("[GameplayObjectiveManager] LevelDefinition is null!");
            return;
        }

        currentLevelDefinition = levelDefinition;

        // Reset tất cả objectives
        activeObjectives.Clear();
        foreach (var levelObjective in currentLevelDefinition.LevelObjectives)
        {
            levelObjective.Reset();
            activeObjectives.Add(levelObjective);
        }

        // Subscribe event
        swallowHandler.OnObjectSwallowed += OnObstacleSwallowed;

        Debug.Log($"[GameplayObjectiveManager] Initialized {activeObjectives.Count} objectives for {levelDefinition.LevelName}");
    }

    /// <summary>
    /// Cleanup khi kết thúc level hoặc destroy manager.
    /// </summary>
    public void Cleanup()
    {
        if (swallowHandler != null)
        {
            swallowHandler.OnObjectSwallowed -= OnObstacleSwallowed;
        }

        activeObjectives.Clear();
        allCompletedFired = false;
        currentLevelDefinition = null;
    }

    private bool allCompletedFired = false;

    private void OnObstacleSwallowed(Obstacle obstacle)
    {
        // Tìm objective matching với obstacle type
        foreach (var objective in activeObjectives) // Duyệt qua từng LevelObjective trong danh sách
        {
            if (objective.ObstacleDefinition == obstacle.ObstacleDefinition) // Nếu cái đang duyệt chính là cái bị nuốt và fire từ Event thì xử lý
            {
                bool wasCompleted = objective.IsCompleted; // Lấy ra xem obstacle vừa nuốt đã hoàn thành chưa?

                // Tăng count
                objective.CurrentCount++; // Cập nhật cho levelObjective đó biết rằng "vừa swallow thêm 1"

                Debug.Log($"[Objective] {objective.ObstacleDefinition.DisplayName}: {objective.CurrentCount}/{objective.RequiredCount}");

                // Fire event update
                OnObjectiveUpdated?.Invoke(objective);

                // Ở bên trên, ta lấy trạng thái wasCompleted trước rồi sau đó mới cập nhật biến đếm cho objective là để phục vụ
                // cho việc Fire cái OnObjectiveCompleted khi vừa mới chuyển từ trạng thái objective.IsCompleted = false sang true

                // Nếu Fire event OnObjectiveCompleted ngay khi objective.IsCompleted = true, thì trong trường hợp
                // swallow "dư" object ra thì mỗi lần swallow sẽ phát event 1 lần mất.

                // Fire complete event chỉ khi vừa chuyển từ chưa complete → complete
                if (!wasCompleted && objective.IsCompleted)
                {
                    Debug.Log($"[Objective] Completed: {objective.ObstacleDefinition.DisplayName}");
                    OnObjectiveCompleted?.Invoke(objective);
                }

                break; // Mỗi obstacle chỉ match 1 objective, đỡ phải duyệt hết list
            }
        }

        // Kiểm tra tất cả objectives đã complete chưa
        if (!allCompletedFired && IsAllCompleted)
        {
            allCompletedFired = true;
            Debug.Log("[GameplayObjectiveManager] All objectives completed! Level Win!");
            OnAllObjectivesCompleted?.Invoke();
        }
    }

    public List<LevelObjective> GetActiveObjectives() => activeObjectives;
}
