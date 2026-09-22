using System;
using UnityEngine;

/// <summary>
/// Gắn lên SpeedBoosterEffect — child của Player.
/// Object này có 3 ParticleSystem con:
///   - Activation  : play ngay, stop action = disable tự tắt
///   - Continuous  : play ngay, duration = thời gian effect, start delay = 0.2 (cài sẵn)
///                   stop action = disable tự tắt khi hết duration
///   - End         : play sau khi Continuous xong (khi remaining <= 0)
///                   stop action = disable tự tắt
/// </summary>
public class SpeedBoosterEffect : MonoBehaviour
{
    [Header("Particles")]
    [SerializeField] private ParticleSystem activationParticle;
    [SerializeField] private ParticleSystem continuousParticle;
    [SerializeField] private ParticleSystem endParticle;

    // ── Runtime state ─────────────────────────────────────────────────────────

    private float         remaining;
    private bool          isActive;
    private float         speedBonus;
    private Func<float>   getBaseSpeed;
    private Action<float> applySpeed;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Update()
    {
        if (!isActive) return;

        remaining -= Time.deltaTime;

        if (remaining <= 0f)
            Deactivate();
    }

    private void OnDestroy()
    {
        isActive = false;
    }

    // =========================================================================
    // Public API
    // =========================================================================

    public void Activate(float bonus, float duration,
                         Func<float> getBaseSpeedFunc, Action<float> applySpeedAction)
    {
        speedBonus   = bonus;
        getBaseSpeed = getBaseSpeedFunc;
        applySpeed   = applySpeedAction;

        if (isActive)
        {
            // Extend: cộng thêm duration, restart Continuous với duration mới
            remaining += duration;
            PlayContinuous(remaining);
            Debug.Log($"[SpeedBoosterEffect] Extended — remaining={remaining:F1}s.");
        }
        else
        {
            remaining = duration;
            isActive  = true;

            // Bật cả 2 cùng lúc — Continuous tự delay 0.2s vì đã cài sẵn trong inspector
            PlayActivation();
            PlayContinuous(duration);
            Debug.Log($"[SpeedBoosterEffect] Activated — +{bonus} for {duration}s.");
        }

        applySpeed(getBaseSpeed() + speedBonus);
    }

    /// <summary>Tắt ngay lập tức — gọi khi Cleanup/Restart.</summary>
    public void ForceDeactivate()
    {
        if (!isActive) return;

        isActive  = false;
        remaining = 0f;

        // Stop Continuous nếu đang chạy (End không cần play khi force)
        if (continuousParticle != null && continuousParticle.gameObject.activeSelf)
        {
            continuousParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            continuousParticle.gameObject.SetActive(false);
        }

        applySpeed?.Invoke(getBaseSpeed?.Invoke() ?? 0f);
        Debug.Log("[SpeedBoosterEffect] ForceDeactivated.");
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private void Deactivate()
    {
        isActive  = false;
        remaining = 0f;

        // Continuous đã tự disable qua stop action khi hết duration
        // Play End ngay sau khi Continuous kết thúc
        PlayEnd();

        applySpeed?.Invoke(getBaseSpeed?.Invoke() ?? 0f);
        Debug.Log("[SpeedBoosterEffect] Expired — speed restored.");
    }

    private void PlayActivation()
    {
        if (activationParticle == null) return;
        activationParticle.gameObject.SetActive(true);
        activationParticle.Play();
    }

    private void PlayContinuous(float duration)
    {
        if (continuousParticle == null) return;

        continuousParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Set duration cho Continuous và tất cả child ParticleSystem
        SetDurationRecursive(continuousParticle, duration);

        continuousParticle.gameObject.SetActive(true);
        continuousParticle.Play();
    }

    private void SetDurationRecursive(ParticleSystem ps, float duration)
    {
        var main  = ps.main;
        main.duration = duration;

        foreach (Transform child in ps.transform)
        {
            if (child.TryGetComponent(out ParticleSystem childPs))
                SetDurationRecursive(childPs, duration);
        }
    }

    private void PlayEnd()
    {
        if (endParticle == null) return;
        endParticle.gameObject.SetActive(true);
        endParticle.Play();
    }
}
