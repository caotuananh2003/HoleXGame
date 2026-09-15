using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "FrameDatabase",
    menuName = "Database/Frame Database")]
public class FrameDatabase : ScriptableObject
{
    [SerializeField]
    private List<FrameDefinition> frames = new();

    public IReadOnlyList<FrameDefinition> Frames => frames;

    public FrameDefinition GetById(string id)
    {
        return frames.Find(x => x.Id == id);
    }
}