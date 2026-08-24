using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

/// <summary>
/// Phím tắt cheat chỉ hoạt động trong Unity Editor.
/// Gắn vào bất kỳ GameObject nào tồn tại xuyên scene (ví dụ: BootstrapContext).
///
/// Phím tắt:
///   R — Xóa toàn bộ save data, reset về default.
///   N — Win game ngay lập tức (chỉ hoạt động khi đang ở GameplayScene).
/// </summary>
public class EditorCheatController : MonoBehaviour
{
    private SaveManager saveManager;

    [Inject]
    private void Construct(SaveManager saveManager)
    {
        this.saveManager = saveManager;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Keyboard.current == null) return;

        if (Keyboard.current.rKey.wasPressedThisFrame)
            ResetSaveData();

        if (Keyboard.current.nKey.wasPressedThisFrame)
            CheatWin();
#endif
    }

    private void ResetSaveData()
    {
        if (saveManager == null)
        {
            Debug.LogWarning("[EditorCheatController] saveManager is null.");
            return;
        }

        saveManager.DeleteSaveData();
        Debug.Log("[EditorCheatController] [R] Save data đã xóa — restart scene để thấy hiệu lực.");
    }

    private void CheatWin()
    {
        GameplayController gameplayController = FindAnyObjectByType<GameplayController>();

        if (gameplayController == null)
        {
            Debug.LogWarning("[EditorCheatController] [N] GameplayController không tìm thấy — chỉ dùng được trong GameplayScene.");
            return;
        }

        Debug.Log("[EditorCheatController] [N] Cheat Win!");
        gameplayController.CheatWin();
    }
}
