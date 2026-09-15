# Hướng dẫn Setup Item System trong Unity Editor

Hệ thống Item/Power-up đã được implement đầy đủ. Tài liệu này hướng dẫn setup trong Unity Editor để sử dụng thực tế.

---

## Tổng quan kiến trúc

**Luồng hoạt động:**
1. **ItemDefinition (SO)** — chứa thông tin item (id, name, icon, isLocked, defaultAmount) + array of **ItemEffectDefinition**
2. **ItemEffectDefinition (SO)** — Strategy Pattern, mỗi effect là một ScriptableObject riêng
3. **ItemManager (MonoBehaviour)** — validate/apply/consume/save, inject từ VContainer
4. **ItemSlotUI (Component)** — UI component cho mỗi slot, hiển thị icon/quantity/locked state
5. **GameplayPanel** — quản lý array of ItemSlotUI, lắng nghe click event, gọi ItemManager.UseItem()

**Dependencies:**
- `HoleController` — để tăng size hole (IncreaseSizeEffect)
- `GameTimer` — để tăng thời gian (TimeExtensionEffect)
- `SaveManager` — để lưu quantity runtime (inject từ VContainer)
- `PlayerData.itemQuantities` — Dictionary<string, int> lưu số lượng item

---

## A. Tạo Effect ScriptableObjects

Mỗi effect type cần tạo một asset riêng.

### 1. IncreaseSizeEffectDefinition

**Đường dẫn:** `Assets/_Game/Data/Items/Effects/IncreaseSizeEffect.asset`

**Cách tạo:**
1. Right-click trong Project → **Create > Gameplay > Items > Effects > Increase Size Effect**
2. Rename thành `IncreaseSizeEffect`
3. Inspector:
   - **Effect Name:** "Increase Size"
   - **Effect Description:** "Instantly grow your hole larger"

**Logic:** Gọi `HoleController.GrowHole()` để tăng kích thước hole ngay lập tức.

---

### 2. TimeExtensionEffectDefinition

**Đường dẫn:** `Assets/_Game/Data/Items/Effects/TimeExtensionEffect.asset`

**Cách tạo:**
1. Right-click → **Create > Gameplay > Items > Effects > Time Extension Effect**
2. Rename thành `TimeExtensionEffect`
3. Inspector:
   - **Effect Name:** "Time Extension"
   - **Effect Description:** "Add extra time to the game"
   - **Additional Seconds:** `30` (thêm 30 giây)

**Logic:** Gọi `GameTimer.AddTime(additionalSeconds)`.

---

### 3. MagnetEffectDefinition

**Đường dẫn:** `Assets/_Game/Data/Items/Effects/MagnetEffect.asset`

**Cách tạo:**
1. Right-click → **Create > Gameplay > Items > Effects > Magnet Effect**
2. Rename thành `MagnetEffect`
3. Inspector:
   - **Effect Name:** "Magnet"
   - **Effect Description:** "Attract nearby objects to your hole"
   - **Duration:** `10` (seconds)
   - **Pull Radius:** `15` (units)
   - **Pull Force:** `500` (force magnitude)

**Logic:** Spawn `MagnetEffect` MonoBehaviour runtime, FixedUpdate loop áp lực hút vào center hole, tự destroy sau duration.

**Note:** Magnet chỉ ảnh hưởng objects có Rigidbody và nằm trên **Layer 9** (Swallowable).

---

### 4. BombShieldEffectDefinition

**Đường dẫn:** `Assets/_Game/Data/Items/Effects/BombShieldEffect.asset`

**Cách tạo:**
1. Right-click → **Create > Gameplay > Items > Effects > Bomb Shield Effect**
2. Rename thành `BombShieldEffect`
3. Inspector:
   - **Effect Name:** "Bomb Shield"
   - **Effect Description:** "Protect your hole from bombs"
   - **Duration:** `15` (seconds)

**Logic:** Spawn `BombShieldEffect` MonoBehaviour runtime, set static property `BombShieldEffect.IsActive = true`. SwallowHandler check property này trước khi trigger game over. Tự destroy sau duration.

**Note:** Nếu dùng shield nhiều lần trong cùng một lúc, duration sẽ **extend** chứ không tạo instance mới (avoid duplicate).

---

## B. Tạo ItemDefinition Assets

ItemDefinition là SO chính chứa thông tin item + array of effects.

### Ví dụ 1: Mega Hole Item (2 effects)

**Đường dẫn:** `Assets/_Game/Data/Items/MegaHoleItem.asset`

