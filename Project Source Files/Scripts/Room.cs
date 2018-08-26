using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds information regarding the exits, collider/room type.
/// </summary>
[AddComponentMenu("Room-Based Generator Scripts/Room Scripts/Room Script")]
public class Room : MonoBehaviour
{
    /// <summary>
    /// Defines the types of rooms.
    /// </summary>
    public enum RoomTypes
    {
        Hall,
        Corridor,
        Junction
    };
    [SerializeField] private RoomTypes roomType; //the type of the room

    /// <summary>
    /// Contains all collider types so their type can easily be extracted and the specific collider generated.
    /// </summary>
    public enum ColliderTypes
    {
        Box,
        Mesh,  
        Capsule,
        Sphere
    };

    [Header("Algorithm Collision Settings")]
    [Tooltip("Is there a specific type of collider that would be best to use with the current model?")]
    [SerializeField] private ColliderTypes colliderType = ColliderTypes.Box;


    [Tooltip("The center of the collider.")]
    [SerializeField] private Vector3 center = Vector3.zero;

    [Tooltip("The height of the Box Collider (Only applicable to Box Coliders).")]
    [SerializeField] private Vector3 size = Vector3.zero;

    [Tooltip("The radius of the sphere collider (Only applicable to Sphere Coliders).")]
    [SerializeField] private float radius = 0.0f;

    [Header("Player Collision Settings")]
    [Tooltip("A mesh to be used as a collision mesh for the Mesh Collider.")]
    [SerializeField] private Mesh meshForMeshCollider = null;

    [Tooltip("A Game Object with colliders only to serve as a collision mesh.")]
    [SerializeField] private bool customCollision = false;

    private List<RoomExit> exitsList = new List<RoomExit>(); //initiallised here as Awake can not be counted on to quickly initialise the list causing nullRefEx --> THIS LIST CONTAINS EXITS FOR THE ROOM ITS ATTACHED TO, NOT ALL ROOMS

    void Awake()
    {
        //populate the exits list
        foreach (var connector in GetComponentsInChildren<RoomConnector>())
            exitsList.Add(new RoomExit(connector));
    }

    public RoomTypes GetRoomType() { return roomType; }
    public ColliderTypes GetColliderType() { return colliderType; }
    public Vector3 GetCenter() { return center; }
    public float GetRadius() { return radius; }
    public RoomExit[] GetRoomExits() { return exitsList.ToArray(); }

    /// <summary>
    /// Checks if there is an exit in the room that has been closed due to a collision.
    /// </summary>
    /// <returns>Returns TRUE if an exit closed due to collision is found. Otherwise, returns FALSE.</returns>
    public bool IsCollidedExitPresent()
    {
        bool collisionDetected = false;
        foreach (var roomExit in exitsList)
        {
            if (roomExit.GetConnectionStatus().Equals(RoomExit.ExitStatus.ClosedDueToCollision))
            {
                collisionDetected = true;
                break;
            }
        }
        return collisionDetected;
    }

    /// <summary>
    /// Finds and returns all of the RoomConnector components that were found in the objects' children.
    /// </summary>
    /// <returns>All RoomConnector child components.</returns>
	public RoomConnector[] GetExits()
	{
        return GetComponentsInChildren<RoomConnector>();
	}

    /// <summary>
    /// Finds all of the RoomConnector components in the objects' children,
    /// and returns only the ones that are still open.
    /// </summary>
    /// <returns>An array of all the open exits.</returns>
    public RoomConnector[] GetActiveExits()
    {
        List<RoomConnector> activeConnections = new List<RoomConnector>();
        foreach(var roomExit in exitsList)
        {
            if (roomExit.IsExitOpen())
                activeConnections.Add(roomExit.GetExit());
        }
        return activeConnections.ToArray();
    }

    /// <summary>
    /// Checks if the passed in exit is still open.
    /// </summary>
    /// <param name="exit">The exit to check whether is still open or not.</param>
    /// <returns>Returns TRUE if the exit is open. Otherwise, returns FALSE.</returns>
    public bool IsExitOpen(RoomConnector exit)
    {
        foreach(var roomExit in exitsList)
        {
            if (roomExit.GetExit().Equals(exit) && roomExit.IsExitOpen())
                return true;
        }
        return false;
    }

    /// <summary>
    /// Finds the specified RoomConnector in the exits list,
    /// and closes it by invoking the CloseExit function.
    /// </summary>
    /// <param name="connectingFrom">The Exit to close.</param>
    /// <param name="connectingTo">The Exit that the first exit is connecting to.</param>
    public void ConnectExits(RoomConnector connectingFrom, RoomConnector connectTo)
    {
        foreach (var roomExit in exitsList)
        {
            if (roomExit.GetExit().Equals(connectingFrom))
                roomExit.CloseExit(connectTo, RoomExit.ExitStatus.Connected);
        }
        connectingFrom.DestroyCollider(); //remove collider as it's no longer necessary
    }
    
