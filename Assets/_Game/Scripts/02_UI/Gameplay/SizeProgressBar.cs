using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Thanh tiến độ hiển thị progress đến milestone grow tiếp theo.
///
/// Subscribe HoleController.OnProgressChanged và OnLevelUp.
/// HoleController tìm qua GetComponentInParent hoặc FindAnyObjectByType.
///
/// Hierarchy gợi ý (trong GameplayCanvas):
///   SizeProgressBar
///   ├── Slider          (Slider component — Interactable=false, Min=0, Max=1)
///   └── LevelText       (TMP_Text — "Level 1")
///
/// Mỗi khi score thay đổi:   Slider.value = progress (0..1)
/// Mỗi khi grow (level up):  Slider.value reset về 0, LevelText cập nhật
/// </summary>
public class SizeProgressBar : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private Slider    progressSlider;
    [SerializeField] private TMP_Text  levelText;

    // ── Dependency ────────────────────────────────────────────────────────────
    private HoleController holeController;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Start()
    {
        holeController = FindAnyObjectByType<HoleController>();

        if (holeController == null)
        {
            Debug.LogWarning("[SizeProgressBar] Không tìm thấy HoleController.");
            return;
        }

        holeController.OnProgressChanged += HandleProgressChanged;
        holeController.OnLevelUp         += HandleLevelUp;

        // Trạng thái ban đầu
        SetProgress(0f);
        SetLevelText(1);
    }

    private void OnDestroy()
    {
        if (holeController != null)
        {
            holeController.OnProgressChanged -= HandleProgressChanged;
            holeController.OnLevelUp         -= HandleLevelUp;
        }
    }

    // =========================================================================
    // Handlers
    // =========================================================================

    /// <summary>Cập nhật slider theo progress hiện tại.</summary>
    private void HandleProgressChanged(int currentLevel, float progress)
    {
        SetProgress(progress);
        SetLevelText(currentLevel);
    }

    /// <summary>Khi grow: reset slider về 0 và cập nhật level text.</summary>
    private void HandleLevelUp(int newLevel)
    {
        SetProgress(0f);
        SetLevelText(newLevel);
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private void SetProgress(float value)
    {
        if (progressSlider != null)
            progressSlider.value = value;
    }

    private void SetLevelText(int level)
    {
        if (levelText != null)
            levelText.text = $"Level {level}";
    }
}
