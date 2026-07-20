using System;
using UnityEngine;

/// <summary>
/// Đếm ngược thời gian gameplay.
/// Gắn vào một GameObject trong GameplayScene.
/// </summary>
public class GameTimer : MonoBehaviour
{
    [SerializeField] private float totalTime = 120f;

    private float remaining;
    private bool running;

    /// <summary>Fired mỗi frame khi timer đang chạy. Truyền ra số giây còn lại.</summary>
    public event Action<float> OnTick;

    /// <summary>Fired một lần khi hết giờ.</summary>
    public event Action OnTimeUp;

    public float Remaining => remaining;
    public bool IsRunning => running;

    public void StartTimer()
    {
        remaining = totalTime;
        running = true;
    }

    public void StopTimer()
    {
        running = false;
    }

    public void ResetTimer()
    {
        remaining = totalTime;
        running = false;
    }

    private void Update()
    {
        if (!running) return;

        remaining -= Time.deltaTime;
        OnTick?.Invoke(remaining);

        if (remaining <= 0f)
        {
            remaining = 0f;
            running = false;
            OnTimeUp?.Invoke();
        }
    }
}
