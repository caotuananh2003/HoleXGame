using UnityEngine;

/// <summary>
/// Entry point của player. Điều phối HoleMovement và HoleSizeController.
/// Gắn vào root GameObject của Player trong GameplayScene.
/// Tự tìm HoleMovement và HoleSizeController bằng GetComponent.
/// </summary>
[RequireComponent(typeof(HoleMovement))]
[RequireComponent(typeof(HoleSizeController))]
[RequireComponent(typeof(SwallowHandler))]
public class HoleController : MonoBehaviour
{
    private HoleMovement       holeMovement;
    private HoleSizeController holeSizeController;

    private int  score;
    private bool isGrowing;

    public int Score => score;

    private void Awake()
    {
        holeMovement   = GetComponent<HoleMovement>();
        holeSizeController = GetComponent<HoleSizeController>();
    }

    private void Start()
    {
        holeSizeController.OnScoreAdded += HandleScoreAdded;
    }

    public void SetInputEnabled(bool enabled)
    {
        holeMovement?.SetInputEnabled(enabled);
    }

    /// <summary>Gọi mỗi frame từ InputManager để di chuyển hole.</summary>
    public void ApplyInput(Vector2 direction, float magnitude)
    {
        holeMovement?.Move(direction, magnitude);
    }

    /// <summary>Gọi khi người dùng nhả tay.</summary>
    public void OnInputReleased()
    {
        holeMovement?.OnInputReleased();
    }

    private void HandleScoreAdded(int amount)
    {
        score += amount;
        isGrowing = false;
        CheckGrow();
    }

    private void CheckGrow()
    {
        if (!isGrowing && score % 10 == 0)
        {
            Debug.Log("Growing");
            isGrowing = true;
            holeSizeController.GrowHole();
        }
    }

    private void OnDestroy()
    {
        holeSizeController.OnScoreAdded -= HandleScoreAdded;
    }
}