    /// <summary>
    /// Close exit due to a collision.
    /// </summary>
    /// <param name="exit">Exit to be closed.</param>
    public void CloseCollidingExit(RoomConnector exit)
    {
        foreach (var roomExit in exitsList)
        {
            if (roomExit.GetExit().Equals(exit))
                roomExit.CloseExit(null, RoomExit.ExitStatus.ClosedDueToCollision);
        }
        //exit.DestroyCollider(); //remove collider as it's no longer necessary
    }

    /// <summary>
    /// Finds all of the exits in the exits list that are closed,
    /// and returns their number.
    /// </summary>
    /// <returns>The number of closed exits.</returns>
    public int GetNumOfClosedExits()
    {
        int closedExits = 0;
        foreach (var roomExit in exitsList)
        {
            if (!roomExit.IsExitOpen())
                closedExits++;
        }
        return closedExits;
    }

    /// <summary>
    /// Reads the collider type and creates a new Collider instance based on the type.
    /// </summary>
    /// <param name="collType"></param>
    /// <returns>A new collider instance based on the type.</returns>
    public Collider FromColliderTypeToColliderInstance(ColliderTypes collType)
    {
        switch(collType)
        {
            case ColliderTypes.Box:
                return new BoxCollider();
            case ColliderTypes.Capsule:
                return new CapsuleCollider();
            case ColliderTypes.Mesh:
                return new MeshCollider();
            case ColliderTypes.Sphere:
                return new SphereCollider();
            default:
            return null;
        }
    }

    /// <summary>
    /// Add box/sphere colliders to all the rooms.
    /// </summary>
    public void SetupCollider()
    {
        DestroyColliders();

        Room.ColliderTypes colliderType = this.GetComponent<Room>().GetColliderType();
        this.gameObject.AddComponent(this.GetComponent<Room>().FromColliderTypeToColliderInstance(colliderType).GetType());

        if (this.GetComponent<SphereCollider>() != null)
        {
            if (radius == 0)
                this.GetComponent<SphereCollider>().radius -= 0.6f;
            else
                this.GetComponent<SphereCollider>().radius = radius;
            if (center != Vector3.zero)
                this.GetComponent<SphereCollider>().center = center;
        }
        else if (this.GetComponent<BoxCollider>() != null && size != Vector3.zero)
        {
            Vector3 colliderSize = this.GetComponent<BoxCollider>().size;
            this.GetComponent<BoxCollider>().size = size;
        }
        else if (this.GetComponent<CapsuleCollider>() != null)
        {
            if (radius != 0)
                this.GetComponent<CapsuleCollider>().radius = radius;
            if (center != Vector3.zero)
                this.GetComponent<CapsuleCollider>().center = center;
        }
    }

    /// <summary>
    ///  Destroy the mesh collider on all of the rooms.
    /// </summary>
    /// <param name="roomList"></param>
    public void DestroyColliders()
    {
        foreach (var collider in this.GetComponents<Collider>())
            Destroy(collider);
    }

    /// <summary>
    /// Checks around each of the rooms active exits for any collisions with other exits.
    /// If it finds any, it closes those exits as they are already connected.
    /// </summary>
    public void ConnectActiveExitsInProximity()
    {
        CollisionDetection collision = new CollisionDetection();
        Collider[] hitColliders;
        foreach(var exit in GetExits())
        {
            hitColliders = collision.DetectCollision(exit, exit.GetRadius());
            if(hitColliders.Length==2)
            {
                //check if second exit is open before connecting them
                //if (!hitColliders[1].gameObject.GetComponentInParent<Room>().IsExitOpen(hitColliders[1].gameObject.GetComponent<RoomConnector>()))
                 //   return;

                //connect the 2 exits
                hitColliders[0].gameObject.GetComponentInParent<Room>().ConnectExits(hitColliders[0].gameObject.GetComponent<RoomConnector>(), hitColliders[1].gameObject.GetComponent<RoomConnector>());
                hitColliders[1].gameObject.GetComponentInParent<Room>().ConnectExits(hitColliders[1].gameObject.GetComponent<RoomConnector>(), hitColliders[0].gameObject.GetComponent<RoomConnector>());
            }
        }
    }

    /// <summary>
    /// Destroys the colliders of every exit and also closes the exits in the room.
    /// </summary>
    public void CloseAllExits()
    {
        foreach(var exit in exitsList)
        {
            exit.GetExit().DestroyCollider();
            exit.CloseExit();
        }
    }

    /// <summary>
    /// If a mesh reference is provided, it creates a mesh collider using the provided mesh reference.
    /// If the customCollision is checked, it goes through the Room's components which have the ManualCollisionMesh script attached,
    /// and calls its EnableCollider function.
    /// </summary>
    public void GenerateCollision()
    {
        if (meshForMeshCollider != null)
        {
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.convex = true;
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = meshForMeshCollider;
        }

        else if (customCollision)
        {
            foreach (var obj in GetComponentsInChildren<ManualCollisionMesh>())
                obj.EnableCollider();
        }

        //Get rid of unnecessary components in the scene.
        if (!customCollision)
            foreach (var obj in GetComponentsInChildren<ManualCollisionMesh>())
                Destroy(obj.gameObject);
    }
}