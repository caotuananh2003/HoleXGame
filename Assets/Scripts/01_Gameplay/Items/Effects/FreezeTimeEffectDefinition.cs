using UnityEngine;

/// <summary>
/// Effect đóng băng thời gian trong gameplay.
/// Time.timeScale = 0 → physics, animation, GameTimer đứng lại.
/// Input và hole movement vẫn hoạt động (dùng unscaledDeltaTime).
///
/// Yêu cầu setup trong scene:
///   Player/FreezeTime (luôn active)
///   └── có component FreezeTimeEffect gắn sẵn
///       └── kéo FreezeTimeOverlay (CanvasGroup) vào field freezeOverlay
/// </summary>
[CreateAssetMenu(fileName = "FreezeTimeEffectDefinition", menuName = "Items/Effects/Freeze Time Effect Definition")]
public class FreezeTimeEffectDefinition : ItemEffectDefinition
{
    [Header("Config")]
    [Tooltip("Thời gian đóng băng (giây, unscaled).")]
    [SerializeField] private float duration = 10f;

    public override ITimedEffect ApplyEffect(ItemEffectContext context)
    {
        if (context.holeController == null)
        {
            Debug.LogWarning("[FreezeTimeEffect] holeController is null — cannot apply effect.");
            return null;
        }

        FreezeTimeEffect freezeEffect = context.holeController.GetComponentInChildren<FreezeTimeEffect>(true);

        if (freezeEffect == null)
        {
            Debug.LogError("[FreezeTimeEffect] Không tìm thấy FreezeTimeEffect trong children của Player. " +
                           "Hãy gắn component FreezeTimeEffect lên object FreezeTime (child của Player).");
            return null;
        }

        if (freezeEffect.Remaining > 0f)
        {
            freezeEffect.ExtendDuration(duration);
            Debug.Log("[FreezeTimeEffect] Extended duration by " + duration + "s.");
            return freezeEffect;
        }

        freezeEffect.Initialize(duration, context.gameTimer);
        Debug.Log("[FreezeTimeEffect] Applied — duration=" + duration + "s.");
        return freezeEffect;
    }
}
