using UnityEngine;

/// <summary>
/// Effect tạo lực hút các object có thể swallow trong phạm vi về phía hole.
/// Spawn runtime MonoBehaviour (MagnetEffect) để xử lý physics loop.
/// MagnetEffect tự destroy sau duration.
/// </summary>
[CreateAssetMenu(fileName = "MagnetEffect", menuName = "Items/Effects/Magnet Effect")]
public class MagnetEffectDefinition : ItemEffectDefinition
{
    [Header("Config")]
    [Tooltip("Bán kính vùng hút (đơn vị Unity).")]
    [SerializeField] private float radius = 10f;

    [Tooltip("Lực hút áp dụng cho từng object/frame.")]
    [SerializeField] private float force = 5f;

    [Tooltip("Thời gian hiệu lực (giây).")]
    [SerializeField] private float duration = 10f;

    public override void ApplyEffect(ItemEffectContext context)
    {
        if (context.holeTransform == null)
        {
            Debug.LogWarning("[MagnetEffect] holeTransform is null — cannot apply effect.");
            return;
        }

        // Spawn runtime component lên hole GameObject
        MagnetEffect magnetEffect = context.holeTransform.gameObject.AddComponent<MagnetEffect>();
        magnetEffect.Initialize(radius, force, duration);

        Debug.Log($"[MagnetEffect] Applied — radius={radius}, force={force}, duration={duration}s.");
    }
}
