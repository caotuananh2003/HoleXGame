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
    [SerializeField] private float           speed             = 3f;
    // directionArrow và fakeGround dùng chung với HoleSizeController

    private FloatingScorePool floatingScorePool;

    // ── Grow milestones ───────────────────────────────────────────────────────
    // Fibonacci-like: mỗi milestone vượt qua trigger 1 lần GrowHole.
    private static readonly int[] GrowMilestones = { 20, 40, 60, 100, 160, 260, 420, 680, 1100, 1780 };

    // ── Runtime ───────────────────────────────────────────────────────────────
    private HoleMovement       holeMovement;
    private HoleSizeController holeSizeController;
    private SwallowHandler     swallowHandler;

    private int score;
    private int nextMilestoneIndex;

    public int Score => score;

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
        floatingScorePool = FindAnyObjectByType<FloatingScorePool>();

        holeMovement.Init(speed, directionArrow, fakeGround);

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

    // =========================================================================
    // Internal
    // =========================================================================

    private void HandleScoreAdded(int amount)
    {
        score += amount;
        CheckGrowMilestones();
        FireProgressChanged();
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
            holeSizeController.OnScoreAdded -= HandleScoreAdded;

        if (swallowHandler != null)
            swallowHandler.OnObjectSwallowed -= HandleSwallowedForFloatingText;

        OnProgressChanged = null;
        OnLevelUp         = null;
    }
}
