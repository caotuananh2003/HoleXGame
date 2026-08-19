# Database Location & Viewing Guide

## 📍 Database Location trong Project

### Cấu trúc thư mục:

```
Assets/_Game/ScriptableObjects/
├── Db/                              ← Master Database
│   ├── PlayerProfile.asset          ← ScriptableObject trung tâm
│   └── Profile/
│       ├── AvatarDatabase.asset     ← Chứa danh sách AvatarDefinition
│       ├── FrameDatabase.asset      ← Chứa danh sách FrameDefinition
│       └── BadgeDatabase.asset      ← Chứa danh sách BadgeDefinition
│
└── Profile/                         ← Definition Assets
    ├── Avatar/
    │   ├── AvatarDefinition_0.asset
    │   ├── AvatarDefinition_1.asset
    │   └── ... (12 avatars)
    ├── Frame/
    │   ├── FrameDefinition_0.asset
    │   ├── FrameDefinition_1.asset
    │   └── ... (20 frames)
    └── Badge/
        ├── BadgeDefinition_0.asset
        ├── BadgeDefinition_1.asset
        ├── BadgeDefinition_2.asset
        └── BadgeDefinition_3.asset  ← (4 badges)
```

---

## 🗂️ 3 Loại Database

### 1. **PlayerProfile** (Master Database)
**Vị trí**: `Assets/_Game/ScriptableObjects/Db/PlayerProfile.asset`

**Nhiệm vụ**: Central hub — chứa references đến tất cả databases khác

**Cấu trúc**:
```
PlayerProfile (ScriptableObject)
├── HoleSkinDatabase    → HoleSkinDatabase asset
├── MapThemeDatabase    → MapThemeDatabase asset
├── ItemDefinitionList  → List<ItemDefinition>
├── AvatarDatabase      → AvatarDatabase asset  ← QUAN TRỌNG
├── FrameDatabase       → FrameDatabase asset   ← QUAN TRỌNG
└── BadgeDatabase       → BadgeDatabase asset   ← QUAN TRỌNG
```

---

### 2. **AvatarDatabase**
**Vị trí**: `Assets/_Game/ScriptableObjects/Db/Profile/AvatarDatabase.asset`

**Nhiệm vụ**: Chứa `List<AvatarDefinition>` — danh sách tất cả avatars trong game

**Cấu trúc internal**:
```csharp
public class AvatarDatabase : ScriptableObject
{
    [SerializeField] private List<AvatarDefinition> avatars;
    public IReadOnlyList<AvatarDefinition> Avatars => avatars;
}
```

**Mỗi AvatarDefinition** (trong folder `Profile/Avatar/`):
```
AvatarDefinition_0.asset:
- id: "0"
- avatar: Sprite (kéo từ Art resources)
- displayName: "Default Avatar"
- unlockedByDefault: true
```

---

### 3. **FrameDatabase**
**Vị trí**: `Assets/_Game/ScriptableObjects/Db/Profile/FrameDatabase.asset`

**Nhiệm vụ**: Chứa `List<FrameDefinition>` — danh sách tất cả frames

**Mỗi FrameDefinition** (trong folder `Profile/Frame/`):
```
FrameDefinition_0.asset:
- id: "frame_0"
- frame: Sprite (kéo từ Art resources)
- displayName: "Default Frame"
- unlockedByDefault: true
```

---

### 4. **BadgeDatabase**
**Vị trí**: `Assets/_Game/ScriptableObjects/Db/Profile/BadgeDatabase.asset`

**Nhiệm vụ**: Chứa `List<BadgeDefinition>` — danh sách tất cả badges

**Mỗi BadgeDefinition** (trong folder `Profile/Badge/`):
```
BadgeDefinition_0.asset:
- id: "badge_0"         ← CÓ THỂ ĐANG RỖNG (CẦN FIX)
- icon: Sprite
- displayName: "Champion"
- unlockedByDefault: true
```

---

## 👀 Cách xem Database trong Unity Editor

### Method 1: Project Window Search (Nhanh nhất)

#### Bước 1: Mở Project Window
```
Menu → Window → General → Project
hoặc
Ctrl + 5
```

#### Bước 2: Search trong Project
```
Trong Project window → search bar (top-right)
→ Gõ: "t:PlayerProfile"
→ Hoặc: "t:AvatarDatabase"
→ Hoặc: "t:BadgeDatabase"
```

#### Bước 3: Click vào asset
```
Click PlayerProfile.asset → Inspector hiện chi tiết
```

---

### Method 2: Browse Directly (Chi tiết hơn)

#### Xem PlayerProfile:
```
Project window → navigate đến:
Assets/_Game/ScriptableObjects/Db/
→ Double-click PlayerProfile.asset
```

