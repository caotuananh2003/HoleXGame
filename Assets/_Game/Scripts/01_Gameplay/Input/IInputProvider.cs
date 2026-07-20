/// <summary>
/// Abstraction layer cho input di chuyển hole.
/// Implement bởi TouchJoystickInput (mobile) hoặc bất kỳ provider nào khác
/// (gamepad, keyboard, AI bot...) mà không cần sửa HoleController.
/// </summary>
public interface IInputProvider
{
    /// <summary>Hướng di chuyển đã normalize (-1..1 mỗi trục).</summary>
    UnityEngine.Vector2 Direction { get; }

    /// <summary>
    /// Độ mạnh kéo, clamp về 0..1.
    /// 0 = đứng yên, 1 = kéo tối đa.
    /// </summary>
    float Magnitude { get; }

    /// <summary>True khi ngón tay / nút đang giữ.</summary>
    bool IsActive { get; }

    /// <summary>True trong đúng frame ngón tay vừa nhả.</summary>
    bool WasReleasedThisFrame { get; }
}
