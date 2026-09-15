using UnityEngine;

/// <summary>
/// Tạo hiệu ứng "hố" 3D bằng cách kết hợp PolygonCollider2D và MeshCollider.
///
/// Player đã có SphereCollider (Is Trigger) sẵn — script này không tự quản lý trigger collider.
///
/// Cách hoạt động:
///   - Start(): tắt trước va chạm giữa tất cả obstacle hiện có và generatedMeshCollider.
///   - OnTriggerEnter: obstacle vào vùng hố → tắt GroundCollider, bật GeneratedMeshCollider.
///   - OnTriggerExit:  obstacle rời vùng hố → bật lại GroundCollider, tắt GeneratedMeshCollider.
///   - FixedUpdate: khi transform di chuyển → cập nhật lại lỗ 2D và generate lại MeshCollider 3D.
///
/// SetRadius() được gọi từ HoleSizeController khi hole grow — trigger RefreshHole().
/// </summary>
public class HoleColliderController : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D hole2DCollider;
    [SerializeField] private PolygonCollider2D ground2DCollider;
    [SerializeField] private MeshCollider      generatedMeshCollider;
    [SerializeField] private Collider          groundCollider;

    [Tooltip("Hệ số scale của hole2DCollider so với transform.localScale.")]
    [SerializeField] private float initialScale = 0.5f;

    // ── Runtime ───────────────────────────────────────────────────────────────
    private Mesh generatedMesh;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Start()
    {
        // Tắt va chạm giữa tất cả obstacle hiện có và generatedMeshCollider.
        // Obstacle ở ngoài hố chỉ cần đứng trên groundCollider — không cần va chạm với mesh có lỗ.
        GameObject[] allGOs = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        foreach (var go in allGOs)
        {
            if (go.layer == LayerMask.NameToLayer("Obstacles"))
            {
                Collider col = go.GetComponent<Collider>();
                if (col != null)
                    Physics.IgnoreCollision(col, generatedMeshCollider, true);
            }
        }

        RefreshHole();
    }

    private void FixedUpdate()
    {
        if (!transform.hasChanged) return;

        transform.hasChanged = false;
        RefreshHole();
    }

    private void OnDestroy()
    {
        if (generatedMesh != null)
            Destroy(generatedMesh);
    }

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>
    /// Cập nhật radius của hố. Gọi từ HoleSizeController mỗi khi hole grow.
    /// </summary>
    public void SetRadius(float radius)
    {
        RefreshHole();
    }

    // =========================================================================
    // Trigger — bật/tắt collision với Ground vs GeneratedMesh
    // =========================================================================

    private void OnTriggerEnter(Collider other)
    {
        Physics.IgnoreCollision(other, groundCollider,         true);
        Physics.IgnoreCollision(other, generatedMeshCollider,  false);
    }

    private void OnTriggerExit(Collider other)
    {
        Physics.IgnoreCollision(other, groundCollider,         false);
        Physics.IgnoreCollision(other, generatedMeshCollider,  true);
    }

    // =========================================================================
    // Internal — hole generation
    // =========================================================================

    private void RefreshHole()
    {
        if (hole2DCollider == null || ground2DCollider == null || generatedMeshCollider == null)
            return;

        hole2DCollider.transform.position   = new Vector2(transform.position.x, transform.position.z);
        hole2DCollider.transform.localScale = transform.localScale * initialScale;

        MakeHole2D();
        Make3DMeshCollider();
    }

    private void MakeHole2D()
    {
        Vector2[] pointPositions = hole2DCollider.GetPath(0);

        for (int i = 0; i < pointPositions.Length; i++)
            pointPositions[i] = hole2DCollider.transform.TransformPoint(pointPositions[i]);

        ground2DCollider.pathCount = 2;
        ground2DCollider.SetPath(1, pointPositions);
    }

    private void Make3DMeshCollider()
    {
        if (generatedMesh != null)
            Destroy(generatedMesh);

        generatedMesh = ground2DCollider.CreateMesh(true, true);
        generatedMeshCollider.sharedMesh = generatedMesh;
    }
}
