using UnityEngine;

[CreateAssetMenu(fileName = "HoleSkinDefinition", menuName = "Definition/Hole Skin Definition")]
public class HoleSkinDefinition : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;

    [Tooltip("Icon hiển thị trong Shop.")]
    [SerializeField] private Sprite shopDisplayIcon;

    [Tooltip("Icon (sprite) áp dụng lên SpriteRenderer của hole trong gameplay.")]
    [SerializeField] private Sprite gameplayDisplayIcon;

    [SerializeField] private bool unlockedByDefault = true;
    //public Material material;
    [SerializeField] private int price;

    public string Id => id;
    public string DisplayName => displayName;
    public Sprite ShopDisplayIcon => shopDisplayIcon;
    public Sprite GameplayDisplayIcon => gameplayDisplayIcon;
    public bool UnlockedByDefault => unlockedByDefault;
    public int Price => price;
}
