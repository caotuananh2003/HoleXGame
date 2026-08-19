# Badge System Fix Guide

## Vấn đề
Badge selection trong EditProfilePopup không hoạt động giống Avatar và Frame.

## Nguyên nhân có thể

### 1. **ProfilePreview.badgeImage không được assign trong Unity Inspector**
   - Vị trí: `EditProfilePopup` prefab → child object `ProfilePreview`
   - Triệu chứng: Badge preview không cập nhật khi select

### 2. **BadgeDatabase không được assign vào PlayerProfile**
   - Vị trí: `PlayerProfile` ScriptableObject asset
   - Triệu chứng: Badge không hiển thị, console warning về database null

### 3. **BadgeItemUI prefab thiếu references**
   - Vị trí: BadgeItemUI prefab
   - Triệu chứng: Badge item không click được hoặc không hiển thị

### 4. **BadgeDefinition assets thiếu Icon**
   - Vị trí: Các BadgeDefinition ScriptableObject assets
   - Triệu chứng: Badge preview hiển thị trống

---

## Bước 1: Chạy Diagnostics Tool

### Trong Unity Editor:
1. Menu: **Tools → Profile → Diagnose Badge System**
2. Xem Console log để xác định vấn đề cụ thể
3. Tool sẽ kiểm tra:
   - ✓ PlayerProfile có tồn tại không
   - ✓ BadgeDatabase được assign không
   - ✓ BadgeDefinition assets có Icon không
   - ✓ BadgeItemUI prefab structure
   - ✓ ProfilePreview references trong EditProfilePopup

---

## Bước 2: Fix PlayerProfile & BadgeDatabase

### A. Kiểm tra PlayerProfile ScriptableObject

**Vị trí:** `Assets/_Game/Data/` hoặc search "PlayerProfile" trong Project

1. Chọn PlayerProfile asset
2. Trong Inspector, tìm field **"Badge Database"**
3. Nếu rỗng → kéo BadgeDatabase asset vào

### B. Tạo BadgeDatabase (nếu chưa có)

1. Right-click trong Project → **Create → Database → Badge Database**
2. Đặt tên: `BadgeDatabase`
3. Kéo asset vào PlayerProfile.BadgeDatabase

### C. Tạo BadgeDefinition assets

1. Right-click → **Create → Definition → Badge Definition**
2. Đặt tên: `Badge_Champion`, `Badge_Winner`, etc.
3. Trong Inspector:
   - **Id**: `badge_champion` (phải unique, lowercase, snake_case)
   - **Icon**: Kéo sprite badge vào
   - **Display Name**: "Champion"
   - **Unlocked By Default**: ✓ (check để unlock mặc định)

4. Thêm BadgeDefinition vào BadgeDatabase:
   - Chọn BadgeDatabase asset
   - **Badges** list → thêm element → kéo BadgeDefinition asset vào

---

## Bước 3: Fix BadgeItemUI Prefab

### Vị trí prefab:
`Assets/_Game/Prefabs/UI/Profile/BadgeItemUI.prefab` (hoặc tương tự)

### Hierarchy mong muốn:
```
BadgeItemUI (BadgeItemUI component)
├── IconImage (Image component)
├── Description
│   ├── UnLocked (GameObject)
│   └── Locked (GameObject)
├── SelectedOverlay (GameObject - ẩn mặc định)
└── Button (Button component)
```

### Inspector Setup:

1. Chọn BadgeItemUI prefab root
2. Component **BadgeItemUI** (inherit từ BaseItemUI):
   - **Icon Image**: kéo child `IconImage` vào
   - **Selected Overlay**: kéo child `SelectedOverlay` vào
   - **Unlocked Object**: kéo `Description/UnLocked` vào
   - **Locked Object**: kéo `Description/Locked` vào
   - **Button**: kéo child `Button` vào

### Note:
- Nếu prefab chưa có → duplicate `AvatarItemUI.prefab` → rename → sửa component thành `BadgeItemUI`
- IconImage phải có **Preserve Aspect** = ✓ để badge không bị méo

---

