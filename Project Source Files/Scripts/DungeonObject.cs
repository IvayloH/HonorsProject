using UnityEngine;

/// <summary>
/// Class defines the spawnable objects in the world.
/// Uses an enum to differentiate which object has a chance of spawning somewhere.
/// </summary>
[AddComponentMenu("Room-Based Generator Scripts/Dungeon Object Scripts/Dungeon Object")]
public class DungeonObject : MonoBehaviour
{
    /// <summary>
    /// Defines where a DungeonObject has a chance of spawning.
    /// </summary>
    public enum ObjectLocation
    {
        Ground,
        Wall,
        Ceiling
    };
    
    [Tooltip("Where should this object have a chance of spawning?")]
    [SerializeField] private ObjectLocation objectLocation;

    [Range(1,100)]
    [Tooltip("How likely is it for this object to spawn in the level?")]
    [SerializeField] private int spawnChance = 25;

    /// <summary>
    /// Returns the location of the object.
    /// </summary>
    /// <returns>Object's location.</returns>
    public ObjectLocation GetObjectLocation()
    {
        return objectLocation;
    }

    /// <summary>
    /// Retrieve the spawn chance for this object.
    /// </summary>
    /// <returns>The spawn chance of the object.</returns>
    public int GetSpawnChance()
    {
        return spawnChance;
    }
}
