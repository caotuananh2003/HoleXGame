using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "AvatarDatabase",
    menuName = "Database/Avatar Database")]
public class AvatarDatabase : ScriptableObject
{
    [SerializeField]
    private List<AvatarDefinition> avatars = new();

    public IReadOnlyList<AvatarDefinition> Avatars => avatars;

    public AvatarDefinition GetById(string id)
    {
        return avatars.Find(x => x.Id == id);
    }
}