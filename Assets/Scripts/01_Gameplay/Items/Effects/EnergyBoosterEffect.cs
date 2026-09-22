using UnityEngine;

/// <summary>
/// Gắn lên EnergyBoosterEffect — child của Player.
/// Khi swallow Energy booster: kích hoạt particle visual.
/// Logic score được xử lý bởi HoleController.HandleBooster — không xử lý ở đây.
/// Stop Action = Disable đã cài trong Inspector, không cần tắt thủ công.
/// </summary>
public class EnergyBoosterEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem energyParticle;

    private void Awake()
    {
        if (energyParticle == null)
            Debug.LogWarning("[EnergyBoosterEffect] energyParticle chưa được gán.", this);
    }

    /// <summary>
    /// Play particle visual. Gọi từ HoleController.HandleBooster khi swallow Energy booster.
    /// </summary>
    public void Play()
    {
        if (energyParticle == null) return;

        energyParticle.gameObject.SetActive(true);
        energyParticle.Play();
    }
}
