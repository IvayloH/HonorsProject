using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// The generator algorithm class. Instantaites and connects rooms, based on several factors, to form an underground level.
/// </summary>
[AddComponentMenu("Room-Based Generator Scripts/Generator Script")]
[RequireComponent(typeof(ConnectionRules))]
public class RoomBasedGenerator : MonoBehaviour
{
    [Header("Rooms Setup")]
    [Tooltip("The room prefabs to be used in generating the level.")]
    [SerializeField] private Room[] Rooms;

    [Space(5)]
    [Tooltip("Initial room to start building level from.")]
    [SerializeField] private Room InitialRoom;

    [Tooltip("The position of the initial room. ")]
    [SerializeField] private Vector3 InitialRoomPosition = new Vector3(0, 0, 0);

    [Tooltip("Final room to attach to the built level. Should be the level's exit room.")]
    [SerializeField] private Room ExitRoom;

    [Space(5)]
    [Header("Closing off Paths")]
    [SerializeField]
    [Tooltip("Rooms to pick from when closing off corridors with so they player does not think the corridor simply lead to nowhere.")]
    private Room[] RoomsToUseToClosePath;
    [Tooltip("Which room types to be closed off?")]
    [SerializeField] private Room.RoomTypes[] RoomTypesToCloseOff;
    [Tooltip("Any fragments the system will be using.")]
    [SerializeField] private Fragment[] wallFragments;

    [Space(5)]
    [Header("Algorithm Parameters")]
    [Tooltip("How many rooms to have for the path from the start to exit room.")]
    [Range(1, 250)]
    [SerializeField] private int numberOfRooms = 6;

    [Tooltip("How many rooms to connect along the main path from start to exit?" +
        "Meaninng there will be a specified number of rooms added to the rest of the rooms." +
        "They will be connected to rooms chosen at random.")]
    [Range(0, 250)]
    [SerializeField] private int expandPathBy = 5;

    [Tooltip("Seed to be used in the generation of the level. (Using the same seed will produce the same level)")]
    [Range(0, 9999)]
    [SerializeField] private int seed = 1;
    [SerializeField] private bool allowLoops = true;
    [SerializeField] private bool randomizeNumOfRooms = false;
    [SerializeField] private bool randomizeExpansion = false;
    [SerializeField] private bool randomizeSeed = false;
    

    [Space(5)]
    [Header("Debugging")]
    [Tooltip("Outputs information regarding the status of the generation of each level to a text file.")]
    [SerializeField] private bool debugLog = false;
    [Tooltip("Colours the main path in a green.")]
    [SerializeField] private bool highlightMainPath = false;
    [Tooltip("Forces the system to keep the room colliders that are dynamically created during the generation. Reduces CPU performance.")]
    [SerializeField] private bool keepRoomColliders = false;
    [Tooltip("Forces the system to keep the collided rooms. Reduces CPU performance.")]
    [SerializeField] private bool keepCollidedRooms = false;
    [Tooltip("Forces the system to keep the gizmos for the exits. Reduces CPU performance.")]
    [SerializeField] private bool keepExitGizmos = false;
    [Tooltip("Forces the system to keep the gizmos for the spawnable objects. Reduces CPU performance.")]
    [SerializeField] private bool keepObjectGizmos = false;
    [Tooltip("Colours the exit room in a different color.")]
    [SerializeField] private bool markEndRoom = false;
    [Tooltip("Whether the system an user interface.")]
    [SerializeField] private bool useCanvas = false;
    [Tooltip("If option above is ticked, provide a reference to the Canvas game object.")]
    [SerializeField] private Canvas canvasToUse = null;

