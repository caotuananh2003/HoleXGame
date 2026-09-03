using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class AudioClipEntry
{
    public string id;
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private void Awake()     { Instance = this; }
    private void OnDestroy() { if (Instance == this) Instance = null; }

    private const string BGMVolumeParameter = "BGMVolume";
    private const string SFXVolumeParameter = "SFXVolume";
    private const float  MutedDecibel       = -80f;

    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Database")]
    [SerializeField] private AudioClipDatabase audioClipDatabase;

    private readonly Dictionary<string, AudioClip> clipsById = new();
    private string currentBgmId;

    private float preMuteBGMVolume = 1f;
    private float preMuteSFXVolume = 1f;

    public bool IsBGMMuted =>
        SaveManager.Instance?.PlayerData != null && SaveManager.Instance.PlayerData.bgmVolume <= 0f;
    public bool IsSFXMuted =>
        SaveManager.Instance?.PlayerData != null && SaveManager.Instance.PlayerData.sfxVolume <= 0f;
    public bool IsVibrationEnabled =>
        SaveManager.Instance?.PlayerData == null || SaveManager.Instance.PlayerData.isVibrationEnabled;

    public void Initialize()
    {
        EnsureAudioSources();
        BuildClipCache();

        if (SaveManager.Instance?.PlayerData == null) // SaveManager can khoi tao PlayerData truoc khi Audiomanager Init.
        {
            Debug.LogWarning("AudioManager initialized before SaveManager data was ready.");
            return;
        }

        audioMixer.SetFloat(BGMVolumeParameter, LinearToDecibel(SaveManager.Instance.PlayerData.bgmVolume));
        audioMixer.SetFloat(SFXVolumeParameter, LinearToDecibel(SaveManager.Instance.PlayerData.sfxVolume));
    }

    public void PlaySFX(string id)
    {
        if (TryGetClip(id, out AudioClip clip))
            sfxSource.PlayOneShot(clip);
    }

    public void PlayBGM(string id)
    {
        if (!TryGetClip(id, out AudioClip clip)) return;
        if (currentBgmId == id && bgmSource.isPlaying) return;

        currentBgmId   = id;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void SetBGMVolume(float volume)
    {
        float v = Mathf.Clamp01(volume);
        audioMixer.SetFloat(BGMVolumeParameter, LinearToDecibel(v));
        if (SaveManager.Instance?.PlayerData != null)
        {
            SaveManager.Instance.PlayerData.bgmVolume = v;
            SaveManager.Instance.Save().Forget();
        }
    }

    public void SetSFXVolume(float volume)
    {
        float v = Mathf.Clamp01(volume);
        audioMixer.SetFloat(SFXVolumeParameter, LinearToDecibel(v));
        if (SaveManager.Instance?.PlayerData != null)
        {
            SaveManager.Instance.PlayerData.sfxVolume = v;
            SaveManager.Instance.Save().Forget();
        }
    }

    public void SetBGMMuted(bool mute)
    {
        if (mute)
        {
            if (SaveManager.Instance?.PlayerData != null)
                preMuteBGMVolume = SaveManager.Instance.PlayerData.bgmVolume > 0f
                    ? SaveManager.Instance.PlayerData.bgmVolume : 1f;
            SetBGMVolume(0f);
        }
        else
        {
            SetBGMVolume(preMuteBGMVolume);
        }
    }

    public void SetSFXMuted(bool mute)
    {
        if (mute)
        {
            if (SaveManager.Instance?.PlayerData != null)
                preMuteSFXVolume = SaveManager.Instance.PlayerData.sfxVolume > 0f
                    ? SaveManager.Instance.PlayerData.sfxVolume : 1f;
            SetSFXVolume(0f);
        }
        else
        {
            SetSFXVolume(preMuteSFXVolume);
        }
    }

    public void SetVibration(bool enabled)
    {
        if (SaveManager.Instance?.PlayerData == null) return;
        SaveManager.Instance.PlayerData.isVibrationEnabled = enabled;
        SaveManager.Instance.Save().Forget();
    }

    #region Helpers
    private void EnsureAudioSources()
    {
        if (bgmSource == null) bgmSource = CreateAudioSource("BGM Source", true,  "BGM");
        if (sfxSource == null) sfxSource = CreateAudioSource("SFX Source", false, "SFX");
    }

    private AudioSource CreateAudioSource(string sourceName, bool loop, string mixerGroupName = null)
    {
        var go     = new GameObject(sourceName);
        go.transform.SetParent(transform);
        var source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop        = loop;

        if (audioMixer != null && !string.IsNullOrEmpty(mixerGroupName))
        {
            AudioMixerGroup[] groups = audioMixer.FindMatchingGroups(mixerGroupName);
            if (groups.Length > 0) source.outputAudioMixerGroup = groups[0];
        }
        return source;
    }

    private void BuildClipCache()
    {
        clipsById.Clear();
        if (audioClipDatabase == null) return;
        foreach (AudioClipEntry entry in audioClipDatabase.Clips)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.id) || entry.clip == null) continue;
            clipsById[entry.id] = entry.clip;
        }
    }

    private bool TryGetClip(string id, out AudioClip clip)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            clip = null;
            Debug.LogWarning("Audio id is empty.");
            return false;
        }
        if (clipsById.TryGetValue(id, out clip)) return true;
        Debug.LogWarning($"Audio clip id '{id}' was not found.");
        return false;
    }

    private static float LinearToDecibel(float v)
    {
        if (v <= 0.0001f) return MutedDecibel;
        return Mathf.Clamp(20f * Mathf.Log10(v), MutedDecibel, 0f);
    }
    #endregion
}
