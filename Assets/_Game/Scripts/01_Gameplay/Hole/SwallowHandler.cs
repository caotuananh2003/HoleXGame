using System;
using UnityEngine;

/// <summary>
/// Fired Event OnObjectSwallowed khi một object rơi vào lỗ.
/// Truyền Obstacle component để các subscriber biết loại obstacle nào bị nuốt.
/// 
/// OnTriggerEnter: object rơi vào collider → fire event với Obstacle data
/// OnTriggerExit:  object rơi xuyên qua hoàn toàn → disable
///
/// Bomb logic:
///   Nếu obstacle.Type == Bomb và BombShieldEffect.IsActive == false
///   → Fire OnBombSwallowedWithoutShield → GameplayController trigger game over
///
/// Trigger collider cần gắn trên cùng GameObject với script này (Player root).
/// </summary>
public class SwallowHandler : MonoBehaviour
{
    private const int Swallowable = 9;

    /// <summary>
    /// Event fire khi obstacle bị nuốt. Truyền Obstacle component.
    /// Subscribers sẽ lấy ObstacleDefinition từ đây để biết loại, điểm số, icon...
    /// </summary>
    public event Action<Obstacle> OnObjectSwallowed;

    /// <summary>
    /// Event fire khi swallow bomb mà không có shield.
    /// GameplayController subscribe để trigger game over.
    /// </summary>
    public event Action OnBombSwallowedWithoutShield;

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.position.y >= -0.1f) return;

        if (other.gameObject.layer == Swallowable)
        {
            // Lấy Obstacle component từ object bị nuốt
            Obstacle obstacle = other.GetComponentInParent<Obstacle>();
            if (obstacle == null)
            {
                Debug.LogWarning($"[SwallowHandler] Object {other.gameObject.name} trên layer Swallowable nhưng không có Obstacle component!", other);
                other.transform.parent.gameObject.SetActive(false);
                return;
            }

            if (obstacle.ObstacleDefinition == null)
            {
                Debug.LogWarning($"[SwallowHandler] Obstacle {obstacle.name} không có ObstacleDefinition!", obstacle);
                other.transform.parent.gameObject.SetActive(false);
                return;
            }

            // ── Check Bomb Logic ──────────────────────────────────────────────
            if (obstacle.ObstacleDefinition.Type == ObstacleType.Bomb)
            {
                if (!BombShieldEffect.IsActive)
                {
                    Debug.Log($"[SwallowHandler] Swallowed BOMB without shield — triggering game over!");
                    OnBombSwallowedWithoutShield?.Invoke();
                    other.transform.parent.gameObject.SetActive(false);
                    return; // Không fire OnObjectSwallowed — bomb không tính score
                }
                else
                {
                    Debug.Log($"[SwallowHandler] Swallowed BOMB but shield is active — no game over.");
                    // Shield active → swallow bomb như normal object, fire OnObjectSwallowed
                }
            }

            // ── Normal Swallow ────────────────────────────────────────────────
            Debug.Log($"[SwallowHandler] Swallowing {other.gameObject.name} (Type: {obstacle.ObstacleDefinition.Id})");
            OnObjectSwallowed?.Invoke(obstacle);
            other.transform.parent.gameObject.SetActive(false);
        }
    }
}
