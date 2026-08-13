using UnityEngine;

/// <summary>
/// Hiển thị trạng thái của 1 clock indicator trong TimeUpPopup.
/// Gắn lên mỗi clock GameObject (+20s, +40s, +60s).
///
/// Hierarchy:
///   ClockObject
///   ├── EnableIcon   (bật khi lần hồi sinh này đã được dùng)
///   └── DisableIcon  (bật khi chưa dùng — trạng thái mặc định)
/// </summary>
public class ClockIndicator : MonoBehaviour
{
    [SerializeField] private GameObject enableIcon;
    [SerializeField] private GameObject disableIcon;

    private void Awake()
    {
        SetUsed(false);
    }

    /// <summary>
    /// true  → enableIcon bật (lần hồi sinh này đã được dùng).
    /// false → disableIcon bật (chưa dùng).
    /// </summary>
    public void SetUsed(bool used)
    {
        if (enableIcon  != null) enableIcon.SetActive(used);
        if (disableIcon != null) disableIcon.SetActive(!used);
    }

    private void OnValidate()
    {
        if (enableIcon  == null) Debug.LogWarning("[ClockIndicator] enableIcon is not assigned.",  this);
        if (disableIcon == null) Debug.LogWarning("[ClockIndicator] disableIcon is not assigned.", this);
    }
}
