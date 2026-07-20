using UnityEngine;
using VContainer;

/// <summary>
/// Entry point của player. Điều phối HoleMovement, HoleSizeController.
/// Gắn vào root GameObject của Hole trong GameplayScene.
/// </summary>

[RequireComponent(typeof(HoleMovement))]
[RequireComponent(typeof(HoleSizeController))]
public class HoleController : MonoBehaviour
{
    #region Inject
    private HoleMovement     holeMovement;
    private HoleSizeController sizeController;

    [Inject]
    private void Construct(HoleMovement holeMovement, HoleSizeController holeSizeController)
    {
        this.holeMovement = holeMovement;
        this.sizeController = holeSizeController;
    }
    #endregion

    private int  score;
    private bool growPending;

    public int Score => score;

    private void Start()
    {
        // Khởi tạo scale ban đầu = 1
        sizeController.GrowHole();

        sizeController.OnScoreAdded += HandleScoreAdded;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void SetInputEnabled(bool enabled)
    {
        holeMovement?.SetInputEnabled(enabled);
    }

    /// <summary>
    /// Gọi mỗi frame từ inputManager để di chuyển hole.
    /// </summary>
    public void ApplyInput(Vector2 direction, float magnitude)
    {
        holeMovement?.Move(direction, magnitude);
    }

    /// <summary>
    /// Gọi khi người dùng nhả tay.
    /// </summary>
    public void OnInputReleased()
    {
        holeMovement?.OnInputReleased();
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void HandleScoreAdded(int amount)
    {
        score += amount;
        growPending = false;
        CheckGrow();
    }

    private void CheckGrow()
    {
        // Grow mỗi 10 điểm
        if (!growPending && score % 10 == 0)
        {
            growPending = true;
            sizeController.GrowHole();
        }
    }

    private void OnDestroy()
    {
        if (sizeController != null)
            sizeController.OnScoreAdded -= HandleScoreAdded;
    }
}
