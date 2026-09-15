using System;
using UnityEngine;

/// <summary>
/// Đếm ngược thời gian gameplay.
/// Gắn vào một GameObject trong GameplayScene.
/// Fire Event OnTick mỗi FixedUpdate để báo time còn lại
/// Fire Event OnTimeUp 1 lần khi hết giờ
/// Có các hàm PrepareTimer(), StartTimer(), StopTimer(), ResetTimer().
/// </summary>
public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private float totalTime;
    private float remaining;
    private bool  running;
    private bool  waitingForFirstInput;

    public event Action<float> OnTick;   // Fired mỗi FixedUpdate khi running.
    public event Action        OnTimeUp; // Fired một lần khi hết giờ.

    public float Remaining            => remaining;
    public bool  IsRunning            => running;
    public bool  WaitingForFirstInput => waitingForFirstInput;

    /// <summary>
    /// Khởi tạo timer với duration nhưng KHÔNG bắt đầu đếm.
    /// Timer đứng yên, hiển thị đúng thời gian, chờ NotifyFirstInput().
    /// Gọi từ GameplayController.InitLevel().
    /// </summary>
    public void PrepareTimer(float duration)
    {
        totalTime            = duration;
        remaining            = duration;
        running              = false;
        waitingForFirstInput = true;

        // Fire ngay để UI hiện đúng giá trị ban đầu
        OnTick?.Invoke(remaining);
    }

    /// <summary>
    /// Kick-start timer khi có input đầu tiên.
    /// Idempotent — gọi nhiều lần chỉ có tác dụng lần đầu.
    /// Gọi từ InputManager.
    /// </summary>
    public void NotifyFirstInput()
    {
        if (!waitingForFirstInput) return;

        waitingForFirstInput = false;
        running              = true;

        Debug.Log("[GameTimer] First input — timer started.");
    }

    public void StartTimer()
    {
        remaining            = totalTime;
        running              = true;
        waitingForFirstInput = false;
    }

    public void StartTimer(float duration)
    {
        totalTime            = duration;
        remaining            = duration;
        running              = true;
        waitingForFirstInput = false;
    }

    public void StopTimer()
    {
        running              = false;
        waitingForFirstInput = false;
    }

    /// <summary>Tạm dừng timer — dùng cho FreezeTime effect.</summary>
    public void FreezeTime()
    {
        running = false;
    }

    /// <summary>Pause toàn bộ timer — dùng khi game pause.</summary>
    public void Pause()
    {
        running = false;
    }

    /// <summary>Tiếp tục timer sau FreezeTime() hoặc Pause() — không reset remaining.</summary>
    public void Resume()
    {
        if (remaining > 0f && !waitingForFirstInput)
            running = true;
    }

    public void ResetTimer()
    {
        remaining            = totalTime;
        running              = false;
        waitingForFirstInput = false;
    }

    /// <summary>
    /// Cộng thêm giây vào thời gian còn lại và tiếp tục chạy.
    /// Dùng khi người chơi hồi sinh bằng ads hoặc currency.
    /// </summary>
    public void AddTime(float seconds)
    {
        remaining += seconds;

        // Nếu đang chờ first input thì không tự chạy
        if (!waitingForFirstInput)
            running = true;
    }

    private void FixedUpdate()
    {
        if (!running) return;

        remaining -= Time.fixedDeltaTime;
        OnTick?.Invoke(remaining);

        if (remaining <= 0f)
        {
            remaining            = 0f;
            running              = false;
            waitingForFirstInput = false;
            OnTimeUp?.Invoke();
        }
    }
}
