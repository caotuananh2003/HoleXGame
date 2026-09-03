using DG.Tweening;
using UnityEngine;

/// <summary>
/// Camera follow player với smooth damping.
/// Khi hole grow, offset (y, z) tăng dần để camera lùi ra xa hơn.
///
/// Setup:
///   - Gắn script này lên Camera object (KHÔNG cần là child của Player).
///   - Kéo Player GameObject vào field [player] trong Inspector.
///
/// Offset ban đầu:
///   offsetY = initialOffsetY  (default 15)
///   offsetZ = initialOffsetZ  (default 12)
///
/// Mỗi lần OnGrown fire:
///   offsetY += cameraStepY   (default 5)
///   offsetZ += cameraStepZ   (default 3)
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject player;

    [Header("Follow")]
    [Tooltip("Thời gian smooth khi camera đuổi theo player (giây). Nhỏ = nhanh hơn.")]
    [SerializeField] private float followSmoothTime = 0.15f;

    [Header("Initial Offset")]
    [SerializeField] private float initialOffsetY = 10f;
    [SerializeField] private float initialOffsetZ = 5f;

    [Header("Grow Step")]
    [SerializeField] private float cameraStepY  = 5f;
    [SerializeField] private float cameraStepZ  = 3f;
    [SerializeField] private float growDuration = 0.5f;
    [SerializeField] private Ease  growEase     = Ease.OutCubic;

    // Offset hiện tại từ player (world space, chỉ dịch Y và Z)
    private Vector3 currentOffset;

    // Offset đang được animate tới (target của DOTween)
    private Vector3 targetOffset;

    // Velocity cho SmoothDamp
    private Vector3 followVelocity;

    private HoleSizeController holeSizeController;
    private bool isReady;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Start()
    {
        if (player == null)
        {
            Debug.LogWarning("[CameraController] Player chưa được assign.", this);
            return;
        }

        holeSizeController = player.GetComponent<HoleSizeController>();
        if (holeSizeController == null)
        {
            Debug.LogWarning("[CameraController] Không tìm thấy HoleSizeController trên Player.", player);
            return;
        }

        currentOffset = new Vector3(0f, initialOffsetY, -initialOffsetZ);
        targetOffset  = currentOffset;

        // Đặt camera ngay vào vị trí đúng ngay frame đầu, không chờ smooth
        transform.position = player.transform.position + currentOffset;

        holeSizeController.OnGrown += HandleGrown;
        isReady = true;
    }

    private void LateUpdate()
    {
        if (!isReady || player == null) return;

        Vector3 desiredPosition = player.transform.position + currentOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref followVelocity,
            followSmoothTime
        );
    }

    private void OnDestroy()
    {
        if (holeSizeController != null)
            holeSizeController.OnGrown -= HandleGrown;

        DOTween.Kill(this);
    }

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>
    /// Reset camera về offset ban đầu và snap ngay về vị trí player.
    /// Gọi từ GameplayController trước mỗi màn mới.
    /// </summary>
    public void ResetToInitial()
    {
        DOTween.Kill(this);
        followVelocity = Vector3.zero;
        currentOffset  = new Vector3(0f, initialOffsetY, -initialOffsetZ);
        targetOffset   = currentOffset;

        if (player != null)
            transform.position = player.transform.position + currentOffset;
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private void HandleGrown(float newRadius)
    {
        targetOffset += new Vector3(0f, cameraStepY, -cameraStepZ);

        // Animate currentOffset → targetOffset để camera lùi mượt
        DOTween.To(
                () => currentOffset,
                v  => currentOffset = v,
                targetOffset,
                growDuration
            )
            .SetEase(growEase)
            .SetId(this); // dùng SetId(this) để Kill đúng khi OnDestroy
    }
}
