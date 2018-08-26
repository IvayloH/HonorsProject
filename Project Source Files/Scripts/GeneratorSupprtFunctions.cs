using System.Linq;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Contains the functions necessary for the RoomBasedGenerator class to work, but that are not part of the main methods.
/// </summary>
public class GeneratorSupprtFunctions
{
    private RandomItem rnd;
    private Room[] Rooms;
    private Fragment[] wallFragments;

    /// <summary>
    /// Initialize the variables that will be used.
    /// </summary>
    /// <param name="rnd">Instance of the RandomItem object to be used.</param>
    /// <param name="Rooms">The array of room objects to be used.</param>
    /// <param name="Fragments">The array of fragment objects to be used.</param>
    public GeneratorSupprtFunctions(RandomItem rnd, Room[] Rooms, Fragment[] Fragments)
    {
        this.rnd = rnd;
        this.Rooms = Rooms;
        this.wallFragments = Fragments;
    }

    /// <summary>
    /// Goes through all of the instantiated rooms from last created to first, until a room with at least one active exit is found.
    /// </summary>
    /// <returns>First room found with at least one exit.</returns>
    public Room BacktrackToLastRoomWithActiveExits()
    {
        Room room = null;
        GameObject[] roomsList = GameObject.FindGameObjectsWithTag("RoomInstance");
        for (int i = roomsList.Length - 1; i >= 0; i--)
        {
            if (roomsList[i].GetComponent<Room>().GetActiveExits().Length > 0)
            {
                room = roomsList[i].GetComponent<Room>();
                break;
            }
        }
        return room;
    }

    /// <summary>
    /// Goes through all of the instantiated rooms from last created to first, until a room with at least one active exit is found,
    /// but it must also match the rooms connection rules.
    /// </summary>
    /// <param name="rules">Connection rules.</param>
    /// <param name="excludeNumberOfRooms">Number of rooms to exclude from the search(excludes from the end).</param>
    /// <returns>The first room that matches the criteria.</returns>
    public Room BacktrackToLastRoomWithActiveExits(Room.RoomTypes[] rules, ref int excludeNumberOfRooms)
    {
        Room room = null;
        GameObject[] roomsList = GameObject.FindGameObjectsWithTag("RoomInstance");
        if (excludeNumberOfRooms > roomsList.Length - 1)
            return null;

        for (int i = roomsList.Length - 1 - excludeNumberOfRooms; i >= 0; i--)
        {
            if (roomsList[i].GetComponent<Room>().GetActiveExits().Length > 0)
            {
                if (rules.Contains(roomsList[i].GetComponent<Room>().GetRoomType()))
                {
                    room = roomsList[i].GetComponent<Room>();
                    excludeNumberOfRooms = roomsList.Length - 1 - i;
                    break;
                }
            }
        }
        return room;
    }

    /// <summary>
    /// Picks a random type from possibleRoomConnections and keeps picking a random room that matches the type,
    /// until a room has been found that has at least 2 exits.
    /// </summary>
    /// <param name="prefab">Room prefab reference.</param>
    /// <param name="possibleRoomConnections">Array of possible room connections.</param>
    public void AvoidDeadEnd(ref Room prefab, Room.RoomTypes[] possibleRoomConnections)
    {
        int additionalSeed = 1;
        while (prefab.GetComponentsInChildren<RoomConnector>().Length < 2)
        {
            var pickRandomRoomType = rnd.GetRandom(possibleRoomConnections);
            prefab = rnd.GetRandomByRoomType(Rooms, pickRandomRoomType);
            additionalSeed++;
        }
    }

    /// <summary>
    /// Keeps randomly picking a room, with a specified room type, that has more exits to avoid a dead end.
    /// </summary>
    /// <param name="prefab">Room prefab reference.</param>
    /// <param name="pickRandomRoomType">The room type to match.</param>
    public void AvoidDeadEnd(ref Room prefab, Room.RoomTypes pickRandomRoomType)
    {
        int additionalSeed = 1;
        while (prefab.GetComponentsInChildren<RoomConnector>().Length < 2)
        {
            prefab = rnd.GetRandomByRoomType(Rooms, pickRandomRoomType);
            additionalSeed++;
        }
    }

    /// <summary>
    /// Finds all of the exits that are active in all of the rooms.
    /// </summary>
    /// <returns>Returns the number of active exits present in the level.</returns>
    public int GetTotalNumberOfActiveExits()
    {
        int activeExits = 0;
        GameObject[] roomsList = GameObject.FindGameObjectsWithTag("RoomInstance");
        for (int i = 0; i < roomsList.Length; i++)
            activeExits += roomsList[i].GetComponent<Room>().GetActiveExits().Length;

        return activeExits;
    }

    /// <summary>
    /// Finds all the rooms that have an exit that is still active. It also uses rooms that have closed exits due to collision.
    /// This function is used for setting up the exit room which would most likely be smaller than the rest of them and may not cause a collision
    /// where other rooms caused a collision when being added.
    /// </summary>
    /// <returns>Returns the number of rooms that have an exit that can be used to setup the exit room.</returns>
    public List<Room> GetRoomsWithPossibleExits()
    {
        GameObject[] instancedRooms = GameObject.FindGameObjectsWithTag("RoomInstance");
        List<Room> roomsWithAvailableExits = new List<Room>();
        foreach (var room in instancedRooms)
        {
            //find all open exits and add them to the list
            if (room.GetComponent<Room>().GetActiveExits().Length > 0 || room.GetComponent<Room>().IsCollidedExitPresent())
                roomsWithAvailableExits.Add(room.GetComponent<Room>());
        }
        return roomsWithAvailableExits;
    }