## Bước 4: Fix ProfilePreview trong EditProfilePopup

### Vị trí:
`EditProfilePopup` prefab → tìm child object `ProfilePreview`

### Hierarchy mong muốn:
```
ProfilePreview (ProfilePreview component)
├── AvatarImage (Image)
├── FrameImage (Image)
└── BadgeImage (Image)
```

### Inspector Setup:

1. Chọn ProfilePreview GameObject
2. Component **ProfilePreview**:
   - **Avatar Image**: kéo child `AvatarImage` vào
   - **Frame Image**: kéo child `FrameImage` vào
   - **Badge Image**: kéo child `BadgeImage` vào ← **QUAN TRỌNG**

### Nếu BadgeImage chưa tồn tại:

1. Right-click `ProfilePreview` → Create Empty → rename `BadgeImage`
2. Add Component → **UI → Image**
3. Setup Image component:
   - **Source Image**: (để trống, sẽ được set runtime)
   - **Image Type**: Simple
   - **Preserve Aspect**: ✓
   - **Raycast Target**: ✗ (không cần)
4. RectTransform:
   - **Position**: đặt vị trí hiển thị badge (thường góc dưới phải của avatar)
   - **Width/Height**: 64x64 (hoặc tùy design)
5. Kéo vào ProfilePreview component

---

## Bước 5: Verify EditProfilePopup Setup

### Chọn EditProfilePopup prefab root, kiểm tra Inspector:

**Component EditProfilePopup:**
- **Profile Preview**: phải có reference đến child ProfilePreview
- **Badge Tab Button**: kéo nút tab "Badge" vào
- **Badge Content**: kéo Transform chứa badge items vào (thường `ScrollView/Viewport/BadgeContent`)
- **Badge Item Prefab**: kéo BadgeItemUI prefab vào
- **Player Profile**: kéo PlayerProfile ScriptableObject vào

### Nếu BadgeContent chưa có:

Duplicate `AvatarContent` → rename `BadgeContent`:
```
ScrollRect
└── Viewport
    └── Content (scroll container)
        ├── AvatarContent (Transform - active theo tab)
        ├── FrameContent (Transform - active theo tab)
        └── BadgeContent (Transform - active theo tab) ← TẠO MỚI
```

---

## Bước 6: Test Flow

### Test trong Unity Play Mode:

1. **Mở MainMenuScene**
2. Click **Profile Button** → ProfilePopup hiển thị
3. Click **Edit** → EditProfilePopup hiển thị
4. Click **Badge Tab** → badge list hiển thị

### Kiểm tra từng bước:

#### A. Badge Item Display
- ✓ Badge items hiển thị đúng icon
- ✓ Selected overlay hiển thị trên badge đang chọn
- ✓ Locked/Unlocked state hiển thị đúng

#### B. Badge Selection
- Click một badge → log console: `[EditProfilePopup] OnBadgeItemClicked: badge_champion`
- ✓ Selected overlay chuyển sang badge mới click
- ✓ Badge preview (ProfilePreview.badgeImage) cập nhật ngay

#### C. Badge Save
- Select badge → click **Save** button
- ✓ Log: `[EditProfilePopup] Profile đã được lưu.`
- Đóng popup → mở lại → badge đã chọn vẫn được highlight

#### D. Badge Sync
- Save badge trong EditProfilePopup
- Đóng về ProfilePopup
- ✓ ProfilePopup preview hiển thị badge vừa chọn
- Đóng về MainmenuPanel
- ✓ ProfileButton preview hiển thị badge vừa chọn

---

## Bước 7: Debug Console Warnings

### Nếu thấy warning này:

#### `[ProfilePreview] Refresh() nhận ProfileData null`
→ SaveManager chưa initialize hoặc PlayerData null
→ Check BootstrapLoader → MainMenuScene flow

#### `[EditProfilePopup] SaveManager.Data is null. Cannot save.`
→ SaveManager không được inject đúng qua VContainer
→ Check MainMenuLifetimeScope có register EditProfilePopup không

#### `[ProfilePreview] audioManager is null. Cannot sync toggles.`
→ Sai warning (copy-paste từ SettingPopup) — ignore hoặc sửa message

