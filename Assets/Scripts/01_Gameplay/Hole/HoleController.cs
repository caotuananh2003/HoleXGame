using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Entry point của player. Điều phối HoleMovement và HoleSizeController.
/// Gắn vào root GameObject của Player trong GameplayScene.
///
/// Tất cả config kéo vào đây, truyền xuống các component qua Init().
/// </summary>
[RequireComponent(typeof(HoleMovement))]
[RequireComponent(typeof(HoleSizeController))]
public class HoleController : MonoBehaviour
{
    // ── HoleSizeController config ─────────────────────────────────────────────
    [Header("Hole Collider")]
    [Tooltip("KillerCollider — BoxCollider bên dưới ground, detect obstacle đã rơi xuống hố.")]
    [SerializeField] private KillerCollider killerCollider;

    [Header("Movement")]
    [SerializeField] private Transform directionArrow;

    private float initialSpeed   = 1.5f;
    private float speedPerGrow   = 0.2f;

    private FloatingScorePool floatingScorePool;

    // Fibonacci-like: mỗi milestone vượt qua trigger 1 lần GrowHole.
    private static readonly int[] GrowMilestones = { 30, 90, 230, 450, 680, 810, 810, 810, 810 };

    // ── Runtime ───────────────────────────────────────────────────────────────
    private float              currentSpeed;
    private HoleMovement       holeMovement;
    private HoleSizeController holeSizeController;
    private IncreaseSizeEffect increaseSizeEffect;
    private SpeedBoosterEffect speedBoosterEffect;
    private EnergyBoosterEffect energyBoosterEffect;

    private int score;
    private int nextMilestoneIndex;

    public int Score => score;

    /// <summary>
    /// Level hiện tại của hole (bắt đầu từ 1).
    /// Level tăng khi vượt milestone (score-based) hoặc khi dùng IncreaseSizeItem.
    /// ItemManager subscribe OnLevelUp để check unlock items.
    /// </summary>
    public int CurrentLevel => nextMilestoneIndex + 1;

    /// <summary>
    /// Fire mỗi khi score thay đổi.
    /// Tham số: (currentLevel, progress 0..1 đến milestone tiếp theo)
    /// currentLevel bắt đầu từ 1.
    /// </summary>
    public event Action<int, float> OnProgressChanged;

    /// <summary>
    /// Fire khi swallow Clock booster. UI subscribe để play fly icon → timerText.
    /// Tham số: (icon sprite, hole world position, booster value in seconds)
    /// </summary>
    public event Action<Sprite, Vector3, float> OnClockBoosterSwallowed;

    /// <summary>
    /// Fire khi vượt milestone và hole grow.
    /// Tham số: level mới (bắt đầu từ 2 khi grow lần đầu).
    /// </summary>
    public event Action<int> OnLevelUp;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        holeMovement       = GetComponent<HoleMovement>();
        holeSizeController = GetComponent<HoleSizeController>();
        increaseSizeEffect  = GetComponentInChildren<IncreaseSizeEffect>();
        speedBoosterEffect  = GetComponentInChildren<SpeedBoosterEffect>();
        energyBoosterEffect = GetComponentInChildren<EnergyBoosterEffect>();
        floatingScorePool  = FindAnyObjectByType<FloatingScorePool>();

        currentSpeed = initialSpeed;

