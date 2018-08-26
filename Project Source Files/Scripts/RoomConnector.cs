using UnityEngine;

/// <summary>
/// The room connections, or room exits. Used to determine the number and location of exits in a room.
/// </summary>
[AddComponentMenu("Room-Based Generator Scripts/Room Scripts/Room Connector Script")]
public class RoomConnector : MonoBehaviour
{
    [Header("Exit Settings")]
    [Tooltip("If set to true, when the algorithm is spawning a fragment between this and another exit, the fragment will be spawned over this exit.")]
    [SerializeField] private bool hasPriority = false;

    [Header("Collider Settings")]
    [Tooltip("Radius of the sphere collider. Used for connecting rooms that are close to each other." +
        "The radius should not be too big. It should be around the same value the floor height is.")]
    [SerializeField] private float radius = 0.01f;

    [Space(10)]
    [Header("Gizmos Settings")]
    [SerializeField] private float GizmosScale = 1.0f;

    //Represent the exits on each of the rooms so they can easily be adjusted in the editor.
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
		Gizmos.DrawSphere(transform.position, 0.125f);
	}

    /// <summary>
    /// Sets up the objects tags and adds a collider.
    /// </summary>
    void Awake()
    {
        gameObject.tag = "Exit";
        gameObject.layer = LayerMask.NameToLayer("RoomConnections");
        SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = radius;
    }

    /// <summary>
    /// Get the priority of the RoomConnector object. 
    /// </summary>
    /// <returns></returns>
    public bool HasPriority()
    {
        return hasPriority;
    }

    /// <summary>
    /// Gets the radius that was set by the user in the Editor.
    /// </summary>
    /// <returns>The radius.</returns>
    public float GetRadius()
    {
        return radius;
    }

    /// <summary>
    /// Destroys the collider component attached to the game object. (The same object the script is attached to)
    /// </summary>
    public void DestroyCollider()
    {
        gameObject.tag = "Collision";
        gameObject.layer = LayerMask.NameToLayer("Collisions");
        if(gameObject.GetComponent<SphereCollider>() != null)
            Destroy(this.GetComponent<SphereCollider>());
    }

    /// <summary>
    /// Rotate the new room so it faces and matches the current rooms exit.
    /// </summary>
    /// <param name="currExit">Current rooms exit.</param>
    /// <param name="newExit">New rooms exit.</param>
    public void MatchExits(RoomConnector newExit)
    {
        var newRoom = newExit.transform.parent;
        var forwardVectorToMatch = -this.transform.forward;
        var correctiveRotation = CalculateAzimuth(forwardVectorToMatch) - CalculateAzimuth(newExit.transform.forward);
        newRoom.RotateAround(newExit.transform.position, Vector3.up, correctiveRotation);
        var correctiveTranslation = this.transform.position - newExit.transform.position;
        newRoom.transform.position += correctiveTranslation;
    }


    /// <summary>
    /// The azimuth is the angle formed between a reference direction 
    /// and a line from the observer to a point of interest projected on the same plane as 
    /// the reference direction orthogonal to the zenith.
    /// </summary>
    /// <param name="vector">Vector used to calculate angle.</param>
    /// <returns>The Azimuth.</returns>
    private static float CalculateAzimuth(Vector3 vector)
    {
        return Vector3.Angle(Vector3.forward, vector) * Mathf.Sign(vector.x);
    }
}
