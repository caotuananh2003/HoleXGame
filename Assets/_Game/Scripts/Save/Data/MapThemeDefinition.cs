using UnityEngine;

[CreateAssetMenu(fileName = "MapTheme Definition", menuName = "Definition/MapTheme Definition")]
public class MapThemeDefinition : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite enableIcon;
    [SerializeField] private Sprite disableIcon;
    [SerializeField] private int price;
    [SerializeField] private bool unlockedByDefault = true;


    public string Id => id;
    public string DisplayName => displayName;
    public Sprite EnableIcon => enableIcon;
    public Sprite DisableIcon => disableIcon;
    public int Price => price;
    public bool UnlockedByDefault => unlockedByDefault;
}
