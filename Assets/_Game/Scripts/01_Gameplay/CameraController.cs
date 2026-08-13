using DG.Tweening;
using UnityEngine;

/// <summary>
/// Dịch chuyển local position của Camera khi hole grow.
/// Gắn lên Camera — là child của Player.
///
/// Mỗi lần OnGrown fire:
///   localPosition.y += cameraStepY  (default +10)
///   localPosition.z -= cameraStepZ  (default  +5)
///
/// Dùng localPosition vì Camera là child của Player.
/// HoleSizeController tự tìm qua GetComponentInParent — không cần kéo Inspector.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Grow Step")]
    [SerializeField] private float cameraStepY  = 10f;
    [SerializeField] private float cameraStepZ  = 5f;
    [SerializeField] private float growDuration = 0.5f;
    [SerializeField] private Ease  growEase     = Ease.OutCubic;

    private HoleSizeController holeSizeController;

    private void Start()
    {
        holeSizeController = GetComponentInParent<HoleSizeController>();

        if (holeSizeController == null)
        {
            Debug.LogWarning("[CameraController] Không tìm thấy HoleSizeController trên parent.");
            return;
        }

        holeSizeController.OnGrown += HandleGrown;
    }

    private void OnDestroy()
    {
        if (holeSizeController != null)
            holeSizeController.OnGrown -= HandleGrown;

        DOTween.Kill(transform);
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private void HandleGrown(float newRadius)
    {
        Vector3 target = transform.localPosition
                         + new Vector3(0f, cameraStepY, -cameraStepZ);

        transform.DOLocalMove(target, growDuration)
            .SetEase(growEase);
    }
}
