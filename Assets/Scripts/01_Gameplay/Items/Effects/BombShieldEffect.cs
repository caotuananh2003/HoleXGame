using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Gắn lên ShieldParent — parent của Shield child object.
/// ShieldParent luôn active. Chỉ Shield child bị bật/tắt theo effect.
///
/// - Kích hoạt : Shield child SetActive(true) → play grow clip
/// - Hết thời gian : play end clip → đợi clip xong → Shield child SetActive(false)
/// </summary>
public class BombShieldEffect : MonoBehaviour, ITimedEffect
{
    // ── Static accessor ───────────────────────────────────────────────────────

    private static BombShieldEffect activeInstance;

    public static bool IsActive
    {
        get
        {
            if (activeInstance == null) return false;
            if (activeInstance.remaining > 0f) return true;
            return false;
        }
    }

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
    private bool      isDeactivating;

    [Header("References")]
    [Tooltip("Shield child object (mesh, animation, light).")]
    [SerializeField] private GameObject shieldChild;

    // Tự resolve từ shieldChild trong Awake — không cần kéo thả
    private Animation     shieldAnimation;
    private AnimationClip growClip;
    private AnimationClip endClip;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        if (activeInstance != null && activeInstance != this)
        {
            activeInstance.ForceDeactivate();
        }

        if (shieldChild != null)
        {
            shieldChild.SetActive(false);
        }

        ResolveFromChild();
    }

    /// <summary>
    /// Lấy Animation component và clips từ shieldChild.
    /// shieldChild được kéo vào Inspector, còn lại tự resolve.
    /// </summary>
    private void ResolveFromChild()
    {
        if (shieldChild == null)
        {
            Debug.LogWarning("[BombShieldEffect] shieldChild chưa được gán.");
            return;
        }

        shieldAnimation = shieldChild.GetComponent<Animation>();

        if (shieldAnimation == null)
        {
            Debug.LogWarning("[BombShieldEffect] Không tìm thấy Animation component trên Shield child.");
            return;
        }

        int index = 0;
        foreach (AnimationState state in shieldAnimation)
        {
            if (index == 0) growClip = state.clip;
            if (index == 1) endClip  = state.clip;
            index++;
        }

        if (growClip == null) Debug.LogWarning("[BombShieldEffect] Không tìm thấy grow clip (Element 0).");
        if (endClip  == null) Debug.LogWarning("[BombShieldEffect] Không tìm thấy end clip (Element 1).");
    }

    private void OnDestroy()
    {
        if (activeInstance == this)
        {
            activeInstance = null;
        }
    }

    private void Update()
    {
        if (isInitialized == false) return;
        if (isDeactivating) return;
        if (remaining <= 0f) return;

        remaining -= Time.deltaTime;

        if (remaining <= 0f)
        {
            remaining = 0f;
            BeginDeactivate();
        }
    }

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>Kích hoạt shield. Gọi từ BombShieldEffectDefinition.ApplyEffect().</summary>
    public void Initialize(float duration)
    {
        isDeactivating = false;
        remaining      = duration;
        totalDuration  = duration;
        activeInstance = this;
        isInitialized  = true;

        if (shieldChild != null)
        {
            shieldChild.SetActive(true);
        }

        if (shieldAnimation != null && growClip != null)
        {
            shieldAnimation.Play(growClip.name);
        }
        else
        {
            Debug.LogWarning("[BombShieldEffect] Animation hoặc growClip chưa được resolve.");
        }

        Debug.Log("[BombShieldEffect] Initialized — duration=" + duration + "s.");
    }

    /// <summary>Extend duration khi dùng lần 2 trong khi còn active.</summary>
    public void ExtendDuration(float additionalTime)
    {
        remaining += additionalTime;
        Debug.Log("[BombShieldEffect] Extended by " + additionalTime + "s. Remaining: " + remaining.ToString("F1") + "s.");
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private void BeginDeactivate()
    {
        isDeactivating = true;

        if (activeInstance == this)
        {
            activeInstance = null;
        }

        PlayEndClipThenDisableAsync().Forget();
    }

    private async UniTaskVoid PlayEndClipThenDisableAsync()
    {
        if (shieldAnimation != null && endClip != null)
        {
            shieldAnimation.Play(endClip.name);
            await UniTask.Delay(
                TimeSpan.FromSeconds(endClip.length),
                cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        if (shieldChild != null)
        {
            shieldChild.SetActive(false);
        }

        isInitialized  = false;
        isDeactivating = false;

        if (OnExpired != null)
        {
            OnExpired.Invoke();
            OnExpired = null;
        }

        Debug.Log("[BombShieldEffect] Deactivated.");
    }

    /// <summary>Tắt ngay lập tức không chờ animation — dùng khi có shield mới thay thế.</summary>
    private void ForceDeactivate()
    {
        isDeactivating = true;
        isInitialized  = false;
        remaining      = 0f;

        if (activeInstance == this)
        {
            activeInstance = null;
        }

        if (shieldChild != null)
        {
            shieldChild.SetActive(false);
        }

        if (OnExpired != null)
        {
            OnExpired.Invoke();
            OnExpired = null;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (remaining > 0f)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, transform.lossyScale.x * 0.5f);
        }
    }
#endif
}
