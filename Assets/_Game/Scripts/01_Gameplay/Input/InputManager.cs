using UnityEngine;
using VContainer;

/// <summary>
/// Đọc input từ IInputProvider và chuyển tiếp cho HoleController mỗi frame.
/// Cả IInputProvider lẫn HoleController đều inject qua VContainer — không SerializeField.
/// </summary>
public class InputManager : MonoBehaviour
{
    private IInputProvider inputProvider;
    private HoleController holeController;

    [Inject]
    private void Construct(IInputProvider inputProvider, HoleController holeController)
    {
        this.inputProvider  = inputProvider;
        this.holeController = holeController;
    }

    private void Update()
    {
        if (inputProvider == null || holeController == null) return;

        if (inputProvider.IsActive)
            holeController.ApplyInput(inputProvider.Direction, inputProvider.Magnitude);

        if (inputProvider.WasReleasedThisFrame)
            holeController.OnInputReleased();
    }
}
