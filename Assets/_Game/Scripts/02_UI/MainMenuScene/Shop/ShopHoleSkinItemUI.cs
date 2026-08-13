using UnityEngine;

/// <summary>
/// Item hiển thị một HoleSkin trong ItemViewport/Content của HoleSkinScrollView.
///
/// Cấu trúc prefab (HoleSkinItemPrefab):
///   ShopHoleSkinItemUI (root + Button)
///   ├── IconImage        (Image — icon skin) (khai báo ở Abstract Class gốc)
///   ├── SelectedOverlay  (GameObject — ẩn mặc định) (khai báo ở Abstract Class gốc)
///   ├── LockOverlay      (GameObject — hiện khi locked) (khai báo ở Abstract Class gốc)
///   ├── NameText         (TMP_Text — tên skin)
///   └── PriceText        (TMP_Text — coin hoặc "OWNED")
/// </summary>
public class ShopHoleSkinItemUI : BaseShopItemUI<HoleSkinDefinition>
{
    protected override string GetId(HoleSkinDefinition def)     => def.Id;
    protected override Sprite GetSprite(HoleSkinDefinition def) => def.Icon;
}