    private GameObject emptyGameObject; //Empty GameObject to use a parent for all of the dungeon elements. (Helps keep a clean hierarchy and makes it easier to restart level)
    private GameObject spParent;
    private GameObject expParent;
    private int secondsToWaitForReset = 1; //seconds to wait in between dungeon level resets
    private Room currRoom; //keep a reference to the room we are currently expanding
    private RoomConnector currExit;
    private int MAX_UNSUCCESSFUL_ROOM_REPLACEMENT_ATTEMPTS; //max attempts of replacing a room when there is a collision detected
    private int CURRENT_UNSUCCESSFUL_ROOM_REPLACMENT_ATTEMPTS;
    private bool hasExitRoomBeenSuccessfullyAdded; //flag for exit room's success
    private int roomsMarkedForDestruction; //keep track of the rooms that need to be destroyed -- for DEBUG purposes only
    private RandomItem rnd;
    private CollisionDetection collision;
    private int uniqueID = 0; //add ID to each rooms name to easily differentiate them
    private GeneratorSupprtFunctions supportFunctions;
    private GameObject closeOffParent; //empty GO for the close off rooms
    CanvasEvents canvasEvents;

    public bool _DEBUG_MODE { get; set; }
    public FileLogger FileLogger { get; set; }
    public bool InitialisingLevel { get; set; } //flag to prevent canvas events from calling functions during the level generation
    public int MaxNumberOfRooms { get; set; }
    public int MaxNumberOfExpansionRooms { get; set; }

    public int GetNumberOfRooms() { return numberOfRooms; }
    public void SetNumberOfRooms(int num) { numberOfRooms = num; }

    public int GetNumberOfExpansionRooms() { return expandPathBy; }
    public void SetNumberOfExpansionRooms(int num) { expandPathBy = num; }

    public int GetSeed() { return seed; }
    public void SetSeed(int seed) { this.seed = seed; }

    public void SetAllowLoops(bool allowLoops) { this.allowLoops = allowLoops; }
    public bool GetAllowLoops() { return allowLoops; } 

    void Awake()
    {
        _DEBUG_MODE = debugLog;
        if (_DEBUG_MODE)
            FileLogger = new FileLogger();
        if(Rooms.Length==0)
        {
            if (_DEBUG_MODE)
                FileLogger.LogError(">!< No rooms to use(array is empty)! >!<");
            Debug.LogError("Please add rooms first.");
        }
        if(InitialRoom==null)
        {
            if (_DEBUG_MODE)
                FileLogger.LogError(">!< No initial room has been set! >!<");
            Debug.LogError("Please specify an initial room first.");
        }
        MaxNumberOfRooms = 250;
        MaxNumberOfExpansionRooms = 250;
        InitialisingLevel = true;
        hasExitRoomBeenSuccessfullyAdded = false;
        MAX_UNSUCCESSFUL_ROOM_REPLACEMENT_ATTEMPTS = Rooms.Length / 3;
        CURRENT_UNSUCCESSFUL_ROOM_REPLACMENT_ATTEMPTS = 0;
        if (randomizeSeed || randomizeNumOfRooms || randomizeExpansion)
        {
            RandomItem sysRnd = new RandomItem();
            if (randomizeSeed)
            {
                int newSeed = seed;
                while (seed == newSeed)
                    newSeed = sysRnd.GetSystemRandom(1, 9999);
                seed = newSeed;
            }
            if(randomizeNumOfRooms)
                numberOfRooms = sysRnd.GetSystemRandom(1, MaxNumberOfRooms);
            if (randomizeExpansion)
                expandPathBy = sysRnd.GetSystemRandom(1, MaxNumberOfRooms); // don't exceed the main path rooms
        }

        if (useCanvas && canvasToUse != null)
        {
            if (canvasToUse.gameObject.activeSelf)
                canvasEvents = canvasToUse.GetComponent<CanvasEvents>();
            else
            {
                if (_DEBUG_MODE)
                    FileLogger.Log("Canvas object is disabled. Turning off the use of the canvas.");
                useCanvas = false;
            }
        }
    }

    void Start()
    {
        rnd = new RandomItem(seed);
        collision = new CollisionDetection();
        supportFunctions = new GeneratorSupprtFunctions(rnd, Rooms, wallFragments);
        StartCoroutine(Reset(secondsToWaitForReset));
    }

