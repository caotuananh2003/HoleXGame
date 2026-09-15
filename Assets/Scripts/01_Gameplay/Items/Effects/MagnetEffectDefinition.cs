using UnityEngine;

/// <summary>
/// Effect tạo lực hút các object có thể swallow trong phạm vi về phía hole.
///
/// Yêu cầu setup trong scene:
///   Player/Visuals/Tornado (inactive mặc định)
///   └── có component MagnetEffect gắn sẵn
///
/// ApplyEffect() chỉ tìm component đã có và gọi Initialize() / ExtendDuration().
/// Không dùng AddComponent — tránh tạo/destroy component động.
/// </summary>
[CreateAssetMenu(fileName = "MagnetEffect", menuName = "Items/Effects/Magnet Effect")]
public class MagnetEffectDefinition : ItemEffectDefinition
{
    [Header("Config")]
    [Tooltip("Bán kính vùng hút (đơn vị Unity).")]
    [SerializeField] private float radius = 10f;

    [Tooltip("Lực hút áp dụng cho từng object/frame (ForceMode.Acceleration — đơn vị m/s²).")]
    [SerializeField] private float force = 20f;

    [Tooltip("Thời gian hiệu lực (giây).")]
    [SerializeField] private float duration = 15f;

    public override ITimedEffect ApplyEffect(ItemEffectContext context)
    {
        if (context.holeTransform == null)
        {
            Debug.LogWarning("[MagnetEffect] holeTransform is null — cannot apply effect.");
            return null;
        }

        MagnetEffect magnetEffect = context.holeTransform.root.GetComponentInChildren<MagnetEffect>(true);

        if (magnetEffect == null)
        {
            Debug.LogError("[MagnetEffect] Không tìm thấy MagnetEffect component trong children của Player. " +
                           "Hãy gắn Tornado (có component MagnetEffect) làm child của Player/Visuals và đặt inactive.");
            return null;
        }

        if (magnetEffect.gameObject.activeInHierarchy && magnetEffect.Remaining > 0f)
        {
            magnetEffect.ExtendDuration(duration);
            Debug.Log($"[MagnetEffect] Extended duration by {duration}s.");
            return magnetEffect;
        }

        magnetEffect.Initialize(radius, force, duration, context.holeTransform);
        Debug.Log($"[MagnetEffect] Applied — radius={radius}, force={force}, duration={duration}s.");
        return magnetEffect;
    }
}
