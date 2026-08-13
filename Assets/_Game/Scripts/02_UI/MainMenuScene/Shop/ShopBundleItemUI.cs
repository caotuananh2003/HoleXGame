using UnityEngine;

/// <summary>
/// Item hiển thị một Bundle trong ScrollView Tab Main của ShopPanel.
///
/// Cấu trúc prefab (BundleItemPrefab):
///   ShopBundleItemUI (root + Button)
///   ├── IconImage (khai báo ở Abstract Class gốc)
///   ├── SelectedOverlay  (ẩn mặc định) (khai báo ở Abstract Class gốc)
///   ├── LockOverlay      (ẩn mặc định — bundle luôn available) (khai báo ở Abstract Class gốc)
///   ├── NameText         (TMP_Text — tên bundle)
///   └── PriceText        (TMP_Text — "$0.99" hoặc "FREE")
/// </summary>
public class ShopBundleItemUI : BaseShopItemUI<BundleDefinition>
{
    protected override string GetId(BundleDefinition def)     => def.Id;
    protected override Sprite GetSprite(BundleDefinition def) => def.Icon;
}
