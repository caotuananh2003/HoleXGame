using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private TouchJoystickInput touchJoystickInput;
    [SerializeField] private HoleController     holeController;

    private void Update()
    {
        if (touchJoystickInput == null || holeController == null) return;

        if (touchJoystickInput.IsActive)
            holeController.ApplyInput(touchJoystickInput.Direction, touchJoystickInput.Magnitude);

        if (touchJoystickInput.WasReleasedThisFrame)
            holeController.OnInputReleased();
    }
}
