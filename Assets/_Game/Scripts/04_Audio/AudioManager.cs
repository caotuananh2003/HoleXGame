using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using VContainer;

[Serializable]
public class AudioClipEntry
{
    public string id;
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    private const string BGMVolumeParameter = "BGMVolume";
    private const string SFXVolumeParameter = "SFXVolume";
    private const float MutedDecibel = -80f;

    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Database")]
    [SerializeField] private AudioClipDatabase audioClipDatabase; // Scriptable Object Lưu các clip và id tương ứng.

    private readonly Dictionary<string, AudioClip> clipsById = new();
    private SaveManager saveManager;
    private string currentBgmId;

    // Luu volume truoc khi mute de co the restore
    private float preMuteBGMVolume = 1f;
    private float preMuteSFXVolume = 1f;

    public bool IsBGMMuted => saveManager?.PlayerData != null && saveManager.PlayerData.bgmVolume <= 0f; // Readonly, trả về BGM có mute hay không
    public bool IsSFXMuted => saveManager?.PlayerData != null && saveManager.PlayerData.sfxVolume <= 0f; // Readonly, trả về SFX có mute hay không

    [Inject]
    private void Construct(SaveManager saveManager)
    {
        this.saveManager = saveManager;
    }

    public void Initialize()
    {
        //Debug.Log("AudioManager initialized.");
        EnsureAudioSources(); // Tạo bgmSource và sfxSource nếu chưa gán trên Inspector (Mặc định không cần gán trên Inspector)
        BuildClipCache(); // Tạo bộ dictionary với key = AudioClipEntry.id và value = AudioClipEntry.clip tương ứng.

        if (saveManager?.PlayerData == null)
        {
            Debug.LogWarning("AudioManager initialized before SaveManager data was ready.");
            return;
        }

        audioMixer.SetFloat(BGMVolumeParameter, LinearToDecibel(saveManager.PlayerData.bgmVolume)); // Tìm parameter có tên BGMVolumeParameter trong AudioMixer và đặt giá trị của nó thành giá trị mới.

        audioMixer.SetFloat(SFXVolumeParameter, LinearToDecibel(saveManager.PlayerData.sfxVolume)); // Tìm parameter có tên SFXVolumeParameter trong AudioMixer và đặt giá trị của nó thành giá trị mới.
    }

    public void PlaySFX(string id)
    {
        if (TryGetClip(id, out AudioClip clip))
            sfxSource.PlayOneShot(clip);
    }

    public void PlayBGM(string id)
    {
        if (!TryGetClip(id, out AudioClip clip))
            return;

        if (currentBgmId == id && bgmSource.isPlaying)
            return;

        currentBgmId = id;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void SetBGMVolume(float volume) // Set volume cho bgm và save lại
    {
        float clampedVolume = Mathf.Clamp01(volume);
        audioMixer.SetFloat(BGMVolumeParameter, LinearToDecibel(clampedVolume));

        if (saveManager?.PlayerData != null)
        {
            saveManager.PlayerData.bgmVolume = clampedVolume;
            saveManager.Save().Forget();
        }
    }

    public void SetSFXVolume(float volume) // Set volume cho sfx và save lại
    {
        float clampedVolume = Mathf.Clamp01(volume);
        audioMixer.SetFloat(SFXVolumeParameter, LinearToDecibel(clampedVolume));

        if (saveManager?.PlayerData != null)
        {
            saveManager.PlayerData.sfxVolume = clampedVolume;
            saveManager.Save().Forget();
        }
    }

    public void SetBGMMuted(bool mute) // Tat/bat BGM. Khi bat lai se restore ve volume truoc do.
    {
        if (mute)
        {
            if (saveManager?.PlayerData != null)
                preMuteBGMVolume = saveManager.PlayerData.bgmVolume > 0f ? saveManager.PlayerData.bgmVolume : 1f;

            SetBGMVolume(0f);
        }
        else
        {
            SetBGMVolume(preMuteBGMVolume);
        }
    }

    public void SetSFXMuted(bool mute) // Tat/bat SFX. Khi bat lai se restore ve volume truoc do.
    {
        if (mute)
        {
            if (saveManager?.PlayerData != null)
                preMuteSFXVolume = saveManager.PlayerData.sfxVolume > 0f ? saveManager.PlayerData.sfxVolume : 1f;

            SetSFXVolume(0f);
        }
        else
        {
            SetSFXVolume(preMuteSFXVolume);
        }
    }

    #region Helper
    private void EnsureAudioSources() // Tạo AudioSource cho bgm và sfx (Trên Inspector không cần kéo ref gì cả)
    {
        if (bgmSource == null)
        {
            bgmSource = CreateAudioSource("BGM Source", true, "BGM");
        }

        if (sfxSource == null)
        {
            sfxSource = CreateAudioSource("SFX Source", false, "SFX");
        }
    }

    private AudioSource CreateAudioSource(string sourceName, bool loop, string mixerGroupName = null)
    {
        GameObject sourceObject = new(sourceName);
        sourceObject.transform.SetParent(transform);

        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;

        // Tu dong gan AudioMixerGroup neu audioMixer da duoc assign
        if (audioMixer != null && !string.IsNullOrEmpty(mixerGroupName))
        {
            AudioMixerGroup[] groups = audioMixer.FindMatchingGroups(mixerGroupName);
            if (groups.Length > 0)
                source.outputAudioMixerGroup = groups[0];
        }

        return source;
    }

    private void BuildClipCache() // Tạo bộ dictionary với key = AudioClipEntry.id và value = AudioClipEntry.clip tương ứng.
    {
        clipsById.Clear(); // Xóa sạch Dictionary

        if (audioClipDatabase == null)
            return;

        // Tạo bộ Dictionary với key = id của clip và value = chính clip đó.
        foreach (AudioClipEntry entry in audioClipDatabase.Clips)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.id) || entry.clip == null)
                continue;

            clipsById[entry.id] = entry.clip;
        }
    }

    private bool TryGetClip(string id, out AudioClip clip) // Thử lấy ra clip với id truyền vào xem trong dict có tồn tại hay không
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            clip = null;
            Debug.LogWarning("Audio id is empty.");
            return false;
        }

        if (clipsById.TryGetValue(id, out clip))
            return true;

        Debug.LogWarning($"Audio clip id '{id}' was not found.");
        return false;
    }

    private static float LinearToDecibel(float linearVolume) // Chuyển giá trị từ 0 -> 1 thành -80Db (Không nghe thấy gì) tới 0Db (Âm thanh bình thường)
    {
        if (linearVolume <= 0.0001f)
            return MutedDecibel;

        // AudioMixer nhan dB: 1 -> 0dB, 0.5 -> khoang -6dB, 0 -> -80dB.
        return Mathf.Clamp(20f * Mathf.Log10(linearVolume), MutedDecibel, 0f);
    }
    #endregion
}
