using System;
using UnityEngine;

/// <summary>
/// Fired Event OnObjectSwallowed khi một object rơi vào lỗ.
/// Truyền Obstacle component để các subscriber biết loại obstacle nào bị nuốt.
/// 
/// OnTriggerEnter: object rơi vào collider → fire event với Obstacle data
/// OnTriggerExit:  object rơi xuyên qua hoàn toàn → disable
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Swallowable)
        {
            // Lấy Obstacle component từ object bị nuốt
            Obstacle obstacle = other.GetComponent<Obstacle>();
            
            if (obstacle != null && obstacle.ObstacleDefinition != null)
            {
                Debug.Log($"Swallowing {other.gameObject.name} (Type: {obstacle.ObstacleDefinition.ObstacleID})");
                OnObjectSwallowed?.Invoke(obstacle);
            }
            else
            {
                Debug.LogWarning($"[SwallowHandler] Object {other.gameObject.name} trên layer Swallowable nhưng không có Obstacle component hoặc Definition!", other);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.position.y >= 0f) return;

        other.gameObject.SetActive(false);
    }
}
