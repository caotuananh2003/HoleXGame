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

[CreateAssetMenu(fileName = "AudioClipDatabase", menuName = "Database/Audio Clip Database")]
public class AudioClipDatabase : ScriptableObject
{
    [SerializeField] private AudioClipEntry[] clips;

    public IReadOnlyList<AudioClipEntry> Clips => clips;
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
    [SerializeField] private AudioClipDatabase audioClipDatabase;

    private readonly Dictionary<string, AudioClip> clipsById = new();
    private SaveManager saveManager;
    private string currentBgmId;

    // Luu volume truoc khi mute de co the restore
    private float preMuteBGMVolume = 1f;
    private float preMuteSFXVolume = 1f;

    public bool IsBGMMuted => saveManager?.Data != null && saveManager.Data.bgmVolume <= 0f;
    public bool IsSFXMuted => saveManager?.Data != null && saveManager.Data.sfxVolume <= 0f;

    [Inject]
    private void Construct(SaveManager saveManager)
    {
        this.saveManager = saveManager;
    }

    public void Initialize()
    {
        Debug.Log("AudioManager initialized.");
        EnsureAudioSources();
        BuildClipCache();

        if (saveManager?.Data == null)
        {
            Debug.LogWarning("AudioManager initialized before SaveManager data was ready.");
            return;
        }

        ApplyMixerVolume(BGMVolumeParameter, saveManager.Data.bgmVolume);
        ApplyMixerVolume(SFXVolumeParameter, saveManager.Data.sfxVolume);
    }

    public void PlaySFX(string id)
    {
        if (!TryGetClip(id, out AudioClip clip))
            return;

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

    public void StopBGM()
    {
        currentBgmId = null;
        bgmSource.Stop();
        bgmSource.clip = null;
    }

    public void SetBGMVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);
        ApplyMixerVolume(BGMVolumeParameter, clampedVolume);

        if (saveManager?.Data == null)
            return;

        saveManager.Data.bgmVolume = clampedVolume;
        saveManager.Save().Forget();
    }

    public void SetSFXVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);
        ApplyMixerVolume(SFXVolumeParameter, clampedVolume);

        if (saveManager?.Data == null)
            return;

        saveManager.Data.sfxVolume = clampedVolume;
        saveManager.Save().Forget();
    }

    /// <summary>
    /// Tat/bat BGM. Khi bat lai se restore ve volume truoc do.
    /// </summary>
    public void SetBGMMuted(bool mute)
    {
        if (mute)
        {
            if (saveManager?.Data != null)
                preMuteBGMVolume = saveManager.Data.bgmVolume > 0f ? saveManager.Data.bgmVolume : 1f;

            SetBGMVolume(0f);
        }
        else
        {
            SetBGMVolume(preMuteBGMVolume);
        }
    }

    /// <summary>
    /// Tat/bat SFX. Khi bat lai se restore ve volume truoc do.
    /// </summary>
    public void SetSFXMuted(bool mute)
    {
        if (mute)
        {
            if (saveManager?.Data != null)
                preMuteSFXVolume = saveManager.Data.sfxVolume > 0f ? saveManager.Data.sfxVolume : 1f;

            SetSFXVolume(0f);
        }
        else
        {
            SetSFXVolume(preMuteSFXVolume);
        }
    }

    private void EnsureAudioSources()
    {
        if (bgmSource == null)
        {
            Debug.Log("Creating bgmSource");
            bgmSource = CreateAudioSource("BGM Source", true, "BGM");
        }

        if (sfxSource == null)
        {
            Debug.Log("Creating sfxSource");
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

    private void BuildClipCache()
    {
        clipsById.Clear();

        if (audioClipDatabase == null)
            return;

        foreach (AudioClipEntry entry in audioClipDatabase.Clips)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.id) || entry.clip == null)
                continue;

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

        if (clipsById.TryGetValue(id, out clip))
            return true;

        Debug.LogWarning($"Audio clip id '{id}' was not found.");
        return false;
    }

    private void ApplyMixerVolume(string parameterName, float linearVolume)
    {
        if (audioMixer == null)
            return;

        audioMixer.SetFloat(parameterName, LinearToDecibel(linearVolume));
    }

    private static float LinearToDecibel(float linearVolume)
    {
        if (linearVolume <= 0.0001f)
            return MutedDecibel;

        // AudioMixer nhan dB: 1 -> 0dB, 0.5 -> khoang -6dB, 0 -> -80dB.
        return Mathf.Clamp(20f * Mathf.Log10(linearVolume), MutedDecibel, 0f);
    }
}