#### `BadgeDefinition.GetById() returned null for id 'badge_xxx'`
→ Badge ID không tồn tại trong BadgeDatabase
→ Check BadgeDatabase.Badges list có item với ID đó không

---

## Code Changes (nếu cần)

### Không cần sửa code nếu:
- BadgeItemUI đã inherit BaseItemUI
- EditProfilePopup có đầy đủ 3 handlers: OnAvatarItemClicked, OnFrameItemClicked, OnBadgeItemClicked
- ProfilePreview có SetAvatar, SetFrame, SetBadge

### Code đã đúng:
```csharp
// EditProfilePopup.cs
private void OnBadgeItemClicked(string badgeId)
{
    editingData.selectedBadgeId = badgeId;
    profilePreview.SetBadge(badgeId);
    RefreshSelectionHighlights();
}

// ProfilePreview.cs
public void SetBadge(string badgeId)
{
    if (badgeImage == null) return;

    BadgeDefinition def = badgeDatabase != null ? badgeDatabase.GetById(badgeId) : null;
    badgeImage.sprite = def != null ? def.Icon : null;
}
```

---

## Common Issues & Solutions

### Issue 1: Badge không click được
**Nguyên nhân**: Button component không được assign hoặc không interactable  
**Fix**: Kiểm tra BadgeItemUI prefab → Button component → Interactable = ✓

### Issue 2: Badge click nhưng preview không đổi
**Nguyên nhân**: ProfilePreview.badgeImage = null  
**Fix**: Assign badgeImage trong EditProfilePopup prefab

### Issue 3: Badge preview hiển thị trắng
**Nguyên nhân**: BadgeDefinition.Icon = null  
**Fix**: Assign sprite Icon cho từng BadgeDefinition asset

### Issue 4: Badge save nhưng không sync sang ProfilePopup
**Nguyên nhân**: EditProfilePopup.OnSaveClicked() không gọi RefreshPreview()  
**Fix**: Code đã đúng — check SaveManager có lưu đúng không (xem PlayerPrefs)

### Issue 5: Badge tab không hiển thị items
**Nguyên nhân**: badgeContent không được assign hoặc badgeItemPrefab null  
**Fix**: Assign đầy đủ trong EditProfilePopup Inspector

---

## Checklist tổng hợp

### ScriptableObject Setup
- [ ] PlayerProfile asset có BadgeDatabase assigned
- [ ] BadgeDatabase có ít nhất 1 BadgeDefinition
- [ ] Tất cả BadgeDefinition có Id và Icon

### Prefab Setup
- [ ] BadgeItemUI prefab có đầy đủ hierarchy
- [ ] BadgeItemUI component có đầy đủ references (IconImage, SelectedOverlay, Button)
- [ ] EditProfilePopup có ProfilePreview assigned
- [ ] ProfilePreview có badgeImage assigned
- [ ] EditProfilePopup có badgeContent assigned
- [ ] EditProfilePopup có badgeItemPrefab assigned

### Scene Setup
- [ ] MainMenuScene có EditProfilePopup prefab instance
- [ ] UISceneRoot hierarchy đúng
- [ ] MainMenuLifetimeScope register EditProfilePopup

### Runtime Test
- [ ] Badge items spawn đúng
- [ ] Badge click → preview cập nhật
- [ ] Badge save → sync sang ProfilePopup và MainmenuPanel
- [ ] Badge state persist sau khi restart game

---

## Contact / Support

Nếu sau khi làm theo guide vẫn gặp vấn đề:

1. Chạy **Diagnostics Tool** (Tools → Profile → Diagnose Badge System)
2. Copy toàn bộ Console log
3. Screenshot Inspector của:
   - PlayerProfile asset
   - BadgeDatabase asset
   - BadgeItemUI prefab
   - EditProfilePopup → ProfilePreview
4. Mô tả chi tiết hiện tượng lỗi (badge không đổi màu? không save? không sync?)

---

## Version History
- v1.0: Initial guide - Badge system parity with Avatar/Frame
