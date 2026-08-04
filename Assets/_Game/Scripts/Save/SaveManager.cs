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
    // ── Audio ────────────────────────────────────────────────────────────────
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;

    // ── Gameplay stats ────────────────────────────────────────────────────────
    public int highscore;
    public int medals;

    // ── Profile ───────────────────────────────────────────────────────────────
    /// <summary>
    /// Dữ liệu profile người chơi: avatar, frame, badge được chọn.
    /// JsonUtility serialize nested class đúng khi class đó có [Serializable].
    /// </summary>
    public ProfileData profile = new ProfileData();

    // ── Shop — item đang trang bị ─────────────────────────────────────────────
    /// <summary>ID của HoleSkin đang trang bị. Rỗng = dùng default.</summary>
    public string equippedHoleSkinId = "";

    /// <summary>ID của MapTheme đang trang bị. Rỗng = dùng default.</summary>
    public string equippedMapThemeId = "";
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
    public PlayerData Data { get; private set; }

    // =========================================================================
    // Public API
    // =========================================================================

    public async UniTask Initialize()
    {
        Data = await Load();
    }

    public async UniTask Save()
    {
        if (Data == null)
            Data = CreateDefaultData();

        string json = JsonUtility.ToJson(Data);
        string encodedJson = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));

        // PlayerPrefs là Unity API — thao tác lưu phải ở main thread.
        await UniTask.SwitchToMainThread();

        PlayerPrefs.SetString(SaveKey, encodedJson);
        PlayerPrefs.Save();

        await UniTask.Yield();
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
            Data = defaultData;
            await Save();
            return defaultData;
        }

        try
        {
            string encodedJson = PlayerPrefs.GetString(SaveKey);
            string json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedJson));
            PlayerData loadedData = JsonUtility.FromJson<PlayerData>(json);

            // Đảm bảo profile không null sau khi load save cũ.
            if (loadedData != null && loadedData.profile == null)
                loadedData.profile = new ProfileData();

            return loadedData ?? CreateDefaultData();
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Save data is invalid. A new save will be created. {exception.Message}");

            PlayerData defaultData = CreateDefaultData();
            Data = defaultData;
            await Save();
            return defaultData;
        }
    }

    private static PlayerData CreateDefaultData()
    {
        return new PlayerData
        {
            bgmVolume = 1f,
            sfxVolume = 1f,
            highscore = 0,
            medals    = 0,
            profile   = new ProfileData(),
        };
    }
}