**Cách tạo:**
1. Right-click → **Create > Gameplay > Items > Item Definition**
2. Rename thành `MegaHoleItem`
3. Inspector:
   - **Item Id:** `"mega_hole"` (unique string, dùng để lưu/load)
   - **Item Name:** "Mega Hole"
   - **Icon:** Kéo sprite icon vào đây
   - **Description:** "Instantly grow your hole and add 30 seconds"
   - **Is Locked:** `false` (nếu true thì không thể use)
   - **Default Amount:** `5` (số lượng khởi tạo khi chưa có save data)
   - **Effects (Array):**
     - Size: `2`
     - Element 0: `IncreaseSizeEffect` (kéo asset từ bước A.1)
     - Element 1: `TimeExtensionEffect` (kéo asset từ bước A.2)

**Kết quả:** Khi dùng item này, sẽ apply **cả 2 effects**: tăng size + thêm thời gian.

---

### Ví dụ 2: Magnet Item (1 effect)

**Đường dẫn:** `Assets/_Game/Data/Items/MagnetItem.asset`

**Cách tạo:**
1. Right-click → **Create > Gameplay > Items > Item Definition**
2. Rename thành `MagnetItem`
3. Inspector:
   - **Item Id:** `"magnet"`
   - **Item Name:** "Magnet"
   - **Icon:** Kéo sprite icon
   - **Description:** "Attract nearby objects for 10 seconds"
   - **Is Locked:** `false`
   - **Default Amount:** `3`
   - **Effects (Array):**
     - Size: `1`
     - Element 0: `MagnetEffect` (kéo asset từ bước A.3)

---

### Ví dụ 3: Bomb Shield Item (1 effect)

**Đường dẫn:** `Assets/_Game/Data/Items/BombShieldItem.asset`

**Cách tạo:**
1. Right-click → **Create > Gameplay > Items > Item Definition**
2. Rename thành `BombShieldItem`
3. Inspector:
   - **Item Id:** `"bomb_shield"`
   - **Item Name:** "Bomb Shield"
   - **Icon:** Kéo sprite icon
   - **Description:** "Protect from bombs for 15 seconds"
   - **Is Locked:** `false`
   - **Default Amount:** `2`
   - **Effects (Array):**
     - Size: `1`
     - Element 0: `BombShieldEffect` (kéo asset từ bước A.4)

---

### Ví dụ 4: Time Booster Item (1 effect)

**Đường dẫn:** `Assets/_Game/Data/Items/TimeBoosterItem.asset`

**Cách tạo:**
1. Right-click → **Create > Gameplay > Items > Item Definition**
2. Rename thành `TimeBoosterItem`
3. Inspector:
   - **Item Id:** `"time_booster"`
   - **Item Name:** "Time Booster"
   - **Icon:** Kéo sprite icon
   - **Description:** "Add 30 seconds to the timer"
   - **Is Locked:** `false`
   - **Default Amount:** `3`
   - **Effects (Array):**
     - Size: `1`
     - Element 0: `TimeExtensionEffect` (kéo asset từ bước A.2)

**Note:** Bạn có thể tạo bao nhiêu ItemDefinition tùy ý, mỗi item có thể chứa **nhiều effects khác nhau**.

---

## C. Setup ItemManager trong GameplayScene

### 1. Tạo GameObject ItemManager

**Hierarchy:**
```
GameplayScene
├─ GameManagers (existing)
│  ├─ GameplayController
│  ├─ LevelManager
│  └─ ItemManager (NEW)
```

**Cách tạo:**
1. Trong Hierarchy, tìm GameObject `GameManagers` (hoặc tương đương)
2. Right-click → **Create Empty**
3. Rename thành `ItemManager`
4. Add Component → **Item Manager** (script)

**Inspector:**
- Không cần kéo field nào cả, ItemManager sẽ tự find `HoleController` và `GameTimer` khi cần.
- `SaveManager` được inject tự động từ VContainer.

---

### 2. Đăng ký ItemManager vào VContainer

**File:** `GameplayLifetimeScope.cs`

**Đã được implement:** ItemManager đã được đăng ký trong `GameplayLifetimeScope.Configure()`:

```csharp
builder.RegisterComponentInHierarchy<ItemManager>();
```

**Verify:**
1. Mở scene **GameplayScene**
2. Tìm GameObject `GameplayLifetimeScope` (thường nằm ở root)
3. Inspector → **Gameplay Lifetime Scope** component
4. Verify rằng script có dòng `RegisterComponentInHierarchy<ItemManager>()`