    /// <summary>
    /// Finds all the rooms that have an exit that is still active.
    /// </summary>
    /// <returns>Returns the number of rooms that have an active exit.</returns>
    public List<Room> GetAllRoomsWithActiveExits()
    {
        GameObject[] instancedRooms = GameObject.FindGameObjectsWithTag("RoomInstance");
        List<Room> roomsWithAvailableExits = new List<Room>();
        foreach (var room in instancedRooms)
        {
            //find all open exits and add them to the list
            if (room.GetComponent<Room>().GetActiveExits().Length > 0)
                roomsWithAvailableExits.Add(room.GetComponent<Room>());
        }
        return roomsWithAvailableExits;
    }

    /// <summary>
    /// Generates a list of fragments that match the passed in argument.
    /// </summary>
    /// <param name="type">The Fragment type to look for.</param>
    /// <returns>A List of Fragment components that are the same type as the parameter.</returns>
    public List<Fragment> GetFragmentsMatchingType(Fragment.FragmentTypes type)
    {
        List<Fragment> fragments = new List<Fragment>();
        foreach (var fragment in wallFragments)
        {
            if (fragment.GetTypeOfFragment().Equals(type))
                fragments.Add(fragment);
        }
        return fragments;
    }

    /// <summary>
    /// Scans through all of the instantiated rooms and find the RoomExit that contains the specified roomConnector.
    /// </summary>
    /// <param name="exitToFind"></param>
    /// <returns></returns>
    public RoomExit FindRoomExit(RoomConnector exitToFind)
    {
        RoomExit exit = null;
        foreach (var roomObj in GameObject.FindGameObjectsWithTag("RoomInstance"))
        {
            foreach (var roomExit in roomObj.GetComponent<Room>().GetRoomExits())
                if (roomExit.GetExit().Equals(exitToFind))
                {
                    exit = roomExit;
                    break;
                }
        }
        return exit;
    }

    /// <summary>
    /// Teleports the player to a position.
    /// </summary>
    /// <param name="initialPosition">Position to teleport player to.</param>
    public void ResetPlayerPosition(Vector3 initialPosition)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        player.transform.position = initialPosition;
    }

    /// <summary>
    /// Prevent the player from being able to move and turn.
    /// </summary>
    public void FreezePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;
        player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    }

    /// <summary>
    /// Allow the player to move.
    /// </summary>
    public void UnfreezePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;
        player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
    }

    /// <summary>
    /// Generates the fragment components and adds them to the level. 
    /// </summary>
    public void AddFragmentsToLevel()
    {
        GameObject fragmentParent = new GameObject("Fragments - ");
        fragmentParent.transform.parent = GameObject.FindGameObjectWithTag("Dungeon").transform;

        int fragmentID = 0;
        List<Fragment> fragments;
        Transform roomExitTransform;
        Quaternion rotation;

        foreach (var roomObj in GameObject.FindGameObjectsWithTag("RoomInstance"))
        {
            foreach (var roomExit in roomObj.GetComponent<Room>().GetRoomExits())
            {
                if (roomExit.FragmentSpawned) //if a fragment has already been spawned at this exit, skip it
                    continue;

                int index = -1;
                if (roomExit.GetConnectionStatus().Equals(RoomExit.ExitStatus.Connected))
                {
                    //only one fragment needs to be spawned between the 2 connected exits so set the FragmentSpawned property of both exits to true
                    roomExit.FragmentSpawned = true;
                    RoomExit connectedExit = FindRoomExit(roomExit.GetConnectedTo());
                    if (connectedExit != null)
                        connectedExit.FragmentSpawned = true;

                    fragments = GetFragmentsMatchingType(Fragment.FragmentTypes.Door);
                    index = rnd.GetUnityEngineRandom(0, fragments.Count);

                    if (roomExit.GetExit().HasPriority())
                        roomExitTransform = roomExit.GetExit().transform;
                    else
                        roomExitTransform = roomExit.GetConnectedTo().transform;

                    //rotate the fragment based on the angles of it's prefab and the Y axis of the exit
                    rotation = Quaternion.Euler(fragments[index].transform.eulerAngles.x,
                                                fragments[index].transform.eulerAngles.y + roomExitTransform.eulerAngles.y + fragments[index].GetRotateAdditionalDegrees(),
                                                fragments[index].transform.eulerAngles.z);

                    Fragment fr = UnityEngine.MonoBehaviour.Instantiate(fragments[index], roomExitTransform.position, rotation, fragmentParent.transform);
                    fr.name += fragmentID;
                    fr.transform.Translate(fragments[index].GetAdditonalMovement());
                }
                else //if ((roomExit.GetConnectionStatus().Equals(RoomExit.ExitStatus.ClosedDueToCollision) || roomExit.GetConnectionStatus().Equals(RoomExit.ExitStatus.Open)))
                {
                    roomExit.FragmentSpawned = true;
                    fragments = GetFragmentsMatchingType(Fragment.FragmentTypes.Wall);
                    index = rnd.GetUnityEngineRandom(0, fragments.Count);

                    roomExitTransform = roomExit.GetExit().transform;

                    rotation = Quaternion.Euler(fragments[index].transform.eulerAngles.x,
                                                fragments[index].transform.eulerAngles.y + roomExitTransform.eulerAngles.y + fragments[index].GetRotateAdditionalDegrees(),
                                                fragments[index].transform.eulerAngles.z);
                    Fragment fr = UnityEngine.MonoBehaviour.Instantiate(fragments[index], roomExitTransform.position, rotation, fragmentParent.transform);
                    fr.name += fragmentID;
                    fr.transform.Translate(fragments[index].GetAdditonalMovement());
                }
                fragmentID++;
            }
        }
        fragmentParent.name += fragmentParent.GetComponentsInChildren<Fragment>().Count();
    }
}
