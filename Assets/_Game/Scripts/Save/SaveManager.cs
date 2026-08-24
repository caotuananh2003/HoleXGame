using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Toàn bộ dữ liệu runtime của người chơi được serialize xuống PlayerPrefs.
///
/// Backward-compatible: khi load save cũ (chưa có field mới),
/// JsonUtility.FromJson bỏ qua field thiếu và giữ giá trị default.
/// </summary>
[Serializable]
public class PlayerData
{
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;

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
    public System.Collections.Generic.List<string> unlockedHoleSkinIds = new();

    /// <summary>Danh sách ID MapTheme đã được mua (ngoài các item unlockedByDefault).</summary>
    public System.Collections.Generic.List<string> unlockedMapThemeIds = new();

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
    [System.NonSerialized]
    public System.Collections.Generic.Dictionary<string, int> itemQuantities = new();

    // Helper cho JsonUtility serialize Dictionary
    [System.Serializable]
    public struct ItemQuantityEntry
    {
        public string itemId;
        public int quantity;
    }

    public System.Collections.Generic.List<ItemQuantityEntry> itemQuantityList = new();

    // ── Items — Unlock state ──────────────────────────────────────────────────
    /// <summary>
    /// Danh sách itemId đã được unlock bởi việc đạt đủ hole level.
    /// Backward-compatible: save cũ không có field này → list rỗng → item vẫn locked.
    /// </summary>
    public System.Collections.Generic.List<string> unlockedItemIds = new();

    /// <summary>
    /// Gọi trước khi serialize (trong SaveManager.Save()).
    /// Convert Dictionary → List để JsonUtility serialize được.
    /// </summary>
    public void PrepareForSerialization()
    {
        itemQuantityList.Clear();
        foreach (var kvp in itemQuantities)
        {
            itemQuantityList.Add(new ItemQuantityEntry { itemId = kvp.Key, quantity = kvp.Value });
        }
    }

    /// <summary>
    /// Gọi sau khi deserialize (trong SaveManager.Load()).
    /// Convert List → Dictionary để dùng runtime.
    /// Đảm bảo unlockedItemIds không null khi load save cũ.
    /// </summary>
    public void AfterDeserialization()
    {
        itemQuantities.Clear();
        foreach (var entry in itemQuantityList)
        {
            itemQuantities[entry.itemId] = entry.quantity;
        }

        // Guard: save cũ không có field này → JsonUtility để null
        if (unlockedItemIds == null)
            unlockedItemIds = new System.Collections.Generic.List<string>();
    }
}

/// <summary>
/// Quản lý load/save PlayerData.
/// Dữ liệu được encode Base64(JSON) và lưu vào PlayerPrefs.
/// Inject vào bất kỳ class nào cần đọc/ghi save data qua VContainer.
/// </summary>
public class SaveManager : MonoBehaviour
{
    private const string SaveKey = "HOLEXGAME_PLAYER_DATA";

    /// <summary>Dữ liệu đang được load. Null cho đến khi Initialize() hoàn tất.</summary>
    public PlayerData PlayerData { get; private set; }

    // =========================================================================
    // Public API
    // =========================================================================

    public async UniTask Initialize()
    {
        PlayerData = await Load();
    }

    public async UniTask Save()
    {
        if (PlayerData == null)
            PlayerData = CreateDefaultData();

        // Serialize Dictionary → List trước khi JsonUtility.ToJson
        PlayerData.PrepareForSerialization();

        string json = JsonUtility.ToJson(PlayerData);
        string encodedJson = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));

        // PlayerPrefs là Unity API — thao tác lưu phải ở main thread.
        await UniTask.SwitchToMainThread();

        PlayerPrefs.SetString(SaveKey, encodedJson);
        PlayerPrefs.Save();

        await UniTask.Yield();
    }

    /// <summary>
    /// Xóa toàn bộ save data trên disk và reset PlayerData về default.
    /// Gọi xong thì PlayerData = default, giống như lần đầu chạy game.
    /// Chỉ dùng trong Editor (gọi từ EditorCheatController).
    /// </summary>
    public void DeleteSaveData()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();

        PlayerData = CreateDefaultData();

        Debug.Log("[SaveManager] Save data đã bị xóa. PlayerData reset về default.");
    }

    // =========================================================================
    // Internal
    // =========================================================================

    private async UniTask<PlayerData> Load()
    {
        await UniTask.SwitchToMainThread();

        if (!PlayerPrefs.HasKey(SaveKey))
        {
            PlayerData defaultData = CreateDefaultData();
            PlayerData = defaultData;
            await Save();
            return defaultData;
        }

        try
        {
            string encodedJson = PlayerPrefs.GetString(SaveKey);
            string json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedJson));
            PlayerData loadedData = JsonUtility.FromJson<PlayerData>(json);

            // Đảm bảo profile không null sau khi load save cũ.
            if (loadedData != null && loadedData.profileData == null)
                loadedData.profileData = new ProfileData();

            // Deserialize List → Dictionary
            if (loadedData != null)
                loadedData.AfterDeserialization();

            return loadedData ?? CreateDefaultData();
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Save data is invalid. A new save will be created. {exception.Message}");

            PlayerData defaultData = CreateDefaultData();
            PlayerData = defaultData;
            await Save();
            return defaultData;
        }
    }

    private static PlayerData CreateDefaultData()
    {
        return new PlayerData
        {
            bgmVolume        = 1f,
            sfxVolume        = 1f,
            currentLevelIndex = -1,
            profileData          = new ProfileData(),
            currency         = 5000,
            lives            = 10,
        };
    }
}
