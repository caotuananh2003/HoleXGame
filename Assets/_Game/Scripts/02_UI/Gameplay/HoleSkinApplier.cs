using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

/// <summary>
/// Apply HoleSkin visual lên Player/Visuals/HoleSkin khi GameplayScene load.
///
/// Logic:
///   1. Đọc SaveManager.Data.equippedHoleSkinId
///   2. Nếu rỗng (chưa từng chọn) → lấy item đầu tiên trong HoleSkinDatabase làm default,
///      ghi lại vào SaveManager và lưu.
///   3. Tìm HoleSkinDefinition theo id → apply Sprite lên SpriteRenderer của HoleSkin.
///
/// Gắn lên Player root. Kéo các ref trong Inspector.
/// </summary>
public class HoleSkinApplier : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Transform Player/Visuals/HoleSkin — có SpriteRenderer cần cập nhật.")]
    [SerializeField] private SpriteRenderer holeSkinRenderer;

    [Tooltip("PlayerProfile SO — cung cấp HoleSkinDatabase.")]
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
        if (!ValidateRefs()) return;

        ResolveDefaultIfNeeded();
        ApplyCurrentSkin();
    }

    // =========================================================================
    // Internal
    // =========================================================================

    /// <summary>
    /// Nếu equippedHoleSkinId rỗng, gán id của item đầu tiên trong database làm default.
    /// </summary>
    private void ResolveDefaultIfNeeded()
    {
        if (!string.IsNullOrEmpty(saveManager.PlayerData.equippedHoleSkinId)) return;

        HoleSkinDatabase db = playerProfile.HoleSkinDatabase;
        if (db == null || db.HoleDefinition.Count == 0)
        {
            Debug.LogWarning("[HoleSkinApplier] HoleSkinDatabase rỗng — không thể set default.");
            return;
        }

        string defaultId = db.HoleDefinition[0].Id;
        saveManager.PlayerData.equippedHoleSkinId = defaultId;
        saveManager.Save().Forget();

        Debug.Log($"[HoleSkinApplier] equippedHoleSkinId rỗng — set default: {defaultId}");
    }

    /// <summary>
    /// Tìm HoleSkinDefinition theo equippedHoleSkinId và apply lên SpriteRenderer.
    /// </summary>
    private void ApplyCurrentSkin()
    {
        string id = saveManager.PlayerData.equippedHoleSkinId;
        if (string.IsNullOrEmpty(id)) return;

        HoleSkinDefinition def = playerProfile.HoleSkinDatabase?.GetById(id);
        if (def == null)
        {
            Debug.LogWarning($"[HoleSkinApplier] Không tìm thấy HoleSkinDefinition với id='{id}'.");
            return;
        }

        holeSkinRenderer.sprite = def.Icon;
        Debug.Log($"[HoleSkinApplier] Applied HoleSkin: {def.DisplayName}");
    }

    private bool ValidateRefs()
    {
        if (holeSkinRenderer == null)
        {
            Debug.LogWarning("[HoleSkinApplier] holeSkinRenderer is not assigned.");
            return false;
        }

        if (playerProfile == null)
        {
            Debug.LogWarning("[HoleSkinApplier] playerProfile is not assigned.");
            return false;
        }

        if (saveManager?.PlayerData == null)
        {
            Debug.LogWarning("[HoleSkinApplier] SaveManager.Data is null.");
            return false;
        }

        return true;
    }
}
