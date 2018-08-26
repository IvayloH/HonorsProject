using UnityEngine;

/// <summary>
/// Set-up the connection rules between the room types;
/// </summary>
[AddComponentMenu("Room-Based Generator Scripts/Room Scripts/Connection Rules Script")]
public class ConnectionRules : MonoBehaviour
{
    [Tooltip("To what types of rooms can a Hall connect to?")]
    [SerializeField] private Room.RoomTypes[] hallConnectionRules;
    
    [Tooltip("To what types of rooms can a Junction connect to?")]
    [SerializeField] private Room.RoomTypes[] junctionConnectionRules;
    
    [Tooltip("To what types of rooms can a Corridor connect to?")]
    [SerializeField] private Room.RoomTypes[] corridorConnectionRules;

    /// <summary>
    /// Use default connection rules if none have been set up
    /// </summary>
    void Awake()
    {
        if (hallConnectionRules.Length == 0)
            hallConnectionRules = new Room.RoomTypes[2] { Room.RoomTypes.Corridor, Room.RoomTypes.Junction };

        if (junctionConnectionRules.Length == 0)
            junctionConnectionRules = new Room.RoomTypes[2] { Room.RoomTypes.Corridor, Room.RoomTypes.Hall };

        if (corridorConnectionRules.Length == 0)
            corridorConnectionRules = new Room.RoomTypes[2] { Room.RoomTypes.Hall, Room.RoomTypes.Junction };
    }

    /// <summary>
    /// Returns an array of the room types that the parameter type can connect to.
    /// </summary>
    /// <returns>The connections possible to the parameter type.</returns>
    public Room.RoomTypes[] GetPossibleConnectionBetweenRoomsByRoomType(Room.RoomTypes roomType)
    {
        switch(roomType)
        {
            case Room.RoomTypes.Hall:
                return hallConnectionRules;

            case Room.RoomTypes.Corridor:
                return corridorConnectionRules;

            case Room.RoomTypes.Junction:
                return junctionConnectionRules;

            default:
                return null;
        }
    }
}