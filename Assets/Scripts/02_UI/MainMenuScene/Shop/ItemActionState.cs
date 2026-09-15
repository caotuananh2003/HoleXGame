/// <summary>
/// Trạng thái của một item trong Shop:
/// - NotOwned  → chưa sở hữu, hiện nút "Xem ADS để mua"
/// - Owned     → đã sở hữu, hiện nút "Select"
/// - Equipped  → đang trang bị, hiện text "Đã trang bị" (button disabled)
/// </summary>
public enum ItemActionState
{
    NotOwned,
    Owned,
    Equipped,
}
