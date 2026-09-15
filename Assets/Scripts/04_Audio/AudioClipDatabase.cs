using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Quy tắc đặt tên id trong Database
/// Generator dùng prefix để phân loại:
/// Prefix Class sinh ra	Ví dụ
/// bgm_	AudioID.BGM	bgm_gameplay → AudioID.BGM.Gameplay
/// sfx_	AudioID.SFX	sfx_item_shield → AudioID.SFX.ItemShield
/// Không có prefix	AudioID.Other	ambient → AudioID.Other.Ambient
/// </summary>
[CreateAssetMenu(fileName = "AudioClipDatabase", menuName = "Database/Audio Clip Database")]
public class AudioClipDatabase : ScriptableObject
{
    [SerializeField] private AudioClipEntry[] clips;

    public IReadOnlyList<AudioClipEntry> Clips => clips;
}