using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Dynamic joystick dùng Unity New Input System.
/// - Chạm màn hình: joystick UI xuất hiện ngay tại điểm chạm.
/// - Kéo: tính direction + magnitude, giới hạn trong maxRadius pixel.
/// - Nhả: joystick ẩn, magnitude = 0.
///
/// Gắn vào một GameObject trong GameplayScene (cùng Canvas với joystick UI).
/// Wire joystickRoot, knob qua Inspector.
/// </summary>
public class TouchJoystickInput : MonoBehaviour, IInputProvider
{
    [Header("Joystick UI")]
    [Tooltip("Root RectTransform của toàn bộ joystick (background + knob). Sẽ ẩn/hiện.")]
    [SerializeField] private RectTransform joystickRoot;

    [Tooltip("Knob (nút tròn nhỏ bên trong). Di chuyển theo ngón tay.")]
    [SerializeField] private RectTransform knob;

    [Tooltip("Bán kính tối đa (pixel) mà knob có thể rời khỏi tâm.")]
    [SerializeField] private float maxRadius = 80f;

    // ── IInputProvider ────────────────────────────────────────────────────────

    public Vector2 Direction          { get; private set; }
    public float   Magnitude          { get; private set; }
    public bool    IsActive           { get; private set; }
    public bool    WasReleasedThisFrame { get; private set; }

    // ── Private ───────────────────────────────────────────────────────────────

    private Vector2 anchorScreenPos;   // vị trí tâm joystick (screen coords)
    private Camera  uiCamera;          // null nếu Canvas là Screen Space Overlay
    private RectTransform canvasRect;  // root canvas RectTransform để tính toán vị trí chính xác

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Tìm root canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            // Lấy root canvas (tránh nested canvas)
            Canvas rootCanvas = canvas.rootCanvas;
            canvasRect = rootCanvas.GetComponent<RectTransform>();

            if (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                uiCamera = rootCanvas.worldCamera;
        }

        HideJoystick();
    }

    private void Update()
    {
        WasReleasedThisFrame = false;

        // Ưu tiên touch trước, fallback về mouse (editor/PC)
        Touchscreen touch = Touchscreen.current;
        Mouse        mouse = Mouse.current;

        if (touch != null && touch.primaryTouch.press.isPressed)
            HandleTouch(touch);
        else if (mouse != null)
            HandleMouse(mouse);
        else
            ResetInput();
    }

    // ── Input handlers ────────────────────────────────────────────────────────

    private void HandleTouch(Touchscreen touch)
    {
        if (touch.primaryTouch.press.wasPressedThisFrame)
        {
            BeginDrag(touch.primaryTouch.position.ReadValue());
        }
        else if (touch.primaryTouch.press.isPressed)
        {
            UpdateDrag(touch.primaryTouch.position.ReadValue());
        }

        if (touch.primaryTouch.press.wasReleasedThisFrame)
        {
            EndDrag();
        }
    }

    private void HandleMouse(Mouse mouse)
    {
        if (mouse.leftButton.wasPressedThisFrame)
        {
            BeginDrag(mouse.position.ReadValue());
        }
        else if (mouse.leftButton.isPressed)
        {
            UpdateDrag(mouse.position.ReadValue());
        }
        else if (mouse.leftButton.wasReleasedThisFrame)
        {
            EndDrag();
        }
        else if (!mouse.leftButton.isPressed)
        {
            ResetInput();
        }
    }

    // ── Drag logic ────────────────────────────────────────────────────────────

    private void BeginDrag(Vector2 screenPos)
    {
        anchorScreenPos = screenPos;
        IsActive        = true;

        if (joystickRoot != null)
        {
            joystickRoot.gameObject.SetActive(true);

            // Dùng canvasRect (root canvas) để convert screen → local chính xác
            RectTransform parent = canvasRect != null
                ? canvasRect
                : joystickRoot.parent as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent,
                screenPos,
                uiCamera,
                out Vector2 localPoint);

            joystickRoot.anchoredPosition = localPoint;
        }

        if (knob != null)
            knob.anchoredPosition = Vector2.zero;

        Direction = Vector2.zero;
        Magnitude = 0f;
    }

    private void UpdateDrag(Vector2 screenPos)
    {
        Vector2 delta = screenPos - anchorScreenPos;

        // Convert screen pixel delta → canvas local units (xử lý Canvas Scaler)
        if (canvasRect != null)
        {
            float scaleFactor = canvasRect.localScale.x;
            if (scaleFactor > 0f)
                delta /= scaleFactor;
        }

        float dist = delta.magnitude;

        // Clamp knob trong maxRadius
        Vector2 clampedDelta = dist > maxRadius
            ? delta.normalized * maxRadius
            : delta;

        if (knob != null)
            knob.anchoredPosition = clampedDelta;

        Direction = dist > 0.01f ? delta.normalized : Vector2.zero;
        Magnitude = Mathf.Clamp01(dist / maxRadius);
        IsActive  = true;
    }

    private void EndDrag()
    {
        WasReleasedThisFrame = true;
        ResetInput();
        HideJoystick();
    }

    private void ResetInput()
    {
        IsActive  = false;
        Direction = Vector2.zero;
        Magnitude = 0f;

        if (knob != null)
            knob.anchoredPosition = Vector2.zero;
    }

    private void HideJoystick()
    {
        if (joystickRoot != null)
            joystickRoot.gameObject.SetActive(false);
    }
}
