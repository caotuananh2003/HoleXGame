using System;
using System.Collections.Generic;

/// <summary>
/// Toàn bộ dữ liệu runtime của người chơi được serialize xuống PlayerPrefs.
///
/// Backward-compatible: khi load save cũ (chưa có field mới),
/// JsonUtility.FromJson bỏ qua field thiếu và giữ giá trị default.
/// </summary>
[Serializable]
public class PlayerData
{
    public float bgmVolume          = 1f;
    public float sfxVolume          = 1f;
    public bool  isVibrationEnabled = true;

    /// <summary>
    /// Index level hiện tại của player (0-based).
    /// Tăng lên 1 mỗi khi qua màn, dùng mod % để quay vòng.
    /// Giá trị -1 = chưa từng chơi, GameplayController sẽ dùng startLevelIndex.
    /// </summary>
    public int currentLevelIndex = -1;

    /// <summary>
    /// Dữ liệu profile người chơi: avatar, frame, badge được chọn.
    /// JsonUtility serialize nested class đúng khi class đó có [Serializable].
    /// </summary>
    public ProfileData profileData = new ProfileData();

    // ── Shop — item đang trang bị ─────────────────────────────────────────────
    /// <summary>ID của HoleSkin đang trang bị. Rỗng = dùng default.</summary>
    public string equippedHoleSkinId = "";

    /// <summary>ID của MapTheme đang trang bị. Rỗng = dùng default.</summary>
    public string equippedMapThemeId = "";

    // ── Shop — item đã mua/unlock ─────────────────────────────────────────────
    /// <summary>Danh sách ID HoleSkin đã được mua (ngoài các item unlockedByDefault).</summary>
    public List<string> unlockedHoleSkinIds = new();

    /// <summary>Danh sách ID MapTheme đã được mua (ngoài các item unlockedByDefault).</summary>
    public List<string> unlockedMapThemeIds = new();

    // ── Economy ───────────────────────────────────────────────────────────────
    /// <summary>Lượng currency hiện có. Mặc định 5000.</summary>
    public int currency = 5000;

    /// <summary>Lượng lives hiện có. Mặc định 10.</summary>
    public int lives = 10;

    // ── Items — Số lượng item hiện tại ────────────────────────────────────────
    /// <summary>
    /// Số lượng item hiện có của player (runtime data).
    /// Key = itemId (string), Value = current quantity (int).
    /// Không serialize dictionary trực tiếp — dùng helper list cho JsonUtility.
    /// </summary>
    [NonSerialized]
    public Dictionary<string, int> itemQuantyDict = new();

    // Helper cho JsonUtility serialize Dictionary
    [Serializable]
    public struct ItemQuantityEntry
    {
        public string itemId;
        public int quantity;
    }

    public List<ItemQuantityEntry> itemQuantityList = new();

    // ── Items — Unlock state ──────────────────────────────────────────────────
    /// <summary>
    /// Danh sách itemId đã được unlock bởi việc đạt đủ hole level.
    /// Backward-compatible: save cũ không có field này → list rỗng → item vẫn locked.
    /// </summary>
    public List<string> unlockedItemIds = new();

    /// <summary>
    /// Gọi trước khi serialize (trong SaveManager.Save()).
    /// Biến Dictionary thành List<ItemQuantityEntry> để JsonUtility serialize được.
    /// </summary>
    public void PrepareForSerialization()
    {
        itemQuantityList.Clear();
        foreach (var item in itemQuantyDict)
        {
            itemQuantityList.Add(new ItemQuantityEntry { itemId = item.Key, quantity = item.Value });
        }
    }

    /// <summary>
    /// Gọi sau khi deserialize (trong SaveManager.Load()).
    /// Convert List → Dictionary để dùng runtime.
    /// Đảm bảo unlockedItemIds không null khi load save cũ.
    /// </summary>
    public void AfterDeserialization()
    {
        itemQuantyDict.Clear();
        foreach (var entry in itemQuantityList)
        {
            itemQuantyDict[entry.itemId] = entry.quantity;
        }

        // Guard: save cũ không có field này → JsonUtility để null
        if (unlockedItemIds == null)
            unlockedItemIds = new List<string>();
    }
}