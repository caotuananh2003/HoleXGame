using UnityEngine;

/// <summary>
/// Effect chặn bomb explosion — khi active, swallow bomb không trigger game over.
///
/// Yêu cầu setup trong scene:
///   Player/Visuals/ShieldVisual (inactive mặc định)
///   └── có component BombShieldEffect gắn sẵn
///
/// ApplyEffect() chỉ tìm component đã có và gọi Initialize() / ExtendDuration().
/// Không dùng AddComponent — tránh tạo/destroy GameObject động.
/// </summary>
[CreateAssetMenu(fileName = "BombShieldEffect", menuName = "Items/Effects/Bomb Shield Effect")]
public class BombShieldEffectDefinition : ItemEffectDefinition
{
    [Header("Config")]
    [Tooltip("Thời gian shield hiệu lực (giây).")]
    [SerializeField] private float duration = 15f;

    public override ITimedEffect ApplyEffect(ItemEffectContext context)
    {
        if (context.holeController == null)
        {
            Debug.LogWarning("[BombShieldEffect] holeController is null — cannot apply effect.");
            return null;
        }

        HoleSizeController sizeController = context.holeController.GetComponent<HoleSizeController>();
        if (sizeController == null)
        {
            Debug.LogWarning("[BombShieldEffect] HoleSizeController not found on HoleController.");
        }

        BombShieldEffect shield = context.holeController.GetComponentInChildren<BombShieldEffect>(true);
        if (shield == null)
        {
            Debug.LogError("[BombShieldEffect] Không tìm thấy BombShieldEffect trong children của Player. " +
                           "Hãy gắn ShieldVisual (có component BombShieldEffect) làm child của Player và đặt inactive.");
            return null;
        }

        // Nếu đang active thì extend, không initialize lại — timer vẫn đang chạy
        if (BombShieldEffect.IsActive)
        {
            shield.ExtendDuration(duration);
            Debug.Log($"[BombShieldEffect] Extended duration by {duration}s.");
            return shield; // Trả về instance đang active để ItemSlotUI cập nhật timer
        }

        shield.Initialize(duration, sizeController);
        Debug.Log($"[BombShieldEffect] Applied — duration={duration}s.");
        return shield; // Timed effect — trả về ITimedEffect để ItemSlotUI track timer
    }
}
