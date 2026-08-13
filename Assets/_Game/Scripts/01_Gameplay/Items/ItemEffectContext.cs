using UnityEngine;

/// <summary>
/// Context holder chứa dependencies cần thiết để apply item effect.
/// Truyền vào ItemEffectDefinition.ApplyEffect() thay vì inject từng dependency riêng lẻ.
/// Tránh coupling giữa Effect và toàn bộ gameplay systems.
/// </summary>
public struct ItemEffectContext
{
    /// <summary>HoleController — điều phối hole movement/size.</summary>
    public HoleController holeController;

    /// <summary>GameTimer — quản lý thời gian gameplay.</summary>
    public GameTimer gameTimer;

    /// <summary>Transform của hole — dùng để spawn runtime components (Magnet, Shield).</summary>
    public Transform holeTransform;

    public ItemEffectContext(HoleController holeController, GameTimer gameTimer, Transform holeTransform)
    {
        this.holeController = holeController;
        this.gameTimer = gameTimer;
        this.holeTransform = holeTransform;
    }
}