**Lưu ý:** Nếu project dùng nhiều scene, đảm bảo ItemManager chỉ được đăng ký trong **GameplayLifetimeScope** (child scope), không đăng ký trong **GameLifetimeScope** (root scope), vì ItemManager phụ thuộc vào `HoleController` và `GameTimer` chỉ có trong gameplay.

---

## D. Setup GameplayPanel UI

### 1. Chuẩn bị ItemSlotUI trong Hierarchy

**Hierarchy (ví dụ):**
```
UICanvas
└─ GameplayPanel
   ├─ TopBar (existing)
   ├─ ScoreText (existing)
   └─ ItemSlotsContainer (NEW)
      ├─ ItemSlot1
      ├─ ItemSlot2
      ├─ ItemSlot3
      └─ ItemSlot4
```

**Cách tạo mỗi ItemSlot:**
1. Right-click `ItemSlotsContainer` → **UI > Button**
2. Rename thành `ItemSlot1`
3. Add Component → **Item Slot UI** (script)
4. Inspector của ItemSlot1:
   - **Icon (Image):** Kéo child Image component vào đây (hiển thị icon item)
   - **Quantity Text (TMP_Text):** Kéo child TextMeshPro vào đây (hiển thị số lượng)
   - **Locked Overlay (GameObject):** Tạo child Image màu đen alpha 0.7, kéo vào đây (hiển thị khi locked)
   - **Button (Button):** Kéo chính ItemSlot1's Button component vào đây

**Cấu trúc UI gợi ý cho mỗi slot:**
```
ItemSlot1 (Button)
├─ Icon (Image) — sprite icon item
├─ QuantityText (TextMeshPro) — "x5"
└─ LockedOverlay (Image) — black 70% alpha, anchor fill
```

**Repeat** cho ItemSlot2, ItemSlot3, ItemSlot4.

---

### 2. Setup GameplayPanel Inspector

**File:** `GameplayPanel.cs` (đã được update)

**Hierarchy:** Tìm GameObject `GameplayPanel` (hoặc tương đương)

**Inspector của GameplayPanel:**
1. **Item Slots (Array):**
   - Size: `4`
   - Element 0: Kéo `ItemSlot1` vào
   - Element 1: Kéo `ItemSlot2` vào
   - Element 2: Kéo `ItemSlot3` vào
   - Element 3: Kéo `ItemSlot4` vào

2. **Item Definitions (Array):**
   - Size: `4`
   - Element 0: Kéo `MegaHoleItem` asset vào (hoặc item khác)
   - Element 1: Kéo `MagnetItem` asset vào
   - Element 2: Kéo `BombShieldItem` asset vào
   - Element 3: Kéo `TimeBoosterItem` asset vào

**Lưu ý:** Thứ tự trong `itemDefinitions` array phải khớp với thứ tự trong `itemSlots` array. Slot 0 hiển thị item 0, slot 1 hiển thị item 1, ...

---

### 3. Verify Injection

**GameplayPanel** inject `ItemManager` từ VContainer:

```csharp
[Inject]
private void Construct(ItemManager itemManager)
{
    this.itemManager = itemManager;
}
```

**Verify:**
1. Play scene
2. Check Console, không có lỗi "ItemManager not found" hoặc NullReferenceException
3. Nếu có lỗi, verify bước C.2 đã đăng ký ItemManager vào VContainer đúng chưa

---

## E. Setup EditorItemCheatController (Optional Test)

**Mục đích:** Test nhanh item system trong Unity Editor bằng phím tắt.

### 1. Tạo GameObject

**Hierarchy:**
```
GameplayScene
└─ DebugTools (NEW)
   └─ EditorItemCheatController
```

**Cách tạo:**
1. Right-click Hierarchy → **Create Empty**
2. Rename thành `DebugTools`
3. Right-click `DebugTools` → **Create Empty**
4. Rename thành `EditorItemCheatController`
5. Add Component → **Editor Item Cheat Controller** (script)

---

### 2. Setup Inspector

**Inspector của EditorItemCheatController:**
- **Test Items (Array):**
  - Size: `4`
  - Element 0: Kéo `MegaHoleItem` asset vào
  - Element 1: Kéo `MagnetItem` asset vào
  - Element 2: Kéo `BombShieldItem` asset vào
  - Element 3: Kéo `TimeBoosterItem` asset vào

**Lưu ý:** Thứ tự phải khớp với phím tắt (key 1 = slot 0, key 2 = slot 1, ...).

