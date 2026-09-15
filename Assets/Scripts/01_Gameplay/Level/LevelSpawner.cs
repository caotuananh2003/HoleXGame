using UnityEngine;

/// <summary>
/// Chịu trách nhiệm duy nhất: Instantiate và Destroy level prefab.
///
/// Workflow:
///   1. Designer tạo Level_001.prefab chứa Map + Obstacles đã xếp sẵn.
///   2. LevelDefinition.LevelPrefab trỏ vào prefab đó.
///   3. GameplayController gọi SpawnLevel() sau LoadLevel().
///   4. GameplayController gọi Cleanup() khi kết thúc level / restart.
///
/// LevelRoot là Transform cha để chứa instance —
/// kéo một GameObject rỗng tên "LevelRoot" vào Inspector.
/// Nếu để trống thì spawn tại scene root.
///
/// Single Responsibility: không biết gì về gameplay logic.
/// </summary>
public class LevelSpawner : MonoBehaviour
{
    [Header("Spawn Config")]
    [Tooltip("Transform cha chứa level instance. Để trống = spawn tại scene root.")]
    [SerializeField] private Transform levelRoot;

    // ── State ─────────────────────────────────────────────────────────────────

    /// <summary>Instance đang active. Null nếu chưa spawn hoặc đã Cleanup.</summary>
    public GameObject SpawnedInstance { get; private set; }

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>
    /// Instantiate level prefab từ LevelDefinition vào LevelRoot.
    /// Nếu đang có instance cũ thì Cleanup trước.
    /// </summary>
    public void SpawnLevel(LevelDefinition levelDefinition)
    {
        if (levelDefinition == null)
        {
            Debug.LogError("[LevelSpawner] LevelDefinition is null — cannot spawn.");
            return;
        }

        if (levelDefinition.LevelPrefab == null)
        {
            Debug.LogError($"[LevelSpawner] '{levelDefinition.LevelName}' chưa có LevelPrefab — cannot spawn.");
            return;
        }

        // Dọn instance cũ nếu còn
        if (SpawnedInstance != null)
        {
            Debug.LogWarning("[LevelSpawner] SpawnLevel() gọi khi đang có instance cũ — tự Cleanup.");
            Cleanup();
        }

        SpawnedInstance = Instantiate(levelDefinition.LevelPrefab, levelRoot);

        // Đảm bảo prefab spawn tại origin của LevelRoot (hoặc world origin nếu không có root)
        SpawnedInstance.transform.localPosition = Vector3.zero;
        SpawnedInstance.transform.localRotation = Quaternion.identity;
        SpawnedInstance.transform.localScale    = Vector3.one;

        Debug.Log($"[LevelSpawner] Spawned '{levelDefinition.LevelName}'.");
    }

    /// <summary>
    /// Destroy instance hiện tại và reset state.
    /// </summary>
    public void Cleanup()
    {
        if (SpawnedInstance == null) return;

        Destroy(SpawnedInstance);
        SpawnedInstance = null;

        Debug.Log("[LevelSpawner] Cleaned up level instance.");
    }

    // =========================================================================
    // Lifecycle
    // =========================================================================

    private void OnDestroy()
    {
        // Không gọi Cleanup() ở đây — khi scene unload thì Unity tự destroy hết.
        // Chỉ null ref để tránh stale pointer.
        SpawnedInstance = null;
    }
}