**Inspector sẽ hiện**:
```
PlayerProfile (ScriptableObject)
┌─────────────────────────────────┐
│ Hole Skin Database              │ [HoleSkinDatabase] │
│ Map Theme Database              │ [MapThemeDatabase] │
│ Item Definition List            │                    │
│   [0] ItemDefinition_X          │                    │
│                                 │                    │
│ Avatar Database                 │ [AvatarDatabase]   │ ← Click để xem
│ Frame Database                  │ [FrameDatabase]    │ ← Click để xem
│ Badge Database                  │ [BadgeDatabase]    │ ← Click để xem
└─────────────────────────────────┘
```

#### Click vào AvatarDatabase field:
```
→ Unity sẽ select AvatarDatabase.asset
→ Inspector hiện:
```
```
AvatarDatabase (ScriptableObject)
┌─────────────────────────────────┐
│ Avatars (List)                  │
│   [0] AvatarDefinition_0        │ ← Click để xem chi tiết
│   [1] AvatarDefinition_1        │
│   [2] AvatarDefinition_2        │
│   ...                           │
│   [11] AvatarDefinition_11      │
└─────────────────────────────────┘
```

#### Click vào AvatarDefinition_0:
```
→ Inspector hiện chi tiết avatar:
```
```
AvatarDefinition_0 (ScriptableObject)
┌─────────────────────────────────┐
│ Id: "0"                         │
│ Avatar: [Sprite icon]           │ ← Kéo sprite vào đây
│ Display Name: "Default Avatar"  │
│ Unlocked By Default: ✓          │
└─────────────────────────────────┘
```

---

### Method 3: Inspector Debug Mode (Nâng cao)

#### Bước 1: Mở Inspector
```
Menu → Window → General → Inspector
hoặc
Ctrl + 4
```

#### Bước 2: Switch sang Debug Mode
```
Inspector window → top-right dropdown
→ Select "Debug"
```

#### Bước 3: Select database asset
```
Project → Select BadgeDatabase.asset
→ Inspector hiện TẤT CẢ internal fields (kể cả private)
```

---

## 🔍 Kiểm tra Badge Database cụ thể

### Bước 1: Mở PlayerProfile
```
Project → Assets/_Game/ScriptableObjects/Db/
→ Double-click PlayerProfile.asset
```

### Bước 2: Check BadgeDatabase field
```
Inspector → BadgeDatabase field:
- Nếu "None (BadgeDatabase)" → CHƯA ĐƯỢC ASSIGN
- Nếu có asset reference → ĐÃ OK
```

### Bước 3: Click vào BadgeDatabase
```
Click BadgeDatabase field circle
→ Unity select BadgeDatabase.asset
→ Inspector hiện:
```
```
BadgeDatabase (ScriptableObject)
┌─────────────────────────────────┐
│ Badges (List)                   │
│   [0] BadgeDefinition_0         │ ← Click để xem
│   [1] BadgeDefinition_1         │
│   [2] BadgeDefinition_2         │
│   [3] BadgeDefinition_3         │
└─────────────────────────────────┘
```

### Bước 4: Click BadgeDefinition_0
```
Inspector hiện:
```
```
BadgeDefinition_0 (ScriptableObject)
┌─────────────────────────────────┐
│ Id: "badge_0"                   │ ← KIỂM TRA CÓ GIÁ TRỊ KHÔNG
│ Icon: [Sprite]                  │ ← KIỂM TRA CÓ SPRITE KHÔNG
│ Display Name: "Champion"        │
│ Unlocked By Default: ✓          │
└─────────────────────────────────┘
```

---

## ⚠️ Vấn đề thường gặp

### Vấn đề 1: BadgeDatabase field = None
**Nguyên nhân**: PlayerProfile chưa được assign BadgeDatabase

**Fix**:
```
1. Project → Assets/_Game/ScriptableObjects/Db/Profile/
   → Tìm BadgeDatabase.asset
2. Select PlayerProfile.asset
3. Inspector → kéo BadgeDatabase.asset vào field "Badge Database"
4. Ctrl+S (Save)
```

---

### Vấn đề 2: BadgeDefinition có Id = rỗng
**Nguyên nhân**: Asset được tạo nhưng chưa điền Id

**Check**:
```
Project → Assets/_Game/ScriptableObjects/Profile/Badge/
→ Select BadgeDefinition_0.asset
→ Inspector → Id field:
   - Nếu rỗng → CẦN FIX
   - Nếu có giá trị → OK
```

**Fix**:
```
Select BadgeDefinition_0.asset
→ Inspector → Id field → Gõ: "badge_0"
→ Display Name → Gõ: "Champion"
→ Icon → Kéo sprite từ Art resources vào
→ Ctrl+S (Save)
```

