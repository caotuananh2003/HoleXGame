using System;
using UnityEngine;

/// <summary>
/// Đếm ngược thời gian gameplay.
/// Gắn vào một GameObject trong GameplayScene.
/// Fire Event OnTick mỗi FixedUpdate để báo time còn lại
/// Fire Event OnTimeUp 1 lần khi hết giờ
/// Có các hàm StartTimer(), StopTimer(), ResetTimer().
/// </summary>
public class GameTimer : MonoBehaviour
{
    [SerializeField] private float totalTime = 120f;

    private float remaining;
    private bool running;

    public event Action<float> OnTick; // Fired mỗi frame khi timer đang chạy. Truyền ra số giây còn lại.

    public event Action OnTimeUp; // Fired một lần khi hết giờ.

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

    private void FixedUpdate()
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
