using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Một slot item trong item bar.
/// Hiển thị icon, label, và overlay cooldown.
/// Gắn vào mỗi slot GameObject con của item bar trong GameplayPanel.
/// </summary>
public class ItemSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image           iconImage;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private Image           cooldownOverlay;   // Image type=Filled, fillMethod=Radial360
    [SerializeField] private Button          useButton;

    private bool onCooldown;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = 0f;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Thiết lập icon và label cho slot này.
    /// </summary>
    public void Setup(Sprite icon, string label)
    {
        if (iconImage != null)
        {
            iconImage.sprite  = icon;
            iconImage.enabled = icon != null;
        }

        if (labelText != null)
            labelText.text = label;

        SetInteractable(true);
    }

    /// <summary>
    /// Bắt đầu cooldown trong `duration` giây.
    /// </summary>
    public void StartCooldown(float duration)
    {
        if (onCooldown) return;
        SetInteractable(false);
        StartCoroutine(CooldownRoutine(duration));
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private System.Collections.IEnumerator CooldownRoutine(float duration)
    {
        onCooldown = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = 1f - Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = 0f;

        onCooldown = false;
        SetInteractable(true);
    }

    private void SetInteractable(bool interactable)
    {
        if (useButton != null)
            useButton.interactable = interactable;
    }
}
