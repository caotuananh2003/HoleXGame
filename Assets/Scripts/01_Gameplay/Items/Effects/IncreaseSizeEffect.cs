using DG.Tweening;
using UnityEngine;

/// <summary>
/// Gắn lên IncreaseSizeEffect — child của Player.
/// Component này chịu trách nhiệm hoàn toàn về visual grow:
///   - Enable particle GrowUp rồi Play
///   - Chạy DOTween sequence squash & stretch trên Visual theo list keyframe
///
/// HoleSizeController chỉ xử lý logic scale Player + collider.
/// IncreaseSizeEffect chỉ xử lý visual feedback.
///
/// Gọi Play() từ IncreaseSizeEffectDefinition.ApplyEffect() mỗi lần grow.
/// </summary>
public class IncreaseSizeEffect : MonoBehaviour
{
    [Header("References")]
    [Tooltip("ParticleSystem GrowUp — bị disabled mặc định, được enable khi grow.")]
    [SerializeField] private ParticleSystem growUpParticle;

    [Tooltip("Transform của Visual (HoleSkin) — đối tượng sẽ squash & stretch.")]
    [SerializeField] private Transform visual;

    [Header("Squash & Stretch Keyframes")]
    [Tooltip(
        "Danh sách scale mà Visual sẽ đi qua khi grow.\n" +
        "Ví dụ: (1.2, 0.8, 1) → (0.8, 1.2, 1) → (1, 1, 1)\n" +
        "Frame cuối nên là scale gốc để Visual trở về trạng thái ban đầu.")]
    [SerializeField] private Vector3[] squashKeyframes = new Vector3[]
    {
        new Vector3(1.2f, 0.8f, 1f),
        new Vector3(0.8f, 1.2f, 1f),
        new Vector3(1f,   1f,   1f),
    };

    [Header("Timing")]
    [Tooltip("Tổng thời gian chạy hết squash & stretch sequence (giây). Chia đều cho từng keyframe.")]
    [SerializeField] private float squashDuration = 0.15f;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        // Particle bị disable mặc định trong Inspector — không cần SetActive ở đây
        if (growUpParticle == null)
            Debug.LogWarning("[IncreaseSizeEffect] growUpParticle chưa được gán.");

        if (visual == null)
            Debug.LogWarning("[IncreaseSizeEffect] visual chưa được gán.");
    }

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>
    /// Play toàn bộ visual grow effect: particle + squash & stretch.
    /// Gọi từ IncreaseSizeEffectDefinition.ApplyEffect() mỗi lần GrowHoleManually().
    /// </summary>
    public void Play()
    {
        PlayParticle();
        PlaySquash();
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private void PlayParticle()
    {
        if (growUpParticle == null) return;

        // Enable trước rồi mới play — StopAction = Disable sẽ tự tắt khi xong
        growUpParticle.gameObject.SetActive(true);
        growUpParticle.Play();
    }

    private void PlaySquash()
    {
        if (visual == null) return;
        if (squashKeyframes == null || squashKeyframes.Length == 0) return;

        DOTween.Kill(visual);

        float stepDuration = squashDuration / squashKeyframes.Length;

        Sequence seq = DOTween.Sequence().SetTarget(visual);

        foreach (Vector3 targetScale in squashKeyframes)
        {
            seq.Append(visual.DOScale(targetScale, stepDuration).SetEase(Ease.Linear));
        }
    }
}
