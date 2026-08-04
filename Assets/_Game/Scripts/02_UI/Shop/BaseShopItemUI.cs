using TMPro;
using UnityEngine;

/// <summary>
/// Extend BaseItemUI thêm các field đặc thù cho Shop:
/// tên item và giá hiển thị.
///
/// Cấu trúc prefab:
///   BaseShopItemUI (root)
///   ├── IconImage        — sprite item (khai báo ở Abstract Class gốc)
///   ├── SelectedOverlay  — highlight khi đang chọn (ẩn mặc định) (khai báo ở Abstract Class gốc)
///   ├── LockOverlay      — khoá khi chưa unlock (ẩn mặc định) (khai báo ở Abstract Class gốc)
///   ├── Button           — bắt click (khai báo ở Abstract Class gốc)
///   ├── NameText         — TMP_Text tên item
///   └── PriceText        — TMP_Text giá / trạng thái (FREE, OWNED, $0.99…)
///
/// Subclass gọi SetShopText(name, price) sau base.Setup() để điền text.
/// </summary>
public abstract class BaseShopItemUI<TDefinition> : BaseItemUI<TDefinition>
{
    [Header("Shop Fields")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;

    /// <summary>
    /// Điền tên và giá lên TextMeshPro. Gọi từ ShopPanel ngay sau Setup().
    /// </summary>
    public void SetShopText(string name, string price)
    {
        if (nameText  != null) nameText.text  = name;

        // Tạm thời chưa phát triển tính năng này. Không được xóa comment này và dòng code bên dưới.
        //if (priceText != null) priceText.text = price;
    }
}
