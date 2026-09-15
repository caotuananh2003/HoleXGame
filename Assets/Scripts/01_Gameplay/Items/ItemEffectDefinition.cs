using UnityEngine;

/// <summary>
/// Base abstraction cho tất cả Item Effect.
/// Strategy Pattern: mỗi effect có behavior riêng, ItemManager không biết detail implementation.
///
/// Subclasses implement ApplyEffect() để define behavior cụ thể:
///   - IncreaseSizeEffectDefinition: gọi HoleSizeController.GrowHole()     → trả về null
///   - TimeExtensionEffectDefinition: gọi GameTimer.AddTime()               → trả về null
///   - MagnetEffectDefinition: spawn MagnetEffect runtime component         → trả về ITimedEffect
///   - BombShieldEffectDefinition: spawn BombShieldEffect runtime component → trả về ITimedEffect
///
/// Return value:
///   - Trả về ITimedEffect nếu effect có duration (Magnet, BombShield...).
///   - Trả về null nếu effect là instant (IncreaseSize, TimeExtension...).
///   - ItemManager dùng giá trị này để fire OnItemEffectStarted cho ItemSlotUI.
///
/// Design principle:
///   - Open/Closed: thêm effect mới không sửa ItemManager
///   - Single Responsibility: Effect chỉ chứa logic apply effect, không biết về ItemManager
///   - Dependency Inversion: ItemManager phụ thuộc abstraction, không phụ thuộc concrete
/// </summary>
public abstract class ItemEffectDefinition : ScriptableObject
{
    /// <summary>
    /// Apply effect vào gameplay.
    /// Context chứa dependencies cần thiết (HoleController, GameTimer...).
    /// Trả về ITimedEffect nếu effect có duration, null nếu là instant effect.
    /// </summary>
    public abstract ITimedEffect ApplyEffect(ItemEffectContext context);
}