    void Update()
    {
        // Destroy room objects that have collided but are still present in the level.
        if (!keepCollidedRooms && GameObject.FindGameObjectsWithTag("Collision").Length != 0)
        {
            int objectsDestroyed = GameObject.FindGameObjectsWithTag("Collision").Length;
            foreach (var collision in GameObject.FindGameObjectsWithTag("Collision"))
                Destroy(collision);
        }

        //randomize parameters and reset level
        if (useCanvas && Input.GetKeyDown(KeyCode.R) && canvasEvents != null)
            canvasEvents.RandomizeParameters();

        //hide the canvas UI
        if (useCanvas && Input.GetKeyDown(KeyCode.H) && canvasEvents != null)
            canvasEvents.HideUIPanel();
    }

    public void ResetLevel()
    {
        if (emptyGameObject != null)
        {
            Destroy(emptyGameObject);
            StartCoroutine(Reset(secondsToWaitForReset));
        }
    }

    /// <summary>
    /// Call RunAlgorithm() here because: Actual object destruction is always delayed until after the current Update loop, but will always be done before rendering.
    /// A.k.a. wait for the 'emptyGameObject' object and its children to be completely destroyed before creating them anew.
    /// </summary>
    /// <returns>Execution of the RunAlgorithm() after specified amount of seconds.</returns>
    IEnumerator Reset(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        RunAlgorithm();
    }

    /// <summary>
    /// Performs all of the necessary function calls for the generation of the dungeon.
    /// </summary>
    private void RunAlgorithm()
    {
        if (_DEBUG_MODE)
        {
            FileLogger.Log("============== STARTING ALGORITHM ==============");
            FileLogger.Log("== Params-> MP: " + numberOfRooms + ", Exp: " + expandPathBy + ", Seed: " + seed + ", Loop: " + allowLoops + " ==");
        }
        try
        {

            emptyGameObject = new GameObject("Level Tree");
            emptyGameObject.transform.position = Vector3.zero;
            emptyGameObject.tag = "Dungeon";
            roomsMarkedForDestruction = 0;
            rnd.UpdateSeed(seed);
            InitialisingLevel = true;
            supportFunctions.FreezePlayer();
            SetupInitialRoom();
            GenerateMainPath();
            SetupExitRoom();
            Expand();
            new WaitForEndOfFrame();
            //In case the level is too small, the exit room would not have been added, so check once more and add it
            if(!hasExitRoomBeenSuccessfullyAdded)
                SetupExitRoom();

            CloseOffPaths();
            if(wallFragments.Count()!=0)
                supportFunctions.AddFragmentsToLevel(); //Fill in the gaps with provided models(fragments)

            if(GetComponent<DungeonObjectSpawner>()!=null)
                GetComponent<DungeonObjectSpawner>().SpawnDungeonObjects(); //spawn objects

            LevelCleanUp(); //delete colliders & no longer necessary components
            new WaitForEndOfFrame();
            SetupPlayerRoomCollision();

            new WaitForEndOfFrame();
            supportFunctions.ResetPlayerPosition(InitialRoomPosition + Vector3.up * 3);
            supportFunctions.UnfreezePlayer();

            InitialisingLevel = false;
            
            if(useCanvas && canvasEvents != null)
                canvasEvents.EnableInput();
        }
        catch(System.Exception e)
        {
            if(!_DEBUG_MODE)
                FileLogger = new FileLogger();
            FileLogger.LogError("FATAL ERROR: " + e.Message + "\r\n" + e.StackTrace);
            Debug.LogError("FATAL ERROR: " + e.Message + "\r\n" + e.StackTrace);
            Application.Quit();
        }

        if (_DEBUG_MODE)
            FileLogger.Log("============== ALGORITHM FINISHED ==============\r\n");
    }

    /// <summary>
    /// Setup an empty game object to hold all of the instantiated rooms.
    /// Setup the initial room and set currRoom to point to it.
    /// </summary>
    private void SetupInitialRoom()
    {
        var initialRoom = (Room)Instantiate(InitialRoom, InitialRoomPosition, InitialRoom.transform.rotation); //first room
        initialRoom.SetupCollider();
        initialRoom.transform.SetParent(emptyGameObject.transform);
        initialRoom.GetComponent<Renderer>().material.color = Color.white;
        initialRoom.name = "InitialRoom";
        initialRoom.tag = "RoomInstance";
        initialRoom.gameObject.layer = LayerMask.NameToLayer("RoomInstances");
        currRoom = initialRoom;

        if(_DEBUG_MODE)
            FileLogger.Log("Initial room setup was successful.");
    }

