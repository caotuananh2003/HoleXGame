using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tạo lực hút thẳng đứng (Vector3.down) cho các obstacle đang nằm trong trigger của hố.
///
/// Khi obstacle vào trigger:
///   - Unfreeze rotation X Y Z vĩnh viễn (không restore khi rời trigger)
///   - Apply suction force mỗi FixedUpdate cho đến khi rời trigger
///
/// Setup:
///   Gắn script này lên cùng GameObject có SphereCollider (Is Trigger) của Player.
/// </summary>
public class HoleSuctionEffect : MonoBehaviour
{
    [Tooltip("Gia tốc hút xuống (m/s²). Cộng thêm vào gravity. Giá trị dương = hút xuống.")]
    [SerializeField] private float suctionAcceleration = 5f;

    private const RigidbodyConstraints FreezeRotationAll =
        RigidbodyConstraints.FreezeRotationX |
        RigidbodyConstraints.FreezeRotationY |
        RigidbodyConstraints.FreezeRotationZ;

    private readonly HashSet<Rigidbody> _inTrigger = new();
    private readonly List<Rigidbody>    _toRemove  = new();

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null || rb.isKinematic) return;
        if (_inTrigger.Contains(rb)) return;

        _inTrigger.Add(rb);

        // Unfreeze rotation vĩnh viễn — không lưu lại để restore
        rb.constraints &= ~FreezeRotationAll;
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

        _toRemove.Clear();

        foreach (Rigidbody rb in _inTrigger)
        {
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

    private void OnDisable()
    {
        _inTrigger.Clear();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        SphereCollider sc = GetComponent<SphereCollider>();
        if (sc == null) return;

        Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, sc.radius * transform.lossyScale.x);
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, sc.radius * transform.lossyScale.x);
    }
#endif
}
