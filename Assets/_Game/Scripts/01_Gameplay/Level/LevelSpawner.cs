using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pure logic class — spawn objects vào scene theo LevelData config.
/// Dùng ObjectPoolService để lấy instances (không Instantiate trực tiếp).
/// Không kế thừa MonoBehaviour — không có Unity lifecycle.
/// </summary>
public class LevelSpawner
{
    private readonly ObjectPoolService poolService;

    public LevelSpawner(ObjectPoolService poolService)
    {
        this.poolService = poolService;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Spawn toàn bộ objects theo LevelData.
    /// Trả về danh sách GameObjects đã spawn (để LevelManager track).
    /// </summary>
    public List<GameObject> Spawn(LevelData data)
    {
        List<GameObject> spawned = new List<GameObject>();

        if (data == null || data.spawnEntries == null || data.spawnEntries.Length == 0)
        {
            Debug.LogWarning("[LevelSpawner] LevelData is null or has no spawn entries.");
            return spawned;
        }

        // Build weighted prefab list
        List<GameObject> weightedPrefabs = BuildWeightedList(data.spawnEntries);
        if (weightedPrefabs.Count == 0) return spawned;

        // Track vị trí đã spawn để tránh overlap
        List<Vector3> usedPositions = new List<Vector3>();

        Bounds mapBounds = data.GetMapBounds();
        float  halfX     = data.mapSize.x / 2f - 1f;   // margin 1 unit từ mép
        float  halfZ     = data.mapSize.y / 2f - 1f;

        for (int i = 0; i < data.totalSpawnCount; i++)
        {
            GameObject prefab = weightedPrefabs[Random.Range(0, weightedPrefabs.Count)];
            if (prefab == null) continue;

            Vector3 pos = FindSpawnPosition(halfX, halfZ, usedPositions, data.minSpacingBetweenObjects);
            usedPositions.Add(pos);

            Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            GameObject obj = poolService.Get(prefab, pos, rot);
            spawned.Add(obj);
        }

        return spawned;
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private List<GameObject> BuildWeightedList(SpawnEntry[] entries)
    {
        List<GameObject> list = new List<GameObject>();
        foreach (SpawnEntry entry in entries)
        {
            if (entry?.prefab == null) continue;
            for (int w = 0; w < entry.weight; w++)
                list.Add(entry.prefab);
        }
        return list;
    }

    private Vector3 FindSpawnPosition(float halfX, float halfZ, List<Vector3> used, float minSpacing)
    {
        const int maxAttempts = 30;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 candidate = new Vector3(
                Random.Range(-halfX, halfX),
                0f,
                Random.Range(-halfZ, halfZ));

            if (IsPositionValid(candidate, used, minSpacing))
                return candidate;
        }

        // Fallback: posit random bất kỳ nếu không tìm được chỗ trống
        return new Vector3(
            Random.Range(-halfX, halfX),
            0f,
            Random.Range(-halfZ, halfZ));
    }

    private bool IsPositionValid(Vector3 candidate, List<Vector3> used, float minSpacing)
    {
        float minSqr = minSpacing * minSpacing;
        foreach (Vector3 pos in used)
        {
            if ((candidate - pos).sqrMagnitude < minSqr)
                return false;
        }
        return true;
    }
}
