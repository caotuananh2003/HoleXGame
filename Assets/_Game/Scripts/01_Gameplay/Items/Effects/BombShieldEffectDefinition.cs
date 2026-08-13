using UnityEngine;

/// <summary>
/// Effect chặn bomb explosion — khi active, swallow bomb không trigger game over.
/// Spawn runtime MonoBehaviour (BombShieldEffect) để track active state.
/// BombShieldEffect tự destroy sau duration.
/// SwallowHandler sẽ check BombShieldEffect.IsActive trước khi trigger game over.
/// </summary>
[CreateAssetMenu(fileName = "BombShieldEffect", menuName = "Items/Effects/Bomb Shield Effect")]
public class BombShieldEffectDefinition : ItemEffectDefinition
{
    [Header("Config")]
    [Tooltip("Thời gian shield hiệu lực (giây).")]
    [SerializeField] private float duration = 15f;

    public override void ApplyEffect(ItemEffectContext context)
    {
        if (context.holeTransform == null)
        {
            Debug.LogWarning("[BombShieldEffect] holeTransform is null — cannot apply effect.");
            return;
        }

        // Nếu đã có BombShieldEffect active, extend duration thay vì stack
        BombShieldEffect existingShield = context.holeTransform.GetComponent<BombShieldEffect>();
        if (existingShield != null)
        {
            existingShield.ExtendDuration(duration);
            Debug.Log($"[BombShieldEffect] Extended duration by {duration}s.");
            return;
        }

        // Spawn runtime component
        BombShieldEffect shieldEffect = context.holeTransform.gameObject.AddComponent<BombShieldEffect>();
        shieldEffect.Initialize(duration);

        Debug.Log($"[BombShieldEffect] Applied — duration={duration}s.");
    }
}
