using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "HoleSkinDatabase",
    menuName = "Database/HoleSkin Database")]
public class HoleSkinDatabase : ScriptableObject
{
    [SerializeField]
    private List<HoleSkinDefinition> holeDefinition = new();

    public IReadOnlyList<HoleSkinDefinition> HoleDefinition => holeDefinition;

    public HoleSkinDefinition GetById(string id)
    {
        return holeDefinition.Find(x => x.Id == id);
    }
}