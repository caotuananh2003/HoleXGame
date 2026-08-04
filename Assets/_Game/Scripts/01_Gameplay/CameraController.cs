using System.Collections;
using UnityEngine;

/// <summary>
/// Zoom camera ra khi hole lớn lên.
/// </summary>
public class CameraController : MonoBehaviour
{
    [SerializeField] private float zoomDuration = 0.5f;

    private HoleSizeController sizeController;
    private Vector3            defaultLocalPos;
    private Vector3            step;

    private void Awake()
    {
        sizeController = FindAnyObjectByType<HoleSizeController>();

        if (sizeController == null)
            Debug.LogWarning("[CameraController] Không tìm thấy HoleSizeController trên parent.");
    }

    private void Start()
    {
        sizeController.OnGrown += OnHoleGrown;
    }

    private void OnHoleGrown(float newScale)
    {
        defaultLocalPos = transform.localPosition;
        step = newScale > 0f ? defaultLocalPos / newScale : defaultLocalPos;

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

    private void OnDestroy()
    {
        if (sizeController != null)
            sizeController.OnGrown -= OnHoleGrown;
    }
}
