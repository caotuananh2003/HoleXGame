using UnityEngine;

/// <summary>
/// Effect tăng kích thước hole bằng cách cộng điểm trực tiếp vào score —
/// giống hệt swallow, không có code path riêng biệt.
///
/// Mỗi lần grow: cộng đúng số điểm để vượt milestone tiếp theo →
/// CheckGrowMilestones() tự trigger grow, speed, visual như bình thường.
/// </summary>
[CreateAssetMenu(fileName = "IncreaseSizeEffect", menuName = "Items/Effects/Increase Size Effect")]
public class IncreaseSizeEffectDefinition : ItemEffectDefinition
{
    [Header("Config")]
    [Tooltip("Số lần grow. Mặc định = 1.")]
    [SerializeField] private int growCount = 1;

    public override ITimedEffect ApplyEffect(ItemEffectContext context)
    {
        if (context.holeController == null)
        {
            Debug.LogWarning("[IncreaseSizeEffect] HoleController is null — cannot apply effect.");
            return null;
        }

        for (int i = 0; i < growCount; i++)
        {
            int range = context.holeController.ScoreRangeOfCurrentLevel();
            if (range <= 0)
            {
                Debug.Log("[IncreaseSizeEffect] Already at max level.");
                break;
            }

            // Cộng đúng khoảng điểm của milestone hiện tại
            // Ví dụ: score=15, range=30 → score trở thành 45 → vượt ngưỡng 30 → grow
            context.holeController.AddScore(range);
            Debug.Log($"[IncreaseSizeEffect] Added {range} score (full milestone range).");
        }

        return null; // Instant effect — không có duration
    }
}