    /// <summary>
    /// Generate the main path of the level.
    /// </summary>
    private void GenerateMainPath()
    {
        spParent = new GameObject("Main Path - ");
        spParent.transform.parent = emptyGameObject.transform;

        if (_DEBUG_MODE)
            FileLogger.Log(">Starting Main Path Function...");

        bool success = true;
        for (int iteration = 0; iteration < numberOfRooms; iteration++)
        {
            int returnValue = GenerateRoom();
            switch(returnValue)
            {
                case -1:
                    success = false;
                    break;
                case 0:
                    iteration--;
                    break;
                default:
                    break;
            }
            if (!success) //break out of loop
                break;
        }
        if(_DEBUG_MODE)
        {
            if (success)
            {
                FileLogger.Log("<Main Path generated successfully.");
            }
            else
            {
                FileLogger.LogError(">!< Main Path Generation FAILED! >!<");
                FileLogger.LogError(">!< Ran out of active exits during Main Path Gen. >!<");
            }
        }
        spParent.name += spParent.GetComponentsInChildren<Room>().Count() + " rooms";
    }

    /// <summary>
    /// Pick a random room with at least one open exit from the generated rooms
    /// and add a new room to it, thus further expanding the level.
    /// This is done 'ExpandPathBy' times.
    /// </summary>
    private void Expand()
    {
        expParent = new GameObject("EXP - ");
        expParent.transform.parent = emptyGameObject.transform;

        uniqueID = 0;
        if (_DEBUG_MODE)
            FileLogger.Log(">Starting Expansion of level with "+supportFunctions.GetTotalNumberOfActiveExits() +" exits...");

        bool success = true;
        for (int iteration = 0; iteration < expandPathBy; iteration++)
        {
            int returnValue = GenerateRoom(false);
            switch (returnValue)
            {
                case -1:
                    success = false;
                    break;
                case 0:
                    iteration--;
                    break;
                default:
                    break;
            }
            if (!success) //break out of loop
                break;
        }

        if (_DEBUG_MODE)
        {
            if (success)
                FileLogger.Log("<Level Expansion was successful.");
            else
            {
                FileLogger.LogError(">!< Expansion Generation FAILED! >!<");
                FileLogger.LogError(">!< Ran out of active exits during expansion. >!<");
            }
        }
        expParent.name += expParent.GetComponentsInChildren<Room>().Count() + " rooms";
    }

