using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Gắn trên Player (cùng GameObject hoặc child). ParticleSystem là child của Player
/// nên tự động phát tại vị trí player khi bomb bị nuốt.
///
/// KillerCollider giữ reference đến component này qua [SerializeField] và gọi
/// PlayAsync() trước khi fire OnBombSwallowedWithoutShield.
/// </summary>
public class BombExplosionEffect : MonoBehaviour
{
    [Tooltip("ParticleSystem hiệu ứng nổ — là child của Player.")]
    [SerializeField] private ParticleSystem explosionParticle;

    [Tooltip("Thời gian chờ thêm sau khi particle dừng (giây).")]
    [SerializeField] private float extraDelay = 0.1f;

    /// <summary>
    /// Play hiệu ứng nổ tại vị trí player và await đến khi hoàn tất.
    /// Gọi từ KillerCollider trước khi fire OnBombSwallowedWithoutShield.
    /// </summary>
    public async UniTask PlayAsync()
    {
        if (explosionParticle == null)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(extraDelay),
                cancellationToken: this.GetCancellationTokenOnDestroy());
            return;
        }

        explosionParticle.gameObject.SetActive(true);
        explosionParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        explosionParticle.Play();

        await UniTask.WaitUntil(() => !explosionParticle.isPlaying,
            cancellationToken: this.GetCancellationTokenOnDestroy());

        if (extraDelay > 0f)
            await UniTask.Delay(System.TimeSpan.FromSeconds(extraDelay),
                cancellationToken: this.GetCancellationTokenOnDestroy());
    }
}
