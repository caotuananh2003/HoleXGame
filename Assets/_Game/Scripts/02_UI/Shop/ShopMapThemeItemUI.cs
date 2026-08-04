using UnityEngine;

/// <summary>
/// Item hiển thị một MapTheme trong ItemViewport/Content của MapThemeScrollView.
///
/// Cấu trúc prefab (MapThemeItemPrefab):
///   ShopMapThemeItemUI (root + Button)
///   ├── IconImage        (Image — EnableIcon làm icon chính) (khai báo ở Abstract Class gốc)
///   ├── SelectedOverlay  (GameObject — ẩn mặc định) (khai báo ở Abstract Class gốc)
///   ├── LockOverlay      (GameObject — hiện khi locked) (khai báo ở Abstract Class gốc)
///   ├── NameText         (TMP_Text — tên theme)
///   └── PriceText        (TMP_Text — coin hoặc "OWNED")
/// </summary>
public class ShopMapThemeItemUI : BaseShopItemUI<MapThemeDefinition>
{
    protected override string GetId(MapThemeDefinition def)     => def.Id;
    protected override Sprite GetSprite(MapThemeDefinition def) => def.EnableIcon;
}