        holeMovement.Init(currentSpeed, directionArrow);
    }

    private void Start()
    {
        holeSizeController.OnGrown += HandleGrown;
        if (killerCollider != null)
        {
            killerCollider.OnObjectSwallowed += HandleObjectSwallowed;
        }
        else
            Debug.LogWarning("[HoleController] KillerCollider not assigned.");

        if (floatingScorePool == null)
            Debug.LogWarning("[HoleController] floatingScorePool is not assigned.");

        nextMilestoneIndex = 0;
    }

    // =========================================================================
    // Public API — gọi từ InputManager / GameplayController
    // =========================================================================

    /// <summary>
    /// Reset toàn bộ state về ban đầu: score, milestone, speed, position.
    /// Gọi từ GameplayController trước mỗi màn mới.
    /// </summary>
    public void ResetToInitial()
    {
        // Reset runtime tracking
        score              = 0;
        nextMilestoneIndex = 0;
        currentSpeed       = initialSpeed;

        // Reset movement speed (tắt luôn speed boost nếu đang active)
        speedBoosterEffect?.ForceDeactivate();
        holeMovement?.SetSpeed(currentSpeed);

        // Reset vị trí player về origin
        transform.position = Vector3.zero;

        // Reset collider + visual về trạng thái ban đầu
        holeSizeController?.Reset();

        // Fire để UI (progress bar, score text...) cập nhật về 0
        FireProgressChanged();

        Debug.Log("[HoleController] ResetToInitial done.");
    }

    public void SetInputEnabled(bool enabled)
    {
        holeMovement?.SetInputEnabled(enabled);
    }

    /// <summary>Gọi mỗi frame từ InputManager.</summary>
    public void ApplyInput(Vector2 direction, float magnitude)
    {
        holeMovement?.Move(direction, magnitude);
    }

    /// <summary>Gọi khi người dùng nhả tay.</summary>
    public void OnInputReleased()
    {
        holeMovement?.OnInputReleased();
    }

    /// <summary>
    /// Cộng trực tiếp vào score và chạy qua CheckGrowMilestones() —
    /// giống hệt swallow. Gọi từ IncreaseSizeEffectDefinition.
    /// </summary>
    public void AddScore(int amount)
    {
        HandleScoreAdded(amount);
    }

    /// <summary>
    /// Khoảng điểm của milestone hiện tại (milestone[n] - milestone[n-1]).
    /// Dùng bởi IncreaseSizeEffectDefinition: cộng thêm đúng lượng này bất kể
    /// score hiện tại đang ở đâu trong khoảng — tránh hard-code "chỉ đủ để chạm ngưỡng".
    ///
    /// Ví dụ: score=15, milestone[0]=30 → range=30 → AddScore(30) → score=45 → vượt milestone.
    /// Trả về 0 nếu đã max level.
    /// </summary>
    public int ScoreRangeOfCurrentLevel()
    {
        if (nextMilestoneIndex >= GrowMilestones.Length) return 0;

        int prev = nextMilestoneIndex > 0 ? GrowMilestones[nextMilestoneIndex - 1] : 0;
        return GrowMilestones[nextMilestoneIndex] - prev;
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private void HandleScoreAdded(int amount)
    {
        score += amount;
        CheckGrowMilestones();
        FireProgressChanged();
    }

    /// <summary>
    /// Callback từ HoleSizeController.OnGrown.
    /// Tăng speed mỗi lần hole grow (cả score path lẫn item path đều qua đây).
    /// </summary>
    private void HandleGrown()
    {
        currentSpeed += speedPerGrow;
        holeMovement?.SetSpeed(currentSpeed);

        Debug.Log($"[HoleController] Speed increased to {currentSpeed}.");
    }

    private void CheckGrowMilestones()
    {
        while (nextMilestoneIndex < GrowMilestones.Length
               && score >= GrowMilestones[nextMilestoneIndex])
        {
            nextMilestoneIndex++;
            int newLevel = nextMilestoneIndex + 1;
            Debug.Log($"[HoleController] Grow! score={score}, milestone={GrowMilestones[nextMilestoneIndex - 1]}");
            holeSizeController.GrowHole();
            increaseSizeEffect?.Play();
            OnLevelUp?.Invoke(newLevel);
        }
    }

    private void FireProgressChanged()
    {
        int   currentLevel = nextMilestoneIndex + 1;
        float progress;

        if (nextMilestoneIndex >= GrowMilestones.Length)
        {
            // Đã vượt hết milestone — giữ bar đầy
            progress = 1f;
        }
        else
        {
            int prevMilestone = nextMilestoneIndex > 0 ? GrowMilestones[nextMilestoneIndex - 1] : 0;
            int nextMilestone = GrowMilestones[nextMilestoneIndex];
            int range         = nextMilestone - prevMilestone;

            progress = range > 0
                ? Mathf.Clamp01((float)(score - prevMilestone) / range)
                : 1f;
        }

        OnProgressChanged?.Invoke(currentLevel, progress);
    }

    private void HandleObjectSwallowed(Obstacle obstacle)
    {
        if (obstacle?.ObstacleDefinition == null) return;

        ObstacleDefinition def = obstacle.ObstacleDefinition;

        Debug.Log($"[HoleController] Swallowed: {obstacle.gameObject.name}, Type={def.Type}");

        if (def.Type == ObstacleType.Booster)
        {
            Debug.Log($"[HoleController] Booster detected: {def.BoosterType}, value={def.BoosterValue}");
            HandleBooster(def);
            return;
        }

        // Normal obstacle — cộng score như bình thường
        HandleScoreAdded(def.ScoreValue);

        if (floatingScorePool != null)
        {
            float playerRadius = 0.5f * transform.localScale.x;
            floatingScorePool.Spawn(def.ScoreValue, transform.position, playerRadius);
        }
    }

    private void HandleBooster(ObstacleDefinition def)
    {
        switch (def.BoosterType)
        {
            case BoosterType.Energy:
                HandleScoreAdded((int)def.BoosterValue);
                energyBoosterEffect?.Play();
                if (floatingScorePool != null)
                {
                    float r = 0.5f * transform.localScale.x;
                    floatingScorePool.Spawn((int)def.BoosterValue, transform.position, r);
                }
                Debug.Log($"[HoleController] Energy booster — +{(int)def.BoosterValue} score.");
                break;

            case BoosterType.Clock:
                // Fire event để UI play fly animation, AddTime sẽ được gọi khi icon đến nơi
                if (OnClockBoosterSwallowed != null)
                {
                    OnClockBoosterSwallowed.Invoke(def.Icon, transform.position, def.BoosterValue);
                }
                else
                {
                    // Không có UI subscriber — cộng thời gian ngay
                    GameTimer.Instance?.AddTime(def.BoosterValue);
                }
                Debug.Log($"[HoleController] Clock booster — +{def.BoosterValue}s.");
                break;

            case BoosterType.Speed:
                // Tăng tốc độ tạm thời qua SpeedBoosterEffect
                if (speedBoosterEffect != null)
                {
                    speedBoosterEffect.Activate(
                        bonus:             def.BoosterValue,
                        duration:          def.BoosterDuration,
                        getBaseSpeedFunc:  () => currentSpeed,
                        applySpeedAction:  s => holeMovement?.SetSpeed(s)
                    );
                }
                else
                {
                    Debug.LogWarning("[HoleController] SpeedBoosterEffect not found on Player.");
                }
                Debug.Log($"[HoleController] Speed booster — x{def.BoosterValue} for {def.BoosterDuration}s.");
                break;
        }
    }

    private void OnDestroy()
    {
        if (holeSizeController != null)
            holeSizeController.OnGrown -= HandleGrown;

        if (killerCollider != null)
            killerCollider.OnObjectSwallowed -= HandleObjectSwallowed;

        OnProgressChanged        = null;
        OnLevelUp                = null;
        OnClockBoosterSwallowed  = null;
    }
}
