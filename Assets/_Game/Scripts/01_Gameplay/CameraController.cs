using System.Collections;
using UnityEngine;

/// <summary>
/// Zoom camera ra khi hole lớn lên.
/// Gắn vào Main Camera — Camera phải là CHILD của Player GameObject.
/// Dùng GetComponentInParent để tìm HoleSizeController — không cần SerializeField.
/// </summary>
public class CameraController : MonoBehaviour
{
    [SerializeField] private float zoomDuration = 0.2f;

    private HoleSizeController sizeController;
    private Vector3            defaultLocalPos;
    private Vector3            step;
    private bool               initialized;

    private void Awake()
    {
        // Camera là child của Player → tìm HoleSizeController trên parent
        sizeController = GetComponentInParent<HoleSizeController>();

        if (sizeController == null)
            Debug.LogWarning("[CameraController] Không tìm thấy HoleSizeController trên parent. " +
                             "Đảm bảo Camera là child của Player GameObject.");
    }

    private void Start()
    {
        if (sizeController != null)
            sizeController.OnGrown += OnHoleGrown;
    }

    private void OnDestroy()
    {
        if (sizeController != null)
            sizeController.OnGrown -= OnHoleGrown;
    }

    private void OnHoleGrown(float newScale)
    {
        if (!initialized)
        {
            defaultLocalPos = transform.localPosition;
            step            = newScale > 0f ? defaultLocalPos / newScale : defaultLocalPos;
            initialized     = true;
        }

        StopAllCoroutines();
        StartCoroutine(AnimateZoom(newScale));
    }

    private IEnumerator AnimateZoom(float targetScale)
    {
        Vector3 startPos  = transform.localPosition;
        Vector3 targetPos = step * targetScale;

        float t = 0f;
        while (t < zoomDuration)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t / zoomDuration);
            yield return null;
        }

        transform.localPosition = targetPos;
    }
}
