using UnityEngine;

[CreateAssetMenu(
    fileName = "ProfileDefinition",
    menuName = "HoleXGame/Avatar Definition")]
public class AvatarDefinition : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private Sprite avatar;
    [SerializeField] private bool unlockedByDefault = true;

    public string Id => id;
    public Sprite Avatar => avatar;
    public bool UnlockedByDefault => unlockedByDefault;
}