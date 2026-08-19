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
    [Header("Hole Size — Colliders")]
    [Tooltip("CapsuleCollider của Visual — detect obstacle rơi xuống hole.")]
    [SerializeField] private CapsuleCollider holeCollider;

    [Tooltip("CapsuleCollider trên Player dùng để boundary giới hạn. Local position luôn là (0,5,0).")]
    [SerializeField] private CapsuleCollider limitedCollider;

    [Tooltip("Root FakeGround chứa tất cả BoxCollider.")]
    [SerializeField] private Transform       fakeGround;

    [Tooltip("Radius ban đầu của hole (0.5).")]
    [SerializeField] private float           initialHoleRadius = 0.5f;

    [Tooltip("Half-size của từng BoxCollider trong FakeGround (100).")]
    [SerializeField] private float           colliderHalfSize  = 100f;

    [Header("Hole Size — Visuals")]
    [Tooltip("HoleSkin — visual hình cái lỗ, luôn hiển thị, scale tăng +1 mỗi lần grow.")]
    [SerializeField] private Transform       holeSkin;

    [Tooltip("DirectionArrow — hiển thị khi có input, ẩn khi không có input, scale tăng +1 mỗi lần grow.")]
    [SerializeField] private Transform       directionArrow;

    [Header("Hole Size — Grow")]
    [SerializeField] private float           growDuration      = 0.5f;
    [SerializeField] private Ease            growEase          = Ease.OutBack;

    // ── HoleMovement config ───────────────────────────────────────────────────
    [Header("Hole Movement")]
    [Tooltip("Vận tốc di chuyển ban đầu.")]
    [SerializeField] private float initialSpeed    = 2f;

    [Tooltip("Lượng vận tốc tăng thêm mỗi lần hole grow.")]
    [SerializeField] private float speedPerGrow    = 1f;
    // directionArrow và fakeGround dùng chung với HoleSizeController

    private FloatingScorePool floatingScorePool;

    // ── Grow milestones ───────────────────────────────────────────────────────
    // Fibonacci-like: mỗi milestone vượt qua trigger 1 lần GrowHole.
    private static readonly int[] GrowMilestones = { 20, 60, 120, 200, 300, 420, 560, 720, 900, 1100 };

    // ── Runtime ───────────────────────────────────────────────────────────────
    private float              currentSpeed;
    private HoleMovement       holeMovement;
    private HoleSizeController holeSizeController;
    private SwallowHandler     swallowHandler;

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
        swallowHandler     = GetComponentInChildren<SwallowHandler>();
        floatingScorePool  = FindAnyObjectByType<FloatingScorePool>();

        currentSpeed = initialSpeed;

        holeMovement.Init(currentSpeed, directionArrow, fakeGround);

        holeSizeController.Init(
            holeCollider,
            limitedCollider,
            holeSkin,
            directionArrow,
            fakeGround,
            initialHoleRadius,
            colliderHalfSize,
            growDuration,
            growEase);
    }

    private void Start()
    {
        holeSizeController.OnScoreAdded += HandleScoreAdded;
        holeSizeController.OnGrown      += HandleGrown;

        if (swallowHandler != null)
            swallowHandler.OnObjectSwallowed += HandleSwallowedForFloatingText;
        else
            Debug.LogWarning("[HoleController] SwallowHandler not found in children.");

        if (floatingScorePool == null)
            Debug.LogWarning("[HoleController] floatingScorePool is not assigned.");

        nextMilestoneIndex = 0;
    }

    // =========================================================================
    // Public API — gọi từ InputManager / GameplayController
    // =========================================================================

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
    /// Grow hole thủ công (từ item). Fire OnLevelUp để UI cập nhật.
    /// Không ảnh hưởng đến score hay milestone tracking.
    /// </summary>
    public void GrowHoleManually()
    {
        if (holeSizeController == null) return;

        holeSizeController.GrowHole();

        // Tăng level hiển thị (không dựa vào milestone)
        int currentLevel = nextMilestoneIndex + 1;
        OnLevelUp?.Invoke(currentLevel + 1);

        // Update progress bar (giữ nguyên progress, chỉ tăng level)
        FireProgressChanged();

        Debug.Log($"[HoleController] Manually grew hole. New display level: {currentLevel + 1}");
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
    /// Tăng speed thêm speedPerGrow mỗi lần hole grow.
    /// </summary>
    private void HandleGrown(float newRadius)
    {
        currentSpeed += speedPerGrow;
        holeMovement?.SetSpeed(currentSpeed);

        Debug.Log($"[HoleController] Speed increased to {currentSpeed} (radius={newRadius:F2}).");
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

    private void HandleSwallowedForFloatingText(Obstacle obstacle)
    {
        if (floatingScorePool           == null) return;
        if (obstacle                    == null) return;
        if (obstacle.ObstacleDefinition == null) return;

        floatingScorePool.Spawn(
            obstacle.ObstacleDefinition.ScoreValue,
            obstacle.transform.position);
    }

    private void OnDestroy()
    {
        if (holeSizeController != null)
        {
            holeSizeController.OnScoreAdded -= HandleScoreAdded;
            holeSizeController.OnGrown      -= HandleGrown;
        }

        if (swallowHandler != null)
            swallowHandler.OnObjectSwallowed -= HandleSwallowedForFloatingText;

        OnProgressChanged = null;
        OnLevelUp         = null;
    }
}
