using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "MapThemeDatabase",
    menuName = "Database/MapTheme Database")]
public class MapThemeDatabase : ScriptableObject
{
    [SerializeField]
    private List<MapThemeDefinition> mapThemeDefinition = new();

    public IReadOnlyList<MapThemeDefinition> MapThemeDefinition => mapThemeDefinition;

    public MapThemeDefinition GetById(string id)
    {
        return mapThemeDefinition.Find(x => x.Id == id);
    }

}