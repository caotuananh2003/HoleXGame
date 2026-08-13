using UnityEngine;

/// <summary>
/// Di chuyển hole theo input đã xử lý. Không tự đọc input.
///
/// DirectionArrow:
///   - Enable  khi có input (magnitude > 0)
///   - Disable khi input = 0, nhả tay, hoặc input bị tắt
///   - Rotation quay theo hướng di chuyển
///
/// FakeGround đồng bộ position theo Player trong LateUpdate.
///
/// Không có SerializeField — nhận config qua Init() từ HoleController.
/// </summary>
public class HoleMovement : MonoBehaviour
{
    private float     speed;
    private Transform directionArrow;
    private Transform fakeGround;

    private bool inputEnabled;
    private bool isInitialized;

    // =========================================================================
    // Init — gọi từ HoleController.Awake()
    // =========================================================================

    public void Init(float speed, Transform directionArrow, Transform fakeGround)
    {
        this.speed          = speed;
        this.directionArrow = directionArrow;
        this.fakeGround     = fakeGround;

        isInitialized = true;

        if (directionArrow == null)
            Debug.LogWarning("[HoleMovement] directionArrow is null — check HoleController Inspector.");
        if (fakeGround == null)
            Debug.LogWarning("[HoleMovement] fakeGround is null — check HoleController Inspector.");

        // DirectionArrow ẩn từ đầu — chỉ hiện khi có input
        SetArrowVisible(false);
    }

    // =========================================================================
    // Public API
    // =========================================================================

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;

        if (!enabled)
            SetArrowVisible(false);
    }

    /// <summary>Gọi khi người dùng nhả tay.</summary>
    public void OnInputReleased()
    {
        SetArrowVisible(false);
    }

    /// <summary>Gọi mỗi frame từ HoleController.ApplyInput().</summary>
    public void Move(Vector2 direction, float magnitude)
    {
        if (!isInitialized || !inputEnabled)
        {
            SetArrowVisible(false);
            return;
        }

        if (magnitude <= 0f)
        {
            SetArrowVisible(false);
            return;
        }

        Vector3 worldDir = new Vector3(direction.x, 0f, direction.y);
        transform.position += worldDir * speed * magnitude * Time.deltaTime;

        // Có input → hiển thị arrow và quay đúng hướng
        SetArrowVisible(true);
        if (directionArrow != null)
            directionArrow.rotation = Quaternion.LookRotation(worldDir) * Quaternion.Euler(0f, 0f, 0f);
    }

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void LateUpdate()
    {
        // Đồng bộ FakeGround về đúng vị trí Player sau khi physics resolve xong.
        if (fakeGround != null)
            fakeGround.position = transform.position;
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private void SetArrowVisible(bool visible)
    {
        if (directionArrow == null) return;
        directionArrow.gameObject.SetActive(visible);
    }
}
