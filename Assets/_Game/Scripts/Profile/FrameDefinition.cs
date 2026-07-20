using UnityEngine;

[CreateAssetMenu(
    fileName = "ProfileDefinition",
    menuName = "HoleXGame/Frame Definition")]
public class FrameDefinition : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private Sprite frame;

    public string Id => id;
    public Sprite Frame => frame;
}