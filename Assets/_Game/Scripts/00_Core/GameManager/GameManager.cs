using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
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
            // Save...
        }
    }

    private void OnApplicationQuit()
    {
        // Save...
    }
}
