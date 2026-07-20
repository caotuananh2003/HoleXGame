using System;
using UnityEngine;

/// <summary>
/// ScriptableObject mô tả config của một level.
/// Tạo asset: Create → HoleXGame → Level Data
/// </summary>
[CreateAssetMenu(fileName = "LevelData", menuName = "HoleXGame/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Map")]
    [Tooltip("Kích thước map (XZ). Camera boundary và spawn area dùng giá trị này.")]
    public Vector2 mapSize = new Vector2(40f, 40f);

    [Header("Spawn")]
    [Tooltip("Tổng số object spawn vào scene lúc bắt đầu ván.")]
    public int totalSpawnCount = 60;

    [Tooltip("Khoảng cách tối thiểu giữa mỗi object khi spawn.")]
    public float minSpacingBetweenObjects = 1.5f;

    [Header("Spawn Entries")]
    [Tooltip("Danh sách prefab có thể spawn và trọng số xuất hiện của mỗi loại.")]
    public SpawnEntry[] spawnEntries;

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Bounds XZ của map, tâm tại (0,0,0).</summary>
    public Bounds GetMapBounds()
    {
        return new Bounds(
            Vector3.zero,
            new Vector3(mapSize.x, 100f, mapSize.y));
    }
}

/// <summary>
/// Một loại object có thể spawn, kèm trọng số.
/// Trọng số cao hơn = xuất hiện nhiều hơn.
/// </summary>
[Serializable]
public class SpawnEntry
{
    [Tooltip("Prefab object sẽ được spawn. Phải có Rigidbody và Collider với Layer = Swallowable (9).")]
    public GameObject prefab;

    [Tooltip("Trọng số xuất hiện. Ví dụ: 3 = xuất hiện gấp 3 lần so với entry weight=1.")]
    [Min(1)]
    public int weight = 1;
}
