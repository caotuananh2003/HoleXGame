using System;
using UnityEngine;

/// <summary>
/// Quản lý kích thước hole
/// 
/// Events:
///   OnScoreAdded(int)   — mỗi khi nuốt 1 object và truyền vào số điểm (int)
///   OnGrown(float)      — mỗi khi hole lớn lên, truyền scale mới
/// </summary>
[RequireComponent(typeof(SwallowHandler))]
public class HoleSizeController : MonoBehaviour
{
    [SerializeField] private Transform visuals;

    public float Scale  { get; private set; }
    public float Radius { get; private set; }

    // Events
    public event Action<int>   OnScoreAdded; // OnScoreAdded?.Invoke(score) khi ăn obstacle. score là obstacle.Definition.ScoreValue
    public event Action<float> OnGrown; // OnGrown?.Invoke(Scale) khi Growing

    // ── Private references (tự tìm) ───────────────────────────────────────────
    private BoxCollider[] holeColliders;   // 8 BoxCollider giả lập ground quanh lỗ
    private SwallowHandler swallowHandler;

    private void Awake()
    {
        if (visuals == null)
            Debug.LogWarning("Visuals is null");

        // Tự tìm tất cả BoxCollider trong children (8 collider ground)
        holeColliders = GetComponentsInChildren<BoxCollider>();
        if (holeColliders.Length == 0)
            Debug.LogWarning("[HoleSizeController] Không tìm thấy BoxCollider nào trong children.");

        swallowHandler = FindAnyObjectByType<SwallowHandler>();
        swallowHandler.OnObjectSwallowed += ForwardScore;

        // Khởi tạo scale ban đầu
        Scale  = 1f;
        Radius = 0.5f;

        ApplyScale(Scale);
        OnGrown?.Invoke(Scale);
    }

    public void GrowHole() // Tăng scale hole lên 1 bậc. Gọi từ HoleController khi đủ điểm.
    {
        Scale *= 1.3f;
        ApplyScale(Scale);
        OnGrown?.Invoke(Scale);
    }

    public void ApplyScale(float Scale)
    {
        if (visuals != null)
        {
            Radius = 0.5f * Scale; // Do baseRadius = 0.5f
            visuals.localScale = new Vector3(Scale, 1f, Scale);

            ApplyRadius(Radius);
            Debug.Log("ApplyScale: Radius = " + Radius);
        } else
        {
            Debug.LogWarning("Visuals is null");
        }
    }

    private void ApplyRadius(float radius) // Thiết lập các collider dịch ra tạo thành (O, radius)
    {
        if (holeColliders == null) return;
        const float colliderRadius = 100f;

        foreach (BoxCollider col in holeColliders)
        {
            if (col == null) continue;
            // Kích thước collider
            col.size = new Vector3(2 * colliderRadius, 0f, 2 * colliderRadius);

            // Hướng từ tâm ra collider
            Vector3 direction = col.center.normalized;

            // Đẩy collider ra xa tâm
            col.center = direction * (colliderRadius + radius);
        }
    }

    private void ForwardScore(Obstacle obstacle)
    {
        if (obstacle != null && obstacle.ObstacleDefinition != null)
        {
            int score = obstacle.ObstacleDefinition.ScoreValue;
            OnScoreAdded?.Invoke(score);
        }
    }

    private void OnDestroy()
    {
        if (swallowHandler != null)
            swallowHandler.OnObjectSwallowed -= ForwardScore;
    }
}
