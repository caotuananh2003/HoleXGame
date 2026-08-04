using UnityEngine;

/// <summary>
/// Định nghĩa một Bundle (gói mua hàng) trong Shop.
/// Tạo asset: Definition → Bundle Definition.
///
/// Bundle là gói currency / item tổng hợp người chơi mua bằng tiền thật hoặc quảng cáo.
/// </summary>
[CreateAssetMenu(fileName = "BundleDefinition", menuName = "Definition/Bundle Definition")]
public class BundleDefinition : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    [Tooltip("Mô tả nội dung của bundle, ví dụ: '100 Coins + 5 Gems'")]
    [SerializeField] private string description;

    [Tooltip("Giá hiển thị dạng chuỗi, ví dụ: '$0.99' hoặc 'FREE'")]
    [SerializeField] private string priceLabel;

    public string Id          => id;
    public string DisplayName => displayName;
    public Sprite Icon        => icon;
    public string Description => description;
    public string PriceLabel  => priceLabel;
}