    /// <summary>
    /// Instantiates a room and adds it to the level. Based on the passed in argument it will either
    /// add a room to the main path of the level(true) or to a random room(false).
    /// Returns 1 if function was successful. 0 if collision occured. -1 if there are no more active exits.
    /// </summary>
    /// <param name="onMainPath">True: generate the main path. False: expand the level</param>
    /// <returns>-1 if dead end has been found; 0 if a collision has occured; 1 if room was generated successfully.</returns>
    private int GenerateRoom(bool onMainPath=true)
    {
        if(supportFunctions.GetTotalNumberOfActiveExits()==0)
            return -1;

        //try and add the exit room if the previous attempt was unsuccessful
        if (!onMainPath && supportFunctions.GetAllRoomsWithActiveExits().Count > 3 && !hasExitRoomBeenSuccessfullyAdded && CURRENT_UNSUCCESSFUL_ROOM_REPLACMENT_ATTEMPTS==0)
        {//dont generate during main path generation, but also make sure the function is not trying to add another room at this time                                                                           
            if (_DEBUG_MODE)
                FileLogger.Log(">!< Trying SetupExitRoom during Expansion >!<");
            SetupExitRoom();
        }

        //if expanding level( NOT onMainPath) and not re-trying due to collision ( curr unsuccessful attempts is 0)
        if(!onMainPath && CURRENT_UNSUCCESSFUL_ROOM_REPLACMENT_ATTEMPTS==0)
            currRoom = rnd.GetRandom(supportFunctions.GetAllRoomsWithActiveExits().ToArray()); //pick a random open exit from the list to use

        //if we're not trying to replace a room due to collision, get a new random exit
        if(CURRENT_UNSUCCESSFUL_ROOM_REPLACMENT_ATTEMPTS==0)
            currExit = rnd.GetRandom(currRoom.GetActiveExits()); //get the exit's room component

        var possibleRoomConnections = GetComponent<ConnectionRules>().GetPossibleConnectionBetweenRoomsByRoomType(currExit.transform.parent.GetComponent<Room>().GetRoomType());
        var pickRandomRoomType = rnd.GetRandom(possibleRoomConnections); //pick randomly a room type which can be connected to the current one's exit
        var newRoomPrefab = rnd.GetRandomByRoomType(Rooms, pickRandomRoomType); //find item with specific room type

        if (onMainPath)
        {
            //avoid dead end if the new room will have only 1 exit which will be closed upon conneciton with the current room, or if there are few exits left
            if (newRoomPrefab.GetExits().Length <= 1 || supportFunctions.GetTotalNumberOfActiveExits()<=3)
                supportFunctions.AvoidDeadEnd(ref newRoomPrefab, possibleRoomConnections);
        }
        else
        {
            if (supportFunctions.GetTotalNumberOfActiveExits() <= 5)
                supportFunctions.AvoidDeadEnd(ref newRoomPrefab, possibleRoomConnections);
        }

        var newRoom = (Room)Instantiate(newRoomPrefab);

        if(onMainPath)
            newRoom.name += "SP"+ uniqueID;
        else
            newRoom.name += "EXP" + uniqueID;


        newRoom.SetupCollider();
        newRoom.tag = "RoomInstance";
        newRoom.gameObject.layer = LayerMask.NameToLayer("RoomInstances");

        var newRoomExits = newRoom.GetExits(); //get exits for the new room
        var exitToMatch = rnd.GetRandom(newRoomExits); //select default exit or choose one at random

        currExit.MatchExits(exitToMatch); //rotate the new room so it connects correctly

        if (collision.DetectCollision(newRoom)) //deal with the collision
        {
            MarkRoomForDestruction(newRoom);
            CURRENT_UNSUCCESSFUL_ROOM_REPLACMENT_ATTEMPTS++;

            if (CURRENT_UNSUCCESSFUL_ROOM_REPLACMENT_ATTEMPTS >= MAX_UNSUCCESSFUL_ROOM_REPLACEMENT_ATTEMPTS) //close off exit and backtrack
            {
                //close off exit only if there are other exits still open - otherwise it would lead to a complete dead end
                CURRENT_UNSUCCESSFUL_ROOM_REPLACMENT_ATTEMPTS = 0;
                currRoom.CloseCollidingExit(currExit);

                //find a new room with active exits.
                if(onMainPath)
                    currRoom = supportFunctions.BacktrackToLastRoomWithActiveExits();

                //else case will be executed next time the function is called as currRoom will be randomly picked if not onMainPath
            }
            
            return 0;
        }
        //reset attempts and connect the rooms
        CURRENT_UNSUCCESSFUL_ROOM_REPLACMENT_ATTEMPTS = 0;
        currRoom.ConnectExits(currExit, exitToMatch);
        newRoom.ConnectExits(exitToMatch, currExit);

        if (allowLoops)
        {
            newRoom.ConnectActiveExitsInProximity();
        }

        if (onMainPath)
        {
            newRoom.transform.SetParent(spParent.transform);
            if (highlightMainPath)
            {
                // "highlight" the main path
                currRoom.GetComponent<Renderer>().material.color = Color.green;
                newRoom.GetComponent<Renderer>().material.color = Color.green;
            }

            //if the new room doesn't have an exit, don't move into it
            if (newRoom.GetActiveExits().Length >= 1)
                currRoom = newRoom;

            if(currRoom.GetActiveExits().Length==0)
            {
                currRoom = supportFunctions.BacktrackToLastRoomWithActiveExits();
                CURRENT_UNSUCCESSFUL_ROOM_REPLACMENT_ATTEMPTS = 0;
            }
        }
        else
            newRoom.transform.SetParent(expParent.transform);
        uniqueID++;

        return 1;
    }

