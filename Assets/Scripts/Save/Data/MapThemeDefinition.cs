using UnityEngine;

[CreateAssetMenu(fileName = "MapTheme Definition", menuName = "Definition/MapTheme Definition")]
public class MapThemeDefinition : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;

    [Tooltip("Icon hiển thị trong Shop khi item đang được chọn / active.")]
    [SerializeField] private Sprite enableIcon;

    [Tooltip("Icon hiển thị trong Shop khi item chưa được chọn / inactive.")]
    [SerializeField] private Sprite disableIcon;

    [Tooltip("Material áp dụng lên mesh của map trong gameplay.")]
    [SerializeField] private Material mapMaterial;

    [SerializeField] private int price;
    [SerializeField] private bool unlockedByDefault = true;

    public string Id => id;
    public string DisplayName => displayName;
    public Sprite EnableIcon => enableIcon;
    public Sprite DisableIcon => disableIcon;
    public Material MapMaterial => mapMaterial;
    public int Price => price;
    public bool UnlockedByDefault => unlockedByDefault;
}
