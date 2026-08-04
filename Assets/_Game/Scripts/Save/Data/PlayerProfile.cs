using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject trung tâm — chứa tất cả Definition databases của game.
/// Đóng vai trò Master Database, không phải runtime player data.
/// 
/// Runtime player data (avatar được chọn, frame được chọn, v.v.)
/// được lưu trong SaveManager.Data.profile (ProfileData).
/// </summary>
[CreateAssetMenu(fileName = "PlayerProfile", menuName = "Scriptable Objects/PlayerProfile")]
public class PlayerProfile : ScriptableObject
{
    [Header("Gameplay Definitions")]
    public HoleSkinDatabase HoleSkinDatabase;
    public MapThemeDatabase MapThemeDatabase;
    public BundleDatabase BundleDatabase;
    public List<ItemDefinition> ItemDefinitionList; // Lam phan nay sau

    [Header("Profile Definitions")]
    public AvatarDatabase AvatarDatabase;
    public FrameDatabase FrameDatabase;
    public BadgeDatabase BadgeDatabase;
}
