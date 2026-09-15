using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Manager chịu trách nhiệm kiểm soát các object cần ăn trong 1 level trong GameplayScene:
///
///   - Subscribe KillerCollider.OnObjectSwallowed
///   - Cập nhật tiến độ từng objective khi obstacle bị nuốt
///   - Fire event khi objective complete
///   - Fire event khi tất cả objectives complete (Win)
///
/// Animation tracking:
///   GameplayPanel gọi RegisterAnimationStarted() khi bắt đầu fly,
///   RegisterAnimationCompleted() khi onArrived callback chạy xong.
///   GameplayController await WaitForAllAnimationsAsync() trước khi mở GameWinPopup.
/// </summary>
public class GameplayObjectiveManager : MonoBehaviour
{
    public static GameplayObjectiveManager Instance { get; private set; }

    private void Awake() { Instance = this; }
    private void OnDestroy() { if (Instance == this) Instance = null; }

    public event Action<LevelObjective> OnObjectiveUpdated;
    public event Action<LevelObjective> OnObjectiveCompleted;
    public event Action                 OnAllObjectivesCompleted;

    [SerializeField] private KillerCollider killerCollider;

    private LevelDefinition      currentLevelDefinition;
    private List<LevelObjective> activeObjectives  = new();
    private bool                 allCompletedFired = false;

    // ── Animation tracking ────────────────────────────────────────────────────

    private int _pendingAnimations = 0;

    /// <summary>Gọi từ GameplayPanel ngay khi Fly() bắt đầu.</summary>
    public void RegisterAnimationStarted()   => _pendingAnimations++;

    /// <summary>Gọi từ GameplayPanel trong onArrived callback (sau khi icon đến nơi).</summary>
    public void RegisterAnimationCompleted() => _pendingAnimations = Mathf.Max(0, _pendingAnimations - 1);

    /// <summary>
    /// Await cho đến khi tất cả fly animation hoàn tất.
    /// GameplayController gọi trước khi mở GameWinPopup.
    /// </summary>
    public async UniTask WaitForAllAnimationsAsync()
    {
        while (_pendingAnimations > 0)
            await UniTask.Yield();
    }

    // ── Public API ────────────────────────────────────────────────────────────

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
        _pendingAnimations     = 0;
        allCompletedFired      = false;

        activeObjectives.Clear();
        foreach (var levelObjective in currentLevelDefinition.LevelObjectives)
        {
            levelObjective.Reset();
            activeObjectives.Add(levelObjective);
        }

        // -= trước để tránh duplicate subscribe khi InitializeLevel gọi nhiều lần
        killerCollider.OnObjectSwallowed -= OnObstacleSwallowed;
        killerCollider.OnObjectSwallowed += OnObstacleSwallowed;

        Debug.Log($"[GameplayObjectiveManager] Initialized {activeObjectives.Count} objectives for {levelDefinition.LevelName}");
    }

    /// <summary>Cleanup khi kết thúc level hoặc destroy manager.</summary>
    public void Cleanup()
    {
        if (killerCollider != null)
            killerCollider.OnObjectSwallowed -= OnObstacleSwallowed;

        activeObjectives.Clear();
        allCompletedFired  = false;
        _pendingAnimations = 0;
        currentLevelDefinition = null;
    }

    public List<LevelObjective> GetActiveObjectives() => activeObjectives;

    // ── Internal ──────────────────────────────────────────────────────────────

    private void OnObstacleSwallowed(Obstacle obstacle)
    {
        foreach (var objective in activeObjectives)
        {
            if (objective.ObstacleDefinition != obstacle.ObstacleDefinition) continue;

            bool wasCompleted = objective.IsCompleted;

            objective.CurrentCount++;

            Debug.Log($"[Objective] {objective.ObstacleDefinition.name}: {objective.CurrentCount}/{objective.RequiredCount}");

            OnObjectiveUpdated?.Invoke(objective);

            if (!wasCompleted && objective.IsCompleted)
            {
                Debug.Log($"[Objective] Completed: {objective.ObstacleDefinition.name}");
                OnObjectiveCompleted?.Invoke(objective);
            }

            break;
        }

        if (!allCompletedFired && IsAllCompleted)
        {
            allCompletedFired = true;
            Debug.Log("[GameplayObjectiveManager] All objectives completed! Level Win!");
            OnAllObjectivesCompleted?.Invoke();
        }
    }
}
