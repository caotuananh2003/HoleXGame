using UnityEngine;

/// <summary>
/// Effect tăng thời gian còn lại của màn chơi.
/// Sử dụng GameTimer.AddTime() hiện có — không tạo Timer system mới.
/// GameTimer.AddTime(seconds) cộng thêm giây vào Remaining và tiếp tục chạy.
/// </summary>
[CreateAssetMenu(fileName = "TimeExtensionEffect", menuName = "Items/Effects/Time Extension Effect")]
public class TimeExtensionEffectDefinition : ItemEffectDefinition
{
    [Header("Config")]
    [Tooltip("Số giây cộng thêm vào timer. Ví dụ: 30 = +30s.")]
    [SerializeField] private float additionalTime = 30f;

    public override void ApplyEffect(ItemEffectContext context)
    {
        if (context.gameTimer == null)
        {
            Debug.LogWarning("[TimeExtensionEffect] GameTimer is null — cannot apply effect.");
            return;
        }

        context.gameTimer.AddTime(additionalTime);

        Debug.Log($"[TimeExtensionEffect] Applied — added {additionalTime}s to timer.");
    }
}
