using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Dynamic joystick dùng Unity New Input System.
/// - Chạm màn hình: joystick UI xuất hiện ngay tại điểm chạm.
/// - Kéo: tính direction + magnitude, giới hạn trong maxRadius (canvas units).
/// - Nhả: joystick ẩn, magnitude = 0.
///
/// Gắn lên JoystickCanvas (cùng GameObject với Canvas component).
/// JoystickRoot là direct child của JoystickCanvas.
/// Knob là direct child của JoystickRoot.
/// </summary>
public class TouchJoystickInput : MonoBehaviour
{
    [Header("Joystick UI")]
    [SerializeField] private RectTransform joystickRoot;
    [SerializeField] private RectTransform knob;

    [Tooltip("Bán kính tối đa (canvas units) knob có thể rời khỏi tâm.")]
    [SerializeField] private float maxRadius = 80f;

    public Vector2 Direction            { get; private set; }
    public float   Magnitude            { get; private set; }
    public bool    IsActive             { get; private set; }
    public bool    WasReleasedThisFrame { get; private set; }

    // anchorLocalPos: vị trí chạm ban đầu trong local space của Canvas
    private Vector2       anchorLocalPos;
    private Camera        uiCamera;
    private RectTransform canvasRect;

    private void Awake()
    {
        // Script gắn trên JoystickCanvas — lấy Canvas của chính object này
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            Canvas root = canvas.rootCanvas;
            canvasRect  = root.GetComponent<RectTransform>();

            // Chỉ cần uiCamera nếu KHÔNG phải Screen Space Overlay
            if (root.renderMode != RenderMode.ScreenSpaceOverlay)
                uiCamera = root.worldCamera;
        }
        else
        {
            Debug.LogWarning("[TouchJoystickInput] Không tìm thấy Canvas trên GameObject này. " +
                             "Script phải gắn lên cùng GameObject với Canvas component.");
        }

        HideJoystick();
    }

    private void Update()
    {
        WasReleasedThisFrame = false;

        Touchscreen touch = Touchscreen.current;
        Mouse        mouse = Mouse.current;

        if (touch != null && touch.primaryTouch.press.isPressed)
            HandleTouch(touch);
        else if (mouse != null)
            HandleMouse(mouse);
        else
            ResetInput();
    }

    private void HandleTouch(Touchscreen touch)
    {
        
        if (touch.primaryTouch.press.wasPressedThisFrame)
            BeginDrag(touch.primaryTouch.position.ReadValue());
        else if (touch.primaryTouch.press.isPressed)
            UpdateDrag(touch.primaryTouch.position.ReadValue());

        if (touch.primaryTouch.press.wasReleasedThisFrame)
        {
            EndDrag();
        }
    }

    private void HandleMouse(Mouse mouse)
    {
        if (mouse.leftButton.wasPressedThisFrame)
            BeginDrag(mouse.position.ReadValue());
        else if (mouse.leftButton.isPressed)
            UpdateDrag(mouse.position.ReadValue());
        else if (mouse.leftButton.wasReleasedThisFrame)
            EndDrag();
        else if (!mouse.leftButton.isPressed)
            ResetInput();
    }

    private void BeginDrag(Vector2 screenPos)
    {
        IsActive = true;

        if (canvasRect == null) return;

        // Convert screen → local space của JoystickCanvas (parent của joystickRoot)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, uiCamera, out Vector2 localPos);

        anchorLocalPos = localPos;

        if (joystickRoot != null)
        {
            joystickRoot.gameObject.SetActive(true);

            // Gán localPosition thay vì anchoredPosition để tránh lệch do anchor offset
            joystickRoot.localPosition = new Vector3(localPos.x, localPos.y, 0f);
        }

        if (knob != null)
            knob.anchoredPosition = Vector2.zero;

        Direction = Vector2.zero;
        Magnitude = 0f;
    }

    private void UpdateDrag(Vector2 screenPos)
    {
        if (canvasRect == null) return;

        // Convert vị trí ngón tay hiện tại sang local space của Canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, uiCamera, out Vector2 currentLocalPos);

        // Tính delta hoàn toàn trong local space — đúng với mọi Canvas Scaler
        Vector2 delta = currentLocalPos - anchorLocalPos;
        float   dist  = delta.magnitude;

        // Clamp knob trong maxRadius
        Vector2 clampedDelta = dist > maxRadius
            ? delta.normalized * maxRadius
            : delta;

        if (knob != null)
            knob.anchoredPosition = clampedDelta;

        Direction = dist > 0.01f ? delta.normalized : Vector2.zero;
        Magnitude = Mathf.Clamp01(dist / maxRadius);
    }

    private void EndDrag()
    {
        Debug.Log("EndDrag");
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
        Debug.Log("HideJoystickk");
        if (joystickRoot != null)
            joystickRoot.gameObject.SetActive(false);
    }
}
