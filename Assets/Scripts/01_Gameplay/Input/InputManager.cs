using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private TouchJoystickInput touchJoystickInput;
    [SerializeField] private HoleController     holeController;
    [SerializeField] private GameTimer          gameTimer;

    private void Update()
    {
        if (touchJoystickInput == null || holeController == null) return;

        if (touchJoystickInput.IsActive)
        {
            // Kick-start timer lần đầu tiên người dùng chạm joystick
            gameTimer?.NotifyFirstInput();

            holeController.ApplyInput(touchJoystickInput.Direction, touchJoystickInput.Magnitude);
        }

        if (touchJoystickInput.WasReleasedThisFrame)
            holeController.OnInputReleased();
    }
}
