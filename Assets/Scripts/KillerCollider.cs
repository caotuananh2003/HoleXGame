using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Gắn lên BoxCollider rộng nằm bên dưới ground (~y = -5).
/// Detect obstacle đã rơi xuyên qua hố thật sự.
/// </summary>
public class KillerCollider : MonoBehaviour
{
    /// <summary>
    /// Fire khi obstacle bị nuốt (non-bomb hoặc bomb có shield).
    /// Dùng event vì nhiều subscriber không xác định tại compile-time
    /// (HoleController tính score, GameplayObjectiveManager track objective).
    /// </summary>
    public event Action<Obstacle> OnObjectSwallowed;

    [Tooltip("BombExplosionEffect gắn trên Player.")]
    [SerializeField] private BombExplosionEffect bombExplosionEffect;

    private void OnTriggerEnter(Collider other)
    {
        Obstacle obstacle = other.GetComponentInParent<Obstacle>();

        if (obstacle == null || obstacle.ObstacleDefinition == null)
        {
            other.transform.parent.gameObject.SetActive(false);
            return;
        }

        if (obstacle.ObstacleDefinition.Type == ObstacleType.Bomb)
        {
            if (!BombShieldEffect.IsActive)
            {
                Debug.Log("[KillerCollider] Bomb swallowed — no shield.");
                HandleBombAsync(other.transform.parent.gameObject).Forget();
            }
            else
            {
                Debug.Log("[KillerCollider] Bomb swallowed — shield active.");
                other.transform.parent.gameObject.SetActive(false);
            }
            return;
        }

        OnObjectSwallowed?.Invoke(obstacle);
        other.transform.parent.gameObject.SetActive(false);
    }

    private async UniTaskVoid HandleBombAsync(GameObject bombRoot)
    {
        // Disable input ngay lập tức — trước khi particle play
        GameplayController.Instance?.OnBombHit();

        bombRoot.SetActive(false);

        if (bombExplosionEffect != null)
            await bombExplosionEffect.PlayAsync();

        // Particle xong — chuyển game over
        GameplayController.Instance?.OnBombExplosionFinished();
    }
}