Lặp lại cho các badges khác:
- BadgeDefinition_1: Id = "badge_1", Display Name = "Winner"
- BadgeDefinition_2: Id = "badge_2", Display Name = "Master"
- BadgeDefinition_3: Id = "badge_3", Display Name = "Legend"

---

### Vấn đề 3: BadgeDatabase.List = rỗng
**Nguyên nhân**: Database asset tồn tại nhưng không có items

**Check**:
```
Project → Select BadgeDatabase.asset
→ Inspector → Badges list:
   - Nếu List rỗng (không có element) → CẦN FIX
   - Nếu có [0], [1], [2], [3] → OK
```

**Fix**:
```
Select BadgeDatabase.asset
→ Inspector → Badges list
→ Click + để thêm element
→ Kéo BadgeDefinition_0.asset vào slot [0]
→ Kéo BadgeDefinition_1.asset vào slot [1]
→ Kéo BadgeDefinition_2.asset vào slot [2]
→ Kéo BadgeDefinition_3.asset vào slot [3]
→ Ctrl+S (Save)
```

---

### Vấn đề 4: Icon = None (Sprite missing)
**Nguyên nhân**: BadgeDefinition chưa có icon sprite

**Check**:
```
Select BadgeDefinition_0.asset
→ Inspector → Icon field:
   - Nếu "None (Sprite)" → CẦN FIX
   - Nếu có sprite → OK
```

**Fix**:
```
1. Project → tìm sprite badge (thường trong Assets/_Game/Art/UI/Badges/)
2. Select BadgeDefinition_0.asset
3. Kéo sprite vào Icon field
4. Ctrl+S (Save)
```

---

## 🛠️ Tạo mới Badge Database (nếu chưa có)

### Bước 1: Tạo BadgeDatabase asset
```
Project → Right-click → Create → Database → Badge Database
→ Đặt tên: BadgeDatabase
→ Save vào: Assets/_Game/ScriptableObjects/Db/Profile/
```

### Bước 2: Tạo BadgeDefinition assets
```
Project → Right-click → Create → Definition → Badge Definition
→ Đặt tên: BadgeDefinition_0
→ Save vào: Assets/_Game/ScriptableObjects/Profile/Badge/
```

Lặp lại cho các badges khác.

### Bước 3: Populate BadgeDatabase
```
Select BadgeDatabase.asset
→ Inspector → Badges list
→ Click + để thêm 4 elements
→ Kéo BadgeDefinition_0 vào [0]
→ Kéo BadgeDefinition_1 vào [1]
→ Kéo BadgeDefinition_2 vào [2]
→ Kéo BadgeDefinition_3 vào [3]
```

### Bước 4: Assign vào PlayerProfile
```
Select PlayerProfile.asset
→ Inspector → Badge Database field
→ Kéo BadgeDatabase.asset vào
→ Ctrl+S (Save)
```

---

## 📊 Summary Checklist

### PlayerProfile Setup
- [ ] PlayerProfile.asset tồn tại tại `Db/PlayerProfile.asset`
- [ ] AvatarDatabase field assigned
- [ ] FrameDatabase field assigned
- [ ] BadgeDatabase field assigned ← **KIỂM TRA ĐIỀU NÀY**

### AvatarDatabase Setup
- [ ] AvatarDatabase.asset tồn tại tại `Db/Profile/AvatarDatabase.asset`
- [ ] Avatars list có 12 items
- [ ] Mỗi item có Id, Avatar sprite, Display Name

### FrameDatabase Setup
- [ ] FrameDatabase.asset tồn tại tại `Db/Profile/FrameDatabase.asset`
- [ ] Frames list có 20 items
- [ ] Mỗi item có Id, Frame sprite, Display Name

### BadgeDatabase Setup
- [ ] BadgeDatabase.asset tồn tại tại `Db/Profile/BadgeDatabase.asset`
- [ ] Badges list có 4 items ← **KIỂM TRA ĐIỀU NÀY**
- [ ] Mỗi item có:
  - [ ] Id (không rỗng) ← **KIỂM TRA ĐIỀU NÀY**
  - [ ] Icon sprite ← **KIỂM TRA ĐIỀU NÀY**
  - [ ] Display Name
  - [ ] Unlocked By Default = true

---

## 🎯 Next Steps

Sau khi verify databases:

1. **Chạy Diagnostics Tool**:
   ```
   Unity Editor → Tools → Profile → Diagnose Badge System
   ```

2. **Test trong Play Mode**:
   - MainMenuScene → Profile Button → Edit Button
   - Click Badge Tab
   - Verify badges hiển thị đúng

3. **Check Console Logs**:
   - Nếu có warning về null reference → fix theo guide
   - Nếu không có warning → database setup OK

---

## Version History
- v1.0: Database location & viewing guide
