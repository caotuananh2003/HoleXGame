using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tạo lực hút thẳng đứng (Vector3.down) cho các obstacle đang nằm trong trigger của hố.
/// Lực này cộng thêm vào gravity sẵn có — obstacle sẽ rơi nhanh hơn so với rơi tự nhiên.
///
/// Setup:
///   Gắn script này lên cùng GameObject có SphereCollider (Is Trigger) của Player.
///   SphereCollider đó đã được dùng bởi HoleColliderController — dùng chung, không cần tạo thêm.
/// </summary>
public class HoleSuctionEffect : MonoBehaviour
{
    [Tooltip("Gia tốc hút xuống (m/s²). Cộng thêm vào gravity. Giá trị dương = hút xuống.")]
    [SerializeField] private float suctionAcceleration = 15f;

    // Track các Rigidbody đang trong trigger để apply lực mỗi FixedUpdate.
    // HashSet: O(1) add/remove, không bị duplicate.
    private readonly HashSet<Rigidbody> _inTrigger = new();

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null || rb.isKinematic) return;

        _inTrigger.Add(rb);
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        _inTrigger.Remove(rb);
    }

    private void FixedUpdate()
    {
        if (_inTrigger.Count == 0) return;

        // Cache để tránh modify collection trong vòng lặp
        _toRemove.Clear();

        foreach (Rigidbody rb in _inTrigger)
        {
            // Guard: object bị destroy hoặc deactivate trong lúc còn trong trigger
            if (rb == null || !rb.gameObject.activeInHierarchy)
            {
                _toRemove.Add(rb);
                continue;
            }

            rb.AddForce(Vector3.down * suctionAcceleration, ForceMode.Acceleration);
        }

        foreach (Rigidbody rb in _toRemove)
            _inTrigger.Remove(rb);
    }

    // Buffer tái sử dụng để tránh GC alloc trong FixedUpdate
    private readonly List<Rigidbody> _toRemove = new();

    // =========================================================================
    // Cleanup
    // =========================================================================

    private void OnDisable()
    {
        // Khi hole bị disable (restart level), xóa hết để tránh apply lực cho
        // object không còn trong trigger
        _inTrigger.Clear();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Visualize vùng trigger trong Scene view
        SphereCollider sc = GetComponent<SphereCollider>();
        if (sc == null) return;

        Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, sc.radius * transform.lossyScale.x);
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, sc.radius * transform.lossyScale.x);
    }
#endif
}
