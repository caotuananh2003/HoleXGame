using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

/// <summary>
/// Quản lý toàn bộ kích thước hole: scale, radius, grow, detect, swallow.
/// Compose HoleDetector và SwallowHandler (cùng GameObject).
/// Events:
///   OnScoreAdded(int)  — mỗi khi nuốt 1 object
///   OnGrown(float)     — mỗi khi hole lớn lên, truyền scale mới
/// </summary>
[RequireComponent(typeof(HoleDetector))]
[RequireComponent(typeof(SwallowHandler))]
public class HoleSizeController : MonoBehaviour
{
    [Header("Visuals name (trong hierarchy)")]
    [Tooltip("Tên GameObject chứa mesh và collider của hole. Mặc định = 'Visuals'.")]
    [SerializeField] private string visualsName = "Visuals";

    // ── Public state ──────────────────────────────────────────────────────────

    public float Scale  { get; private set; }
    public float Radius { get; private set; }

    public event Action<int>   OnScoreAdded;
    public event Action<float> OnGrown;

    private Transform        visuals;
    private CapsuleCollider  detectionCapsule;

    #region Inject
    private HoleDetector     holeDetector;
    private SwallowHandler   swallowHandler;

    [Inject]
    private void Construct(HoleDetector holeDetector, SwallowHandler swallowHandler)
    {
        this.holeDetector = holeDetector;
        this.swallowHandler = swallowHandler;
    }
    #endregion

    private readonly List<Rigidbody> victims = new();

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Tự tìm visuals qua hierarchy
        visuals          = transform.Find(visualsName);
        detectionCapsule = GetComponentInChildren<CapsuleCollider>();

        if (visuals == null)
            Debug.LogWarning($"[HoleSizeController] Không tìm thấy child '{visualsName}'. " +
                             "Kiểm tra tên GameObject Visuals trong hierarchy.");

        if (detectionCapsule == null)
            Debug.LogWarning("[HoleSizeController] Không tìm thấy CapsuleCollider trong children. " +
                             "Kiểm tra HoleFill có CapsuleCollider chưa.");

        // Inject shared victims list
        holeDetector?.Initialize(victims, Radius);
        swallowHandler.Initialize(victims, Scale);

        // Forward events — dùng named method để unsubscribe đúng
        if (swallowHandler != null)
            swallowHandler.OnObjectSwallowed += ForwardScore;
    }

    private void OnDestroy()
    {
        if (swallowHandler != null)
            swallowHandler.OnObjectSwallowed -= ForwardScore;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Tăng scale hole lên 1 bậc. Gọi từ HoleController khi đủ điểm.</summary>
    public void GrowHole()
    {
        Scale++;
        Radius++;
        ApplyScale();
        holeDetector.SetRadius(Radius);
        swallowHandler.SetScale(Scale);
        OnGrown?.Invoke(Scale);
    }

    // ── Trigger relay ─────────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        swallowHandler?.HandleTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        swallowHandler?.HandleTriggerExit(other);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    //private void ApplyScale()
    //{
    //    if (visuals != null)
    //    {
    //        visuals.localScale    = new Vector3(Scale, Scale, Scale);
    //        visuals.localPosition = new Vector3(0f, -Scale / 2f - 0.49f, 0f);
    //    }

    //    if (detectionCapsule != null)
    //    {
    //        detectionCapsule.center = new Vector3(0f, -1f - Scale / 2f, 0f);
    //        detectionCapsule.radius = Scale / 2f;
    //    }
    //}

    private void ApplyScale()
    {
        if (visuals != null)
        {
            visuals.localScale = new Vector3(Scale, 1f, Scale);
            //visuals.localPosition = new Vector3(0f, -1f, 0f);
        }

        if (detectionCapsule != null)
        {
            //detectionCapsule.center = new Vector3(0f, -1f, 0f);
            detectionCapsule.radius = Scale / 2f;
        }
    }

    private void ForwardScore(int pts) => OnScoreAdded?.Invoke(pts);

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
}