    /// <summary>
    /// Try and add the specified 'ExitRoom' to one of the open exits.
    /// </summary>
    private void SetupExitRoom()
    {
        if (ExitRoom == null) //no exit room prefab was provided
        {
            hasExitRoomBeenSuccessfullyAdded = true;
            if (_DEBUG_MODE)
                FileLogger.Log(">No Exit Room Specified. Terminating SetupExitRoom function.<");
            return;
        }

        if (_DEBUG_MODE)
            FileLogger.Log(">Starting SetupExitRoom...");

        if((supportFunctions.GetAllRoomsWithActiveExits().Count < 3 || supportFunctions.GetRoomsWithPossibleExits().Count<5) && expandPathBy>1)
        {
            //Make sure there are enough exits before spawning an exit room as it would certainly be a dead end
            if (_DEBUG_MODE)
                FileLogger.Log("Postponing SetupExitRoom due to possibility of creating a dead-end.");
            hasExitRoomBeenSuccessfullyAdded = false;
            return;
        }
        
        //Select a room to connect the exit to.
        int excludeRooms = 0; //counter to keep track of how many rooms we've backtracked so we do not check the same ones again
        currRoom = supportFunctions.BacktrackToLastRoomWithActiveExits(GetComponent<ConnectionRules>().GetPossibleConnectionBetweenRoomsByRoomType(ExitRoom.GetRoomType()),
                                              ref excludeRooms); ; //find a room to add the exit to(needs to meet one of the connection rules criteria)

        if (currRoom.Equals(InitialRoom))
        {
            if (_DEBUG_MODE)
                FileLogger.Log(">Attempted to connect exit room to initial room. Returning and awaiting more available rooms.<");

            hasExitRoomBeenSuccessfullyAdded = true;
            return;
        }

        if (currRoom == null)
        {
            //force stop the execution of the function if a suitable room wasn't found
            if (_DEBUG_MODE)
                FileLogger.Log(">!< Could not add exit room to any of the rooms. Terminating SetupExitRoom function! >!<");

            hasExitRoomBeenSuccessfullyAdded = false;
            return;
        }

        Room exitRoom = (Room)Instantiate(ExitRoom);
        exitRoom.SetupCollider();
        exitRoom.tag = "RoomInstance";
        exitRoom.gameObject.layer = LayerMask.NameToLayer("RoomInstances");
        var exitRoomExits = exitRoom.GetExits(); //get exits for the new room
        RoomConnector exitToMatch = null;
        bool collisionDetected = false;

        do
        {
            if (currRoom == null || currRoom.Equals(InitialRoom))
            {
                //force stop the execution of the function if a suitable room wasn't found
                if (_DEBUG_MODE)
                    FileLogger.Log(">!< Could not add exit room to any of the rooms. Terminating SetupExitRoom function! >!<");

                MarkRoomForDestruction(exitRoom);
                hasExitRoomBeenSuccessfullyAdded = false;
                return;
            }

            currExit = rnd.GetRandom(currRoom.GetActiveExits());

            if (collisionDetected)
            {
                currRoom = supportFunctions.BacktrackToLastRoomWithActiveExits(GetComponent<ConnectionRules>().GetPossibleConnectionBetweenRoomsByRoomType(exitRoom.GetRoomType()), 
                                                              ref excludeRooms); //find a room to add the exit to
                excludeRooms++; //if this room cannot be connected to the one that was just found, then ignore it next time as we don't close the exit / prevent inf loop
                continue;
            }   

            exitToMatch = rnd.GetRandom(exitRoomExits); //select default exit or choose one at random
            currExit.MatchExits(exitToMatch); //rotate the new room so it connects correctly
        }
        while (collisionDetected = collision.DetectCollision(exitRoom)); //deal with the collision

        currRoom.ConnectExits(currExit, exitToMatch);
        exitRoom.ConnectExits(exitToMatch, currExit);

        exitRoom.transform.parent = emptyGameObject.transform;
        exitRoom.name = "ExitRoom";
        hasExitRoomBeenSuccessfullyAdded = true;
        if (markEndRoom)
            exitRoom.GetComponent<Renderer>().material.color = Color.red;

        if (_DEBUG_MODE)
            FileLogger.Log("<SetupExitRoom was successful.");
    }

