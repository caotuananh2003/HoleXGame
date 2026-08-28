using UnityEngine;
using UnityEngine.InputSystem;

public class EditorCheatController : MonoBehaviour
{
    private void Update()
    {
#if UNITY_EDITOR
        if (Keyboard.current == null) return;
        if (Keyboard.current.rKey.wasPressedThisFrame) ResetSaveData();
        if (Keyboard.current.nKey.wasPressedThisFrame) CheatWin();
#endif
    }

    private void ResetSaveData()
    {
        if (SaveManager.Instance == null) { Debug.LogWarning("[EditorCheatController] SaveManager.Instance is null."); return; }
        SaveManager.Instance.DeleteSaveData();
        Debug.Log("[EditorCheatController] [R] Save data đã xóa.");
    }

    private void CheatWin()
    {
        if (GameplayController.Instance == null) { Debug.LogWarning("[EditorCheatController] GameplayController not found."); return; }
        Debug.Log("[EditorCheatController] [N] Cheat Win!");
        GameplayController.Instance.CheatWin();
    }
}
