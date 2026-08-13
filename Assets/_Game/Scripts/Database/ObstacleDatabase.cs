using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ObstacleDatabase",
    menuName = "Database/Obstacle Database")]
public class ObstacleDatabase : ScriptableObject
{
    [SerializeField]
    private List<ObstacleDefinition> obstacleDefinition = new();

    public IReadOnlyList<ObstacleDefinition> ObstacleDefinition => obstacleDefinition;

    public ObstacleDefinition GetById(string id)
    {
        return obstacleDefinition.Find(x => x.Id == id);
    }

}