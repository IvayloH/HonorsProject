using UnityEngine;

/// <summary>
/// Class defines the location of where an object can spawn using the same ENUM from the DungeonObject class.
/// </summary>
[AddComponentMenu("Room-Based Generator Scripts/Dungeon Object Scripts/Dungeon Object Placement")]
public class DungeonObjectPlacement : MonoBehaviour
{
    [Header("Object Placement Settings")]
    [Tooltip("What kind of objects will be spawned here?")]
    [SerializeField] private DungeonObject.ObjectLocation location;
    [Tooltip("Object has a chance of spawning here only if a specific exit is closed.")]
    [SerializeField] private bool spawnOnlyIfExitClosed = false;
    [Tooltip("Which exit should be checked to determine whether to spawn an item here?")]
    [SerializeField] private RoomConnector exitToCheck = null;

    [Space(10)]
    [Header("Gizmos Settings")]
    [SerializeField] private float GizmosScale = 1.0f;

    /// <summary>
    /// Represent the object placments in each of the rooms so they can easily be adjusted in the editor.
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * GizmosScale);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position - transform.right * GizmosScale);
        Gizmos.DrawLine(transform.position, transform.position + transform.right * GizmosScale);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * GizmosScale);

        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(transform.position, new Vector3(0.3f, 0.3f, 0.3f));
    }

    /// <summary>
    /// Returns the ObjectLocation that was specified by the user in the editor.
    /// </summary>
    /// <returns></returns>
    public DungeonObject.ObjectLocation GetPlacementLocation()
    {
        return location;
    }

    /// <summary>
    /// Retrieves the exit that has been specified by the user.
    /// </summary>
    /// <returns>RoomConnector object or null.</returns>
    public RoomConnector GetAssociatedExit()
    {
        return exitToCheck;
    }

    /// <summary>
    /// Returns false if a chest cannot be spawned. Otherwise, returns true.
    /// </summary>
    /// <returns></returns>
    public bool IsSpawningRestricted()
    {
        if (!spawnOnlyIfExitClosed)
            return false;

        bool restricted = false;
        foreach(var roomExit in GetComponentInParent<Room>().GetRoomExits())
        {
            if (roomExit.GetExit().Equals(exitToCheck))
                if (roomExit.IsExitConnected())
                    restricted = true;
        }
        return restricted;
    }
}
