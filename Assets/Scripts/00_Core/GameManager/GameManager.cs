using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public GameState CurrentState { get; private set; } = GameState.None;

    public event Action<GameState, GameState> OnStateChanged;

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        GameState previous = CurrentState;
        CurrentState = newState;

        Debug.Log($"Game State : {previous} -> {CurrentState}");

        OnStateChanged?.Invoke(previous, CurrentState);
    }

    public bool IsState(GameState state)
    {
        return CurrentState == state;
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            // Save khi app pause (mobile minimize, alt-tab, etc.)
            SaveManager.Instance?.Save();
            Debug.Log("[GameManager] OnApplicationPause — saved.");
        }
    }

    private void OnApplicationQuit()
    {
        // Save ĐỒNG BỘ khi tắt app — PHẢI chờ hoàn tất trước khi quit
        // Dùng SaveSynchronous() thay vì Save().GetAwaiter().GetResult()
        // vì UniTask.GetAwaiter().GetResult() có thể throw nếu task chưa complete
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Save();
            Debug.Log("[GameManager] OnApplicationQuit — saved.");
        }
    }
}
