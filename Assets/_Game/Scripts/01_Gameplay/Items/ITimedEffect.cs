using System;

/// <summary>
/// Interface cho các effect có thời hạn (duration-based).
/// ItemSlotUI dùng interface này để cập nhật Timer Image mà không cần biết
/// cụ thể là BombShieldEffect, MagnetEffect hay effect nào khác.
///
/// Implementing classes: BombShieldEffect, MagnetEffect.
/// </summary>
public interface ITimedEffect
{
    /// <summary>Thời gian còn lại (giây). Giảm về 0 khi hết hiệu lực.</summary>
    float Remaining { get; }

    /// <summary>Tổng thời gian khi bắt đầu (giây). Dùng để tính fillAmount = Remaining / TotalDuration.</summary>
    float TotalDuration { get; }

    /// <summary>Fire khi effect hết thời gian hoặc bị deactivate. ItemSlotUI dùng để dừng update timer.</summary>
    event Action OnExpired;
}
