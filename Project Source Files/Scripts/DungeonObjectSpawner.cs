using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles generating dungeon objects at specified locations.
/// Spawns objects that have the same type as an object placement, at the placements location.
/// Takes into consideration what the chance of an object is to spawn - set by the user.
/// </summary>
[AddComponentMenu("Room-Based Generator Scripts/Dungeon Object Scripts/Dungeon Object Spawner")]
public class DungeonObjectSpawner : MonoBehaviour
{
    [Header("Objects to Spawn")]
    [SerializeField] private DungeonObject[] spawnableDungeonObjects;

    private List<DungeonObjectPlacement> groundPlacements;
    private List<DungeonObjectPlacement> wallPlacements;
    private List<DungeonObjectPlacement> ceilingPlacements;
    private List<DungeonObject> objectsList;

    private GameObject dungeonObjectsParent; //empty game object
    private RandomItem rnd;

    /// <summary>
    /// Spawn the dungeon objects in the level.
    /// </summary>
    public void SpawnDungeonObjects()
    {
        if (spawnableDungeonObjects.Length == 0)
            return;
        dungeonObjectsParent = new GameObject("Containers - ");
        dungeonObjectsParent.transform.parent = GameObject.FindGameObjectWithTag("Dungeon").transform;

        rnd = new RandomItem();
        GatherIntel();
        SpawnGroundObjects();
        SpawnWallObjects();
        SpawnCeilingObjects();

        dungeonObjectsParent.name += dungeonObjectsParent.GetComponentsInChildren<DungeonObject>().Length;
    }


    /// <summary>
    /// Examines all of the rooms that were spawned and populates one of the three lists(groundPlacements, wallPlacements, ceilingPlacements),
    /// based on what the type of the placement in the room is.
    /// </summary>
    private void GatherIntel()
    {
        groundPlacements = new List<DungeonObjectPlacement>();
        wallPlacements  = new List<DungeonObjectPlacement>();
        ceilingPlacements = new List<DungeonObjectPlacement>();
        
        foreach(var roomObj in GameObject.FindGameObjectsWithTag("RoomInstance"))
        {
            foreach(var placement in roomObj.GetComponentsInChildren<DungeonObjectPlacement>())
            {
                switch(placement.GetPlacementLocation())
                {
                    case DungeonObject.ObjectLocation.Ground:
                        if (!placement.IsSpawningRestricted() && roomObj.name!="ExitRoom")
                            groundPlacements.Add(placement);
                        break;
                    case DungeonObject.ObjectLocation.Wall:
                        wallPlacements.Add(placement);
                        break;
                    case DungeonObject.ObjectLocation.Ceiling:
                        ceilingPlacements.Add(placement);
                        break;
                    default:
                        break;
                }
                placement.tag = "ObjectPlacement";
            }
        }
    }

    /// <summary>
    /// Spawns objects of the ground type at the locations of the ground type placements.
    /// </summary>
    private void SpawnGroundObjects()
    {
        objectsList = GatherSpecificObjects(DungeonObject.ObjectLocation.Ground);
        if (objectsList.Count == 0 || groundPlacements.Count == 0)
            return;

        Quaternion rotation;
        foreach(var placement in groundPlacements)
        {
            int index = rnd.GetSystemRandom(0, objectsList.Count);
            if (rnd.GetSystemRandom(0, 99) > objectsList[index].GetSpawnChance())
                continue;
            rotation = Quaternion.Euler(objectsList[index].transform.eulerAngles.x,
                                                   objectsList[index].transform.eulerAngles.y + placement.transform.eulerAngles.y,
                                                   objectsList[index].transform.eulerAngles.z);
            Instantiate(objectsList[index], placement.transform.position, rotation, dungeonObjectsParent.transform);
        }
    }

    /// <summary>
    /// Spawns objects of the wall type at the locations of the wall type placements.
    /// </summary>
    private void SpawnWallObjects()
    {
        objectsList = GatherSpecificObjects(DungeonObject.ObjectLocation.Wall);
        if (objectsList.Count == 0 || wallPlacements.Count == 0)
            return;

        Quaternion rotation;
        foreach (var placement in wallPlacements)
        {
            int index = rnd.GetSystemRandom(0, objectsList.Count);
            if (rnd.GetSystemRandom(0, 99) > objectsList[index].GetSpawnChance())
                continue;
            rotation = Quaternion.Euler(objectsList[index].transform.eulerAngles.x,
                                       objectsList[index].transform.eulerAngles.y + placement.transform.eulerAngles.y,
                                       objectsList[index].transform.eulerAngles.z);
            Instantiate(objectsList[index], placement.transform.position, rotation, dungeonObjectsParent.transform);
        }
    }

    /// <summary>
    /// Spawns objects of the ceiling type at the locations of the ceiling type placements.
    /// </summary>
    private void SpawnCeilingObjects()
    {
        objectsList = GatherSpecificObjects(DungeonObject.ObjectLocation.Ceiling);
        if (objectsList.Count == 0 || ceilingPlacements.Count == 0)
            return;

        Quaternion rotation;
        foreach (var placement in ceilingPlacements)
        {
            int index = rnd.GetSystemRandom(0, objectsList.Count);
            if (rnd.GetSystemRandom(0, 99) > objectsList[index].GetSpawnChance())
                continue;
            rotation = Quaternion.Euler(objectsList[index].transform.eulerAngles.x,
                                       objectsList[index].transform.eulerAngles.y + placement.transform.eulerAngles.y,
                                       objectsList[index].transform.eulerAngles.z);
            Instantiate(objectsList[index], placement.transform.position, rotation, dungeonObjectsParent.transform);
        }
    }

    /// <summary>
    /// Looks through the list of objects provided by the user and returns a new list with all of the objects that match the specific location.
    /// </summary>
    /// <param name="location">The ObjectLocation to match.</param>
    /// <returns>A list with objects that match the ObjectLocation passed in.</returns>
    private List<DungeonObject> GatherSpecificObjects(DungeonObject.ObjectLocation location)
    {
        if (spawnableDungeonObjects.Length == 0)
            return default(List<DungeonObject>);

        List<DungeonObject> containers = new List<DungeonObject>();
        foreach(var container in spawnableDungeonObjects)
        {
            if(container.GetObjectLocation().Equals(location))
                containers.Add(container);
        }
        return containers;
    }
}