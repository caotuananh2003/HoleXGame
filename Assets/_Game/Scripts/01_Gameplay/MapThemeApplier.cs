using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

/// <summary>
/// Resolve và lưu MapTheme mặc định khi GameplayScene load.
///
/// Logic:
///   1. Đọc SaveManager.Data.equippedMapThemeId
///   2. Nếu rỗng → lấy item đầu tiên trong MapThemeDatabase làm default và lưu.
///   3. Apply visual — TODO khi MapThemeDefinition có Material hoặc Prefab ground.
///
/// Gắn lên bất kỳ GameObject trong GameplayScene.
/// </summary>
public class MapThemeApplier : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("PlayerProfile SO — cung cấp MapThemeDatabase.")]
    [SerializeField] private PlayerProfile playerProfile;

    // ── Dependency ────────────────────────────────────────────────────────────
    private SaveManager saveManager;

    [Inject]
    private void Construct(SaveManager saveManager)
    {
        this.saveManager = saveManager;
    }

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Start()
    {
        if (playerProfile == null)
        {
            Debug.LogWarning("[MapThemeApplier] playerProfile is not assigned.");
            return;
        }

        if (saveManager?.PlayerData == null)
        {
            Debug.LogWarning("[MapThemeApplier] SaveManager.Data is null.");
            return;
        }

        ResolveDefaultIfNeeded();

        // TODO: ApplyCurrentTheme() khi MapThemeDefinition có Material hoặc Prefab ground.
    }

    // =========================================================================
    // Internal
    // =========================================================================

    /// <summary>
    /// Nếu equippedMapThemeId rỗng, gán id của item đầu tiên làm default.
    /// </summary>
    private void ResolveDefaultIfNeeded()
    {
        if (!string.IsNullOrEmpty(saveManager.PlayerData.equippedMapThemeId)) return;

        MapThemeDatabase db = playerProfile.MapThemeDatabase;
        if (db == null || db.MapThemeDefinition.Count == 0)
        {
            Debug.LogWarning("[MapThemeApplier] MapThemeDatabase rỗng — không thể set default.");
            return;
        }

        string defaultId = db.MapThemeDefinition[0].Id;
        saveManager.PlayerData.equippedMapThemeId = defaultId;
        saveManager.Save().Forget();

        Debug.Log($"[MapThemeApplier] equippedMapThemeId rỗng — set default: {defaultId}");
    }
}
