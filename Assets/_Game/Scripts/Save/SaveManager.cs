using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;
    public int highscore;
    public int medals;
}

public class SaveManager : MonoBehaviour
{
    private const string SaveKey = "HOLEXGAME_PLAYER_DATA";

    public PlayerData Data { get; private set; }

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

        // PlayerPrefs la Unity API, nen thao tac luu duoc giu tren main thread.
        await UniTask.SwitchToMainThread();

        PlayerPrefs.SetString(SaveKey, encodedJson);
        PlayerPrefs.Save();

        await UniTask.Yield();
    }

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
            medals = 0
        };
    }
}
