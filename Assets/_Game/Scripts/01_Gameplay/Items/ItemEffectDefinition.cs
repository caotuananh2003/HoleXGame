using UnityEngine;

/// <summary>
/// Base abstraction cho tất cả Item Effect.
/// Strategy Pattern: mỗi effect có behavior riêng, ItemManager không biết detail implementation.
///
/// Subclasses implement ApplyEffect() để define behavior cụ thể:
///   - IncreaseSizeEffectDefinition: gọi HoleSizeController.GrowHole()
///   - TimeExtensionEffectDefinition: gọi GameTimer.AddTime()
///   - MagnetEffectDefinition: spawn MagnetEffect runtime component
///   - BombShieldEffectDefinition: spawn BombShieldEffect runtime component
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
    /// </summary>
    public abstract void ApplyEffect(ItemEffectContext context);
}
