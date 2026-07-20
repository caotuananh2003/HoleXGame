using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Service quản lý nhiều ObjectPool theo prefab key.
/// Đăng ký vào GameplayLifetimeScope để inject vào LevelSpawner, HoleSizeController, v.v.
///
/// Cách dùng:
///   // Lấy object
///   GameObject obj = poolService.Get(prefab, position, rotation);
///
///   // Trả về pool
///   poolService.Return(prefab, obj);
///
///   // Trả tất cả về pool khi kết thúc ván
///   poolService.ReturnAll();
/// </summary>
public class ObjectPoolService : MonoBehaviour
{
    [Tooltip("Số instance tạo sẵn cho mỗi prefab khi pool được tạo lần đầu.")]
    [SerializeField] private int prewarmCount = 10;

    // Key = prefab source, Value = pool của prefab đó
    private readonly Dictionary<GameObject, ObjectPool<PooledObject>> pools = new();

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Lấy một instance của prefab từ pool.
    /// Prefab phải có component PooledObject (tự động thêm nếu thiếu).
    /// </summary>
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        ObjectPool<PooledObject> pool = GetOrCreatePool(prefab);
        PooledObject item = pool.Get();
        item.transform.SetPositionAndRotation(position, rotation);
        item.Initialize(prefab, this);
        return item.gameObject;
    }

    /// <summary>
    /// Trả một instance về pool theo prefab key.
    /// </summary>
    public void Return(GameObject prefab, GameObject instance)
    {
        if (!pools.TryGetValue(prefab, out ObjectPool<PooledObject> pool)) return;
        PooledObject pooled = instance.GetComponent<PooledObject>();
        if (pooled != null)
            pool.Return(pooled);
    }

    /// <summary>
    /// Trả tất cả active instances về pool (dùng khi restart/end game).
    /// </summary>
    public void ReturnAll()
    {
        foreach (ObjectPool<PooledObject> pool in pools.Values)
            pool.ReturnAll();
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private ObjectPool<PooledObject> GetOrCreatePool(GameObject prefab)
    {
        if (pools.TryGetValue(prefab, out ObjectPool<PooledObject> existing))
            return existing;

        // Tạo một transform con để giữ inactive objects gọn gàng
        Transform root = new GameObject($"Pool_{prefab.name}").transform;
        root.SetParent(transform, false);

        ObjectPool<PooledObject> pool = new ObjectPool<PooledObject>(
            factory: () =>
            {
                GameObject go     = Instantiate(prefab, root);
                PooledObject item = go.GetComponent<PooledObject>()
                                 ?? go.AddComponent<PooledObject>();
                return item;
            },
            poolRoot: root,
            prewarm: prewarmCount
        );

        pools[prefab] = pool;
        return pool;
    }
}
