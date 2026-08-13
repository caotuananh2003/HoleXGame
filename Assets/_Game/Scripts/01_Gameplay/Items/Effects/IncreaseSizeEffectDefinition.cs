using UnityEngine;

/// <summary>
/// Effect tăng kích thước hole.
/// Sử dụng HoleSizeController.GrowHole() hiện có — không tạo hệ thống size mới.
/// GrowHole() tăng radius +0.5 và animate visual.
/// </summary>
[CreateAssetMenu(fileName = "IncreaseSizeEffect", menuName = "Items/Effects/Increase Size Effect")]
public class IncreaseSizeEffectDefinition : ItemEffectDefinition
{
    [Header("Config")]
    [Tooltip("Số lần gọi GrowHole(). Mặc định = 1 (tăng 1 bậc size).")]
    [SerializeField] private int growCount = 1;

    public override void ApplyEffect(ItemEffectContext context)
    {
        if (context.holeController == null)
        {
            Debug.LogWarning("[IncreaseSizeEffect] HoleController is null — cannot apply effect.");
            return;
        }

        HoleSizeController sizeController = context.holeController.GetComponent<HoleSizeController>();
        if (sizeController == null)
        {
            Debug.LogWarning("[IncreaseSizeEffect] HoleSizeController not found on HoleController.");
            return;
        }

        for (int i = 0; i < growCount; i++)
        {
            sizeController.GrowHole();
        }

        Debug.Log($"[IncreaseSizeEffect] Applied — hole grew {growCount} time(s).");
    }
}
