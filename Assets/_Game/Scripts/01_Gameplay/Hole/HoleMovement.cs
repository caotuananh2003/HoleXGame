using UnityEngine;

/// <summary>
/// Di chuyển hole theo input đã xử lý. Không tự đọc input.
/// Tự tìm DirectionRing qua hierarchy — không cần SerializeField.
///
/// Hierarchy mong đợi:
///   Player (root — nơi gắn script này)
///   └── Visuals
///       └── DirectionRing
/// </summary>
public class HoleMovement : MonoBehaviour
{
    [SerializeField] private float speed = 1.5f;

    private Transform directionRing;
    private bool      inputEnabled;

    private void Awake()
    {
        // Tự tìm DirectionRing trong hierarchy
        directionRing = transform.Find("Visuals/DirectionRing");

        if (directionRing == null)
            Debug.LogWarning("[HoleMovement] Không tìm thấy 'Visuals/DirectionRing'. " +
                             "Kiểm tra tên GameObject trong hierarchy.");
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        if (!enabled)
            SetRingVisible(false);
    }

    /// <summary>
    /// direction: hướng normalize. magnitude: 0..1.
    /// </summary>
    public void Move(Vector2 direction, float magnitude)
    {
        if (!inputEnabled || magnitude <= 0f) return;

        Vector3 worldDir = new Vector3(direction.x, 0f, direction.y);
        transform.position += worldDir * speed * magnitude * Time.deltaTime;

        SetRingVisible(true);
        if (directionRing != null)
            directionRing.rotation = Quaternion.LookRotation(worldDir) * Quaternion.Euler(90f, 0f, 0f);
    }

    /// <summary>Gọi khi người dùng nhả tay.</summary>
    public void OnInputReleased()
    {
        SetRingVisible(false);
    }

    private void SetRingVisible(bool visible)
    {
        if (directionRing != null)
            directionRing.gameObject.SetActive(visible);
    }
}
