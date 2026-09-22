using UnityEngine;

/// <summary>
/// Áp dụng MapTheme (material) lên MeshRenderer của Ground object trong gameplay.
/// Hoạt động tương tự HoleSkinApplier.
///
/// Setup:
///   1. Gắn script này vào bất kỳ GameObject nào trong GameplayScene.
///   2. Kéo MeshRenderer của Ground vào field Ground Renderer.
///   3. Kéo PlayerProfile vào field Player Profile.
/// </summary>
public class MapThemeApplier : MonoBehaviour
{
    [SerializeField] private PlayerProfile playerProfile;

    [Tooltip("MeshRenderer của Ground (Plane) trong gameplay. MapMaterial sẽ được gán vào đây.")]
    [SerializeField] private MeshRenderer groundRenderer;

    private void Start()
    {
        Apply();
    }

    /// <summary>
    /// Áp dụng theme hiện tại từ save data.
    /// Gọi từ Start() hoặc từ GameplayController.StartLevel() sau khi save đã load xong.
    /// </summary>
    public void Apply()
    {
        if (playerProfile == null) { Debug.LogWarning("[MapThemeApplier] playerProfile is not assigned."); return; }
        if (SaveManager.Instance?.PlayerData == null) { Debug.LogWarning("[MapThemeApplier] PlayerData is null."); return; }

        ResolveDefaultIfNeeded();
        ApplyCurrentTheme();
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private void ResolveDefaultIfNeeded()
    {
        if (!string.IsNullOrEmpty(SaveManager.Instance.PlayerData.equippedMapThemeId)) return;

        MapThemeDatabase db = playerProfile.MapThemeDatabase;
        if (db == null || db.MapThemeDefinition.Count == 0) { Debug.LogWarning("[MapThemeApplier] MapThemeDatabase rỗng."); return; }

        string defaultId = db.MapThemeDefinition[0].Id;
        SaveManager.Instance.PlayerData.equippedMapThemeId = defaultId;
        SaveManager.Instance.Save();
    }

    private void ApplyCurrentTheme()
    {
        if (groundRenderer == null) { Debug.LogWarning("[MapThemeApplier] groundRenderer is not assigned."); return; }

        string id = SaveManager.Instance.PlayerData.equippedMapThemeId;
        if (string.IsNullOrEmpty(id)) return;

        MapThemeDefinition def = playerProfile.MapThemeDatabase?.GetById(id);
        if (def == null)         { Debug.LogWarning($"[MapThemeApplier] Không tìm thấy id='{id}'."); return; }
        if (def.MapMaterial == null) { Debug.LogWarning($"[MapThemeApplier] MapMaterial của '{id}' chưa được gán."); return; }

        groundRenderer.material = def.MapMaterial;
    }
}
