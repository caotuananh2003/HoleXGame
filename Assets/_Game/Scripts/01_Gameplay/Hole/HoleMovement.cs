using UnityEngine;

/// <summary>
/// Di chuyển hole theo input đã xử lý. Không tự đọc input.
/// Thiết lập visible cho directionRing
/// </summary>
/// 
public class HoleMovement : MonoBehaviour
{
    [SerializeField] private float speed = 1.5f;

    [SerializeField]  private Transform directionRing;
    private bool      inputEnabled;

    private void Awake()
    {
        inputEnabled = false;
        if (directionRing == null)
            Debug.LogWarning("[HoleMovement] Không tìm thấy 'DirectionRing'");
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;

        if (!enabled)
            SetRingVisible(false);
    }

    /// <summary>Gọi khi người dùng nhả tay.</summary>
    public void OnInputReleased()
    {
        SetRingVisible(false);
    }

    public void Move(Vector2 direction, float magnitude) // Di chuyển
    {
        if (!inputEnabled || magnitude <= 0f) return;

        Vector3 worldDir = new Vector3(direction.x, 0f, direction.y);
        transform.position += worldDir * speed * magnitude * Time.deltaTime;

        SetRingVisible(true);
        if (directionRing != null)
            directionRing.rotation = Quaternion.LookRotation(worldDir) * Quaternion.Euler(90f, 0f, 0f);
    }

    private void SetRingVisible(bool visible)
    {
        directionRing.gameObject.SetActive(visible);
    }
}
