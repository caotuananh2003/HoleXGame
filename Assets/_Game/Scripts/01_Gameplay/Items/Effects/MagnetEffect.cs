using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime component gắn vào hole GameObject khi MagnetEffect active.
/// Mỗi frame tìm all objects swallowable trong radius, AddForce hướng về hole center.
/// Tự destroy sau duration.
///
/// Layer "Swallowable" = 9 (theo SwallowHandler convention).
/// </summary>
public class MagnetEffect : MonoBehaviour
{
    private const int SwallowableLayer = 9;

    private float radius;
    private float force;
    private float remaining;

    private LayerMask swallowableLayerMask;

    // Reuse buffer để tránh GC alloc mỗi frame
    private Collider[] overlapBuffer = new Collider[128];

    public void Initialize(float radius, float force, float duration)
    {
        this.radius = radius;
        this.force = force;
        this.remaining = duration;

        swallowableLayerMask = 1 << SwallowableLayer;

        Debug.Log($"[MagnetEffect] Initialized — duration={duration}s.");
    }

    private void FixedUpdate()
    {
        remaining -= Time.fixedDeltaTime;

        if (remaining <= 0f)
        {
            Debug.Log("[MagnetEffect] Duration expired — destroying component.");
            Destroy(this);
            return;
        }

        ApplyMagnetForce();
    }

    private void ApplyMagnetForce()
    {
        Vector3 center = transform.position;

        // OverlapSphere tìm all colliders trong radius thuộc layer Swallowable
        int count = Physics.OverlapSphereNonAlloc(center, radius, overlapBuffer, swallowableLayerMask);

        for (int i = 0; i < count; i++)
        {
            Collider col = overlapBuffer[i];
            if (col == null || !col.gameObject.activeInHierarchy) continue;

            Rigidbody rb = col.attachedRigidbody;
            if (rb == null || rb.isKinematic) continue;

            // Tính direction từ object → hole center
            Vector3 direction = (center - rb.position).normalized;

            // AddForce hướng về hole
            rb.AddForce(direction * force, ForceMode.Force);
        }
    }

    private void OnDestroy()
    {
        Debug.Log("[MagnetEffect] Destroyed.");
    }

#if UNITY_EDITOR
    // Visualize radius trong Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
