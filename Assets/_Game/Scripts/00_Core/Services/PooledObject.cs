using UnityEngine;

/// <summary>
/// Marker component — tự động thêm vào bất kỳ object nào được pool quản lý.
/// Giữ tham chiếu về prefab source và ObjectPoolService để có thể tự trả về pool.
/// </summary>
public sealed class PooledObject : MonoBehaviour
{
    private GameObject        sourcePrefab;
    private ObjectPoolService poolService;

    internal void Initialize(GameObject prefab, ObjectPoolService service)
    {
        sourcePrefab = prefab;
        poolService  = service;
    }

    /// <summary>
    /// Trả object này về pool thay vì Destroy.
    /// </summary>
    public void ReturnToPool()
    {
        if (poolService == null || sourcePrefab == null)
        {
            gameObject.SetActive(false);
            return;
        }

        poolService.Return(sourcePrefab, gameObject);
    }
}
