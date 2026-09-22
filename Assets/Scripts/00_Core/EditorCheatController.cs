using UnityEngine;
using UnityEngine.InputSystem;

public class EditorCheatController : MonoBehaviour
{
#if UNITY_EDITOR
    [Tooltip("ItemDatabase để cheat dùng item theo phím 1-4.")]
    [SerializeField] private ItemDatabase itemDatabase;

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.rKey.wasPressedThisFrame) ResetSaveData();
        if (Keyboard.current.nKey.wasPressedThisFrame) CheatWin();

        if (Keyboard.current.digit1Key.wasPressedThisFrame) CheatUseItem(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) CheatUseItem(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) CheatUseItem(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) CheatUseItem(3);
    }

    private void ResetSaveData()
    {
        if (SaveManager.Instance == null) { Debug.LogWarning("[EditorCheatController] SaveManager.Instance is null."); return; }

        // Yêu cầu giữ Shift+R để tránh bấm nhầm
        if (!Keyboard.current.leftShiftKey.isPressed && !Keyboard.current.rightShiftKey.isPressed)
        {
            Debug.Log("[EditorCheatController] Giữ Shift+R để xóa save data.");
            return;
        }

        SaveManager.Instance.DeleteSaveData();
        Debug.Log("[EditorCheatController] [Shift+R] Save data đã xóa.");
    }

    private void CheatWin()
    {
        if (GameplayController.Instance == null) { Debug.LogWarning("[EditorCheatController] GameplayController not found."); return; }
        Debug.Log("[EditorCheatController] [N] Cheat Win!");
        GameplayController.Instance.CheatWin();
    }

    private void CheatUseItem(int index)
    {
        if (itemDatabase == null)
        {
            Debug.LogWarning("[EditorCheatController] itemDatabase chưa được gán.");
            return;
        }

        if (index < 0 || index >= itemDatabase.Items.Count)
        {
            Debug.LogWarning("[EditorCheatController] Item index " + index + " out of range (database có " + itemDatabase.Items.Count + " items).");
            return;
        }

        ItemDefinition item = itemDatabase.Items[index];
        if (item == null)
        {
            Debug.LogWarning("[EditorCheatController] Item tại index " + index + " là null.");
            return;
        }

        if (ItemManager.Instance == null)
        {
            Debug.LogWarning("[EditorCheatController] ItemManager.Instance is null.");
            return;
        }

        ItemManager.Instance.CheatUseItem(item);
    }
#endif
}
