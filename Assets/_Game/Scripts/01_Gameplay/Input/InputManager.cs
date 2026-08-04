using UnityEngine;
using VContainer;

/// <summary>
/// Đọc input từ IInputProvider và chuyển tiếp cho HoleController mỗi frame.
/// Cả IInputProvider lẫn HoleController đều inject qua VContainer — không SerializeField.
/// </summary>
public class InputManager : MonoBehaviour
{
    private TouchJoystickInput touchJoystickInput;
    private HoleController holeController;

    [Inject]
    private void Construct(TouchJoystickInput touchJoystickInput, HoleController holeController)
    {
        this.touchJoystickInput  = touchJoystickInput;
        this.holeController = holeController;
    }

    private void Update()
    {
        if (touchJoystickInput == null || holeController == null) return;

        if (touchJoystickInput.IsActive)
            holeController.ApplyInput(touchJoystickInput.Direction, touchJoystickInput.Magnitude);

        if (touchJoystickInput.WasReleasedThisFrame)
            holeController.OnInputReleased();
    }
}