    /// <summary>
    /// Go through all of the generated rooms that match the parameter and add a small room to them so they don't lead to nowhere.
    /// </summary>
    /// <param name="typeToCloseOff">The room type to look for.</param>
    private void CloseOffPaths()
    {
        if (closeOffParent == null)
        {
            closeOffParent = new GameObject("Path closing rooms - ");
            closeOffParent.transform.parent = emptyGameObject.transform;
        }

        foreach (var roomType in RoomTypesToCloseOff)
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("RoomInstance"))
            {
                currRoom = go.GetComponent<Room>();
                if (currRoom.GetRoomType().Equals(roomType))
                {
                    foreach (var exit in currRoom.GetRoomExits())
                    {
                        if (exit.GetConnectionStatus().Equals(RoomExit.ExitStatus.Connected)) //if exit is connected, skip it
                            continue;

                        Room newRoom = (Room)Instantiate(rnd.GetRandom(RoomsToUseToClosePath));
                        newRoom.SetupCollider();
                        currExit = exit.GetExit();
                        var newRoomExits = newRoom.GetExits();
                        var exitToMatch = rnd.GetRandom(newRoomExits);
                        newRoom.tag = "RoomInstance";
                        newRoom.gameObject.layer = LayerMask.NameToLayer("RoomInstances");

                        currExit.MatchExits(exitToMatch);

                        if (collision.DetectCollision(newRoom))
                        {
                            MarkRoomForDestruction(newRoom);
                            currRoom.CloseCollidingExit(currExit); // cannot add room here so close the exit
                            continue;
                        }
                        currRoom.ConnectExits(currExit, exitToMatch);
                        newRoom.ConnectExits(exitToMatch, currExit);

                        newRoom.transform.SetParent(closeOffParent.transform);
                    }
                }
            }
        }
        closeOffParent.name += closeOffParent.GetComponentsInChildren<Room>().Count();
    }

    /// <summary>
    /// Add the room object to he 'Collisions' layer so it can be destroyed. Also close its exits.
    /// </summary>
    /// <param name="roomToDestroy"></param>
    private void MarkRoomForDestruction(Room roomToDestroy)
    {
        roomToDestroy.tag = "Collision";
        roomToDestroy.gameObject.layer = LayerMask.NameToLayer("Collisions");
        roomToDestroy.CloseAllExits();
        roomsMarkedForDestruction++;
    }

    /// <summary>
    /// Calls the 'GenerateCollision()' functions on each of the fragment and room objects in the level.
    /// </summary>
    private void SetupPlayerRoomCollision()
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag("Fragment"))
            obj.GetComponent<Fragment>().GenerateCollision();
        foreach (var obj in GameObject.FindGameObjectsWithTag("RoomInstance"))
            obj.GetComponent<Room>().GenerateCollision();
    }

    /// <summary>
    ///  Destroy the collider on all of the rooms. Also destroy all room connection game objects.
    /// </summary>
    /// <param name="roomList"></param>
    private void LevelCleanUp()
    {
        if (!keepRoomColliders)
        {
            foreach (var room in GameObject.FindGameObjectsWithTag("RoomInstance"))
                room.GetComponent<Room>().DestroyColliders();
        }

        if (!keepExitGizmos)
        {
            foreach (var obj in GameObject.FindGameObjectsWithTag("Exit"))
                Destroy(obj);
        }

        if (!keepObjectGizmos)
        {
            foreach (var obj in GameObject.FindGameObjectsWithTag("ObjectPlacement"))
                Destroy(obj);
        }
    }
}