---

### 3. Phím tắt

| Phím | Chức năng |
|------|-----------|
| **1** | Use item slot 0 (MegaHole) |
| **2** | Use item slot 1 (Magnet) |
| **3** | Use item slot 2 (BombShield) |
| **4** | Use item slot 3 (TimeBooster) |
| **Q** | Add 1000 currency |
| **E** | Reset all item quantities về defaultAmount |

**Chỉ hoạt động:** Trong Unity Editor (không build vào game).

---

## F. Test Flow

### 1. Khởi tạo dữ liệu

**Lần chạy đầu tiên (chưa có save file):**
- PlayerData.itemQuantities sẽ được khởi tạo với `defaultAmount` từ ItemDefinition
- Ví dụ: `MegaHoleItem.defaultAmount = 5` → quantity ban đầu là 5

**Lần chạy thứ 2 trở đi:**
- Load từ save file, quantity là giá trị đã lưu trước đó

---

### 2. Test Use Item

**Cách test:**
1. Play GameplayScene
2. Click vào một ItemSlotUI (hoặc nhấn phím 1234 nếu đã setup EditorCheatController)
3. **Expected behavior:**
   - Nếu item unlocked + quantity > 0:
     - Effect được apply (hole tăng size / thêm thời gian / magnet hoạt động / bomb shield active)
     - Quantity giảm 1
     - UI cập nhật số lượng mới
     - SaveManager.Save() được gọi
   - Nếu item locked hoặc quantity = 0:
     - Log warning "Cannot use item"
     - Không thay đổi gì

---

### 3. Test Save/Load

**Cách test:**
1. Play scene, use item vài lần (ví dụ quantity giảm từ 5 → 3)
2. Stop scene
3. Play scene lại
4. **Expected:** Quantity hiển thị là 3 (đã lưu và load đúng)

---

### 4. Test Bomb Shield

**Setup:**
1. Tạo ObstacleDefinition asset với `Obstacle Type = Bomb`
2. Spawn obstacle đó vào level
3. Play scene

**Test:**
- **Không có shield:** Swallow bomb → GameController trigger game over (cần subscribe `SwallowHandler.OnBombSwallowedWithoutShield`)
- **Có shield:** Use BombShieldItem → swallow bomb → hole vẫn ổn, không game over

**Lưu ý:** Hiện tại GameplayController **chưa subscribe** event `OnBombSwallowedWithoutShield`. Cần thêm logic:

```csharp
// Trong GameplayController.cs
private void Start()
{
    swallowHandler.OnBombSwallowedWithoutShield += HandleBombSwallowed;
}

private void OnDestroy()
{
    swallowHandler.OnBombSwallowedWithoutShield -= HandleBombSwallowed;
}

private void HandleBombSwallowed()
{
    // Trigger game over
    Debug.LogError("Game Over: Bomb swallowed without shield!");
}
```

---

### 5. Test Magnet Effect

**Setup:**
1. Play scene với nhiều obstacles có Rigidbody
2. Use MagnetItem

**Expected:**
- Các object trong bán kính 15 units (configurable) sẽ bị hút về hole
- Effect kéo dài 10 seconds (configurable)
- Chỉ ảnh hưởng objects trên **Layer 9** (Swallowable)

**Debug:**
- Magnet effect có Gizmo hiển thị bán kính pull trong Scene view (màu cyan)

---

## G. Mở rộng sau này

### 1. Thêm effect mới

**Ví dụ:** Effect "Freeze Time" (dừng timer trong 5 giây)

**Steps:**
1. Tạo class `FreezeTimeEffectDefinition.cs` kế thừa `ItemEffectDefinition`
2. Override `Apply()` method:
   ```csharp
   protected override void Apply(ItemEffectContext context)
   {
       if (context.GameTimer != null)
       {
           context.GameTimer.PauseTimer(duration);
       }
   }
   ```
3. Tạo SO asset `FreezeTimeEffect.asset`
4. Thêm effect này vào array của ItemDefinition mong muốn

**Không cần sửa ItemManager!** (Open/Closed Principle)

---

### 2. Thêm item mới

**Steps:**
1. Tạo ItemDefinition asset mới (bước B)
2. Kéo vào `GameplayPanel.itemDefinitions` array
3. Đảm bảo có UI slot tương ứng trong `itemSlots` array

---

### 3. Shop/IAP integration

**Flow:**
- Shop → Click "Buy Item" → Gọi `ItemManager.AddQuantity(itemId, amount)` → Save
- ItemSlotUI tự động refresh khi quantity thay đổi

