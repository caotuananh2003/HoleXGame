using System;
using UnityEngine;

/// <summary>
/// Quản lý load/save PlayerData.
/// Dữ liệu được encode Base64(JSON) và lưu vào PlayerPrefs.
/// </summary>
public class SaveManager : MonoBehaviour
{
    /// <summary>Dữ liệu của người chơi. Luôn có giá trị sau khi Awake() chạy xong.</summary>
    public PlayerData PlayerData { get; private set; }

    public static SaveManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        PlayerData = Load();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
#if UNITY_EDITOR
            Debug.Log("[SaveManager] OnDestroy — Editor mode, saving...");
#endif
            if (PlayerData != null)
            {
                Save();
            }

            Instance = null;
        }
    }

    private const string SaveKey = "HOLEXGAME_PLAYER_DATA";


    // =========================================================================
    // Public API
    // =========================================================================

    public void Save()
    {
        if (PlayerData == null)
        {
            Debug.LogWarning("[SaveManager] Save: PlayerData is null, skipping save.");
            return;
        }

        try
        {
            PlayerData.PrepareForSerialization();

            string json        = JsonUtility.ToJson(PlayerData);
            string encodedJson = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));

            PlayerPrefs.SetString(SaveKey, encodedJson);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveManager] Save failed: {ex.Message}");
        }
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

    private PlayerData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            PlayerData defaultData = CreateDefaultData();
            PlayerData = defaultData;
            Save();
            return defaultData;
        }

        try
        {
            string encodedJson = PlayerPrefs.GetString(SaveKey);
            string json        = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedJson));
            PlayerData loadedData = JsonUtility.FromJson<PlayerData>(json);

            if (loadedData != null && loadedData.profileData == null)
            {
                loadedData.profileData = new ProfileData();
            }

            if (loadedData != null)
            {
                loadedData.AfterDeserialization();
            }

            if (loadedData != null)
            {
                return loadedData;
            }
            return CreateDefaultData();
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Save data is invalid. A new save will be created. {exception.Message}");

            PlayerData defaultData = CreateDefaultData();
            PlayerData = defaultData;
            Save();
            return defaultData;
        }
    }

    private static PlayerData CreateDefaultData()
    {
        return new PlayerData
        {
            bgmVolume          = 1f,
            sfxVolume          = 1f,
            isVibrationEnabled = true,
            currentLevelIndex  = -1,
            profileData        = new ProfileData(),
            currency           = 5000,
            lives              = 10,
        };
    }
}
