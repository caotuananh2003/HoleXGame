using UnityEngine;

[CreateAssetMenu(fileName = "Hole Definition", menuName = "Definition/Hole Definition")]
public class HoleSkinDefinition : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private bool unlockedByDefault = true;
    //public Material material;
    [SerializeField] private int price;

    public string Id => id;
    public string DisplayName => displayName;
    public bool UnlockedByDefault => unlockedByDefault;
    public Sprite Icon => icon;
    public int Price => price;
}