---

### 4. Unlock logic

**Cách unlock:**
```csharp
// Ví dụ: unlock khi đạt level 5
if (playerLevel >= 5)
{
    megaHoleItem.IsLocked = false; // Lưu ý: thay đổi SO runtime sẽ persist khi save scene
}
```

**Alternative:** Lưu unlock state trong PlayerData thay vì thay đổi SO.

---

## H. Validation Checklist

### Compile Check
- [ ] Mở Unity
- [ ] Không có compile error trong Console
- [ ] Tất cả script đều có namespace và summary

### NullReference Check
- [ ] Play scene
- [ ] Không có NullReferenceException trong Console
- [ ] Verify tất cả SerializeField đã kéo đúng object trong Inspector

### Lifecycle Check
- [ ] ItemManager được inject đúng vào GameplayPanel
- [ ] ItemSlotUI register/unregister listener đúng (OnEnable/OnDisable)
- [ ] MagnetEffect/BombShieldEffect tự destroy sau duration
- [ ] Event listeners không bị leak (check khi play/stop scene nhiều lần)

### Functional Check
- [ ] Click item slot → effect apply đúng
- [ ] Quantity giảm sau khi use
- [ ] Save/Load persistent đúng
- [ ] Locked item không thể use
- [ ] Item với quantity = 0 không thể use
- [ ] Multiple effects trong cùng 1 item apply cả 2
- [ ] Bomb shield protect đúng
- [ ] Magnet hút objects đúng
- [ ] Editor cheat keys hoạt động (chỉ trong Editor)

---

## I. Troubleshooting

### Vấn đề: ItemManager không inject được vào GameplayPanel
**Nguyên nhân:** Chưa đăng ký trong VContainer
**Giải pháp:** Verify bước C.2, đảm bảo `builder.RegisterComponentInHierarchy<ItemManager>()` trong GameplayLifetimeScope

---

### Vấn đề: Quantity không được lưu khi stop scene
**Nguyên nhân:** SaveManager.Save() không được gọi sau khi UseItem()
**Giải pháp:** Verify ItemManager.UseItem() có dòng `saveManager.Save().Forget()`

---

### Vấn đề: Magnet không hút objects
**Nguyên nhân:** Objects không nằm trên Layer 9
**Giải pháp:** Đảm bảo obstacles có Rigidbody và nằm trên **Layer 9 (Swallowable)**

---

### Vấn đề: BombShield không hoạt động
**Nguyên nhân:** GameplayController chưa subscribe event OnBombSwallowedWithoutShield
**Giải pháp:** Thêm logic subscribe event trong GameplayController (xem bước F.4)

---

### Vấn đề: UI không cập nhật khi quantity thay đổi
**Nguyên nhân:** ItemManager không fire event OnItemUsed
**Giải pháp:** Verify GameplayPanel.Start() có subscribe `itemManager.OnItemUsed += OnItemUsedHandler`

---

## J. Best Practices

1. **Đặt tên ItemId unique:** Tránh conflict giữa các item, dùng snake_case (ví dụ: `"mega_hole"`)
2. **DefaultAmount hợp lý:** Không set quá cao (ví dụ: 999) để test, dùng 3-5 là đủ
3. **Không hardcode:** Mọi config (duration, pull radius, force) đều lưu trong SO, không hardcode trong script
4. **Test trên build:** Editor cheat keys không hoạt động trên build, cần test manual hoặc tích hợp Shop
5. **Layer 9 cho Swallowable:** Magnet chỉ ảnh hưởng Layer 9, đảm bảo obstacles đúng layer

---

## K. Summary

**Files cần tạo trong Editor:**
- 4 Effect SO assets (bước A)
- 4+ ItemDefinition SO assets (bước B)
- 1 ItemManager GameObject trong GameplayScene (bước C)
- 4 ItemSlotUI components trong GameplayPanel (bước D)
- 1 EditorItemCheatController GameObject (optional, bước E)

**Dependencies:**
- HoleController (existing)
- GameTimer (existing)
- SaveManager (existing)
- PlayerData.itemQuantities (đã thêm)

**Kết quả:**
- Player có thể click UI slot để use item
- Effect apply đúng (tăng size / thêm time / magnet / bomb shield)
- Quantity giảm và được lưu persistent
- Dễ mở rộng: thêm effect mới không cần sửa ItemManager

---

**Hoàn tất hệ thống Item/Power-up.**
