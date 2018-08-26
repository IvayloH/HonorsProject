using UnityEngine;

public class CollisionDetection
{
    private FileLogger fileLogger;

    /// <summary>
    /// Retrieve the file logger instance from the dungeon generator script or initialize it.
    /// The same instance is used to prevent the log file being overwriten.
    /// </summary>
    public CollisionDetection()
    {
        if (GameObject.FindGameObjectWithTag("DungeonGenerator") != null)
            fileLogger = GameObject.FindGameObjectWithTag("DungeonGenerator").GetComponent<RoomBasedGenerator>().FileLogger;
        else
            fileLogger = new FileLogger(true);

        if(fileLogger == null)
            fileLogger = new FileLogger(true);
    }

    /// <summary>
    /// Checks if the object that was passed as argument is colliding with any other collider in the level.
    /// </summary>
    /// <param name="roomObj">The object which will be used as a reference point in world space for the collision check.</param>
    /// <returns>True if a collision is detected.</returns>
    public bool DetectCollision(Room roomObj)
    {
        Collider[] hitColliders = null;
        Vector3 objScale = roomObj.transform.localScale; // if the scale is different from 1,1,1 the Physics will create a much smaller/larger collision box
        if (roomObj.GetComponent<BoxCollider>() != null)
        {
            Vector3 roomColliderSize = roomObj.GetComponent<BoxCollider>().size;
            Vector3 minBounds = roomObj.GetComponent<BoxCollider>().bounds.min;
            Vector3 maxBounds = roomObj.GetComponent<BoxCollider>().bounds.max;
            Vector3 overlapBox = new Vector3((minBounds.x + maxBounds.x) / 2.0f,
                                             (minBounds.y + maxBounds.y) / 2.0f,
                                             (minBounds.z + maxBounds.z) / 2.0f);
            Vector3 overlapBoxExtents = new Vector3((roomColliderSize.x / 2.0f) * objScale.x, (roomColliderSize.y / 2.0f) * objScale.y, (roomColliderSize.z / 2.0f) * objScale.z);

            hitColliders = Physics.OverlapBox(overlapBox,
                                              overlapBoxExtents,
                                              roomObj.transform.rotation,
                                              LayerMask.GetMask("RoomInstances"));
        }

        else if (roomObj.GetComponent<SphereCollider>() != null)
        {
            hitColliders = Physics.OverlapSphere(roomObj.transform.position + roomObj.GetComponent<SphereCollider>().center,
                                                 roomObj.GetComponent<SphereCollider>().radius * objScale.x,
                                                 LayerMask.GetMask("RoomInstances"));
        }

        else if (roomObj.GetComponent<CapsuleCollider>() != null)
        {
            hitColliders = Physics.OverlapCapsule(roomObj.transform.position + roomObj.GetComponent<CapsuleCollider>().bounds.min,
                                                  roomObj.GetComponent<CapsuleCollider>().bounds.max,
                                                  roomObj.GetComponent<CapsuleCollider>().radius,
                                                  LayerMask.GetMask("RoomInstances"));
        }

        else if (roomObj.GetComponent<MeshCollider>() != null)
        {
            BoxCollider boxCollider = roomObj.gameObject.AddComponent<BoxCollider>();
            Vector3 minBounds = roomObj.GetComponent<MeshCollider>().bounds.min;
            Vector3 maxBounds = roomObj.GetComponent<MeshCollider>().bounds.max;
            Vector3 overlapBoxCenter = new Vector3((minBounds.x + maxBounds.x) / 2.0f,
                                                    (minBounds.y + maxBounds.y) / 2.0f,
                                                    (minBounds.z + maxBounds.z) / 2.0f);
            hitColliders = Physics.OverlapBox(overlapBoxCenter,
                                              new Vector3((boxCollider.size.x / 2.0f) * objScale.x, (boxCollider.size.y / 2.0f)*objScale.y, (boxCollider.size.z / 2.0f)*objScale.z),
                                              roomObj.transform.rotation,
                                              LayerMask.GetMask("RoomInstances"));
            Object.Destroy(boxCollider);
            return hitColliders.Length - 2 > 0; // Mesh collider and box collider, so '-2'
        }
        if (hitColliders == null)
        {
            fileLogger.LogError(">!> No Collider was found on object " + roomObj.name + " . Collision detection impossible.");
            Debug.LogError(">!> No Collider was found on object " + roomObj.name + " . Collision detection impossible.");
            Application.Quit();
        }
        return hitColliders.Length - 1 > 0; //-1 as the box/sphere collider belonging to the object is also hit
    }

    /// <summary>
    /// Invoke the Physics.OverlapSphere function and check for hit colliders in the RoomConnections layer.
    /// </summary>
    /// <param name="exit">The RoomConnector to be used as a starting point for the OverlapSphere.</param>
    /// <returns>An array of the colliders hit by the cast.</returns>
    public Collider[] DetectCollision(RoomConnector exit, float radius)
    {
        return Physics.OverlapSphere(exit.gameObject.GetComponent<SphereCollider>().transform.position,
                                                        radius,
                                                        LayerMask.GetMask("RoomConnections"));
    }
}
