using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gắn lên FreezeTime — child của Player.
///
/// Khi active:
///   - GameTimer.Pause() → đồng hồ đứng lại
///   - Player, physics, item vẫn hoạt động bình thường (Time.timeScale không đổi)
///   - FreezeTimeOverlay fade alpha 0 → 1 (DOTween)
///
/// Khi hết thời gian:
///   - GameTimer.Resume() → đồng hồ chạy tiếp
///   - FreezeTimeOverlay fade alpha 1 → 0 (DOTween)
///   - Fire OnExpired
/// </summary>
public class FreezeTimeEffect : MonoBehaviour, ITimedEffect
{
    [Header("Overlay")]
    [Tooltip("CanvasGroup của FreezeTimeOverlay UI.")]
    [SerializeField] private CanvasGroup freezeOverlay;

    [Tooltip("Thời gian fade in overlay (giây).")]
    [SerializeField] private float fadeInDuration = 0.3f;

    [Tooltip("Thời gian fade out overlay (giây).")]
    [SerializeField] private float fadeOutDuration = 0.4f;

    [Header("Particle")]
    [Tooltip("ParticleSystem child của FreezeTime object. Stop Action = Disable đã cài trong Inspector.")]
    [SerializeField] private ParticleSystem freezeParticle;

    // ── ITimedEffect ──────────────────────────────────────────────────────────

    public float Remaining
    {
        get { return remaining; }
    }

    public float TotalDuration
    {
        get { return totalDuration; }
    }

    public event Action OnExpired;

    // ── Runtime state ─────────────────────────────────────────────────────────

    private float     remaining;
    private float     totalDuration;
    private bool      isInitialized;
    private GameTimer gameTimer;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        if (freezeOverlay != null)
        {
            freezeOverlay.alpha = 0f;
            freezeOverlay.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isInitialized == false) return;
        if (remaining <= 0f) return;

        remaining -= Time.deltaTime;

        if (remaining <= 0f)
        {
            remaining = 0f;
            Deactivate();
        }
    }

    private void OnDestroy()
    {
        // Đảm bảo timer được resume nếu object bị destroy đột ngột
        if (isInitialized && gameTimer != null)
        {
            gameTimer.Resume();
        }
    }

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>Kích hoạt freeze. Gọi từ FreezeTimeEffectDefinition.ApplyEffect().</summary>
    public void Initialize(float duration, GameTimer timer)
    {
        remaining     = duration;
        totalDuration = duration;
        isInitialized = true;
        gameTimer     = timer;

        // Chỉ dừng GameTimer — không đụng timeScale
        if (gameTimer != null)
        {
            gameTimer.FreezeTime();
        }
        else
        {
            Debug.LogWarning("[FreezeTimeEffect] GameTimer is null — timer sẽ không bị dừng.");
        }

        // Bật overlay trước, alpha = 0, rồi fade in
        if (freezeOverlay != null)
        {
            DOTween.Kill(freezeOverlay);
            freezeOverlay.alpha = 0f;
            freezeOverlay.gameObject.SetActive(true);
            freezeOverlay.DOFade(1f, fadeInDuration);
        }

        // Enable particle rồi play — duration được sync với freeze duration, trừ startLifetime
        // để particle ngừng emit đúng lúc freeze hết (các particle đang bay sẽ tắt đúng thời điểm)
        // Stop Action = Disable tự tắt khi hết
        if (freezeParticle != null)
        {
            var main = freezeParticle.main;
            main.duration = Mathf.Max(0f, duration - main.startLifetime.constantMax);

            freezeParticle.gameObject.SetActive(true);
            freezeParticle.Play();
        }

        Debug.Log("[FreezeTimeEffect] Initialized — duration=" + duration + "s. Timer paused.");
    }

    /// <summary>Extend duration khi dùng lần 2 trong khi còn active.</summary>
    public void ExtendDuration(float additionalTime)
    {
        remaining     += additionalTime;
        totalDuration += additionalTime;

        // Restart particle với duration mới, trừ startLifetime
        if (freezeParticle != null)
        {
            var main = freezeParticle.main;
            main.duration = Mathf.Max(0f, remaining - main.startLifetime.constantMax);

            freezeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            freezeParticle.gameObject.SetActive(true);
            freezeParticle.Play();
        }

        Debug.Log("[FreezeTimeEffect] Extended by " + additionalTime + "s. Remaining: " + remaining.ToString("F1") + "s.");
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private void Deactivate()
    {
        isInitialized = false;

        // Resume timer
        if (gameTimer != null)
        {
            gameTimer.Resume();
        }

        // Fade overlay ra, sau khi xong thì disable
        if (freezeOverlay != null)
        {
            DOTween.Kill(freezeOverlay);
            freezeOverlay.DOFade(0f, fadeOutDuration)
                .OnComplete(() => freezeOverlay.gameObject.SetActive(false));
        }

        if (OnExpired != null)
        {
            OnExpired.Invoke();
            OnExpired = null;
        }

        Debug.Log("[FreezeTimeEffect] Deactivated — timer resumed.");
    }

    /// <summary>
    /// Tắt overlay ngay lập tức không có animation.
    /// Gọi từ GameplayController.Cleanup() khi về MainMenu hoặc restart.
    /// </summary>
    public void ForceDeactivate()
    {
        if (!isInitialized) return;

        isInitialized = false;

        DOTween.Kill(freezeOverlay);

        if (freezeOverlay != null)
        {
            freezeOverlay.alpha = 0f;
            freezeOverlay.gameObject.SetActive(false);
        }

        // Stop particle ngay lập tức
        if (freezeParticle != null && freezeParticle.gameObject.activeSelf)
        {
            freezeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            freezeParticle.gameObject.SetActive(false);
        }

        if (OnExpired != null)
        {
            OnExpired.Invoke();
            OnExpired = null;
        }

        Debug.Log("[FreezeTimeEffect] ForceDeactivated.");
    }
}
