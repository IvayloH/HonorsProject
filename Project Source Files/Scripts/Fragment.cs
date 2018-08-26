using UnityEngine;

/// <summary>
/// Class that defines the components that will be used as a filler for the gaps in the rooms
/// These Fragments are meant to be doors/doorways/walls.
/// </summary>
[AddComponentMenu("Room-Based Generator Scripts/Room Scripts/Fragment")]
public class Fragment : MonoBehaviour
{
    public enum FragmentTypes
    {
        Wall,
        Door
    };
      
    [Tooltip("The type of the fragment. Will this fragment be used to close a room off or will it be used as a connection between 2 rooms?")]
    [SerializeField] private FragmentTypes fragmentType;

    [Header("Fragment Collision Settings")]
    [Tooltip("Would you like to use a specific mesh for the mesh collider? Otherwise a box collider will be used.")]
    [SerializeField] private Mesh meshForMeshCollider = null;
    [Tooltip("This should be ticked only if there hasn't been a collision mesh referenced above.")]
    [SerializeField] private bool customCollision = false;
    
    [Space(10)]
    [Header("Additional Rotation Settings")]
    [SerializeField] private float additionalRotationOnYAxis = 0.0f;
    [Space(10)]
    [Header("Additional Movement Settings")]
    [SerializeField] private float onXAxis = 0.0f;
    [SerializeField] private float onYAxis = 0.0f;
    [SerializeField] private float onZAxis = 0.0f;

    public Vector3 GetAdditonalMovement() { return new Vector3(onXAxis, onYAxis, onZAxis); }
    public float GetRotateAdditionalDegrees() { return additionalRotationOnYAxis; }

    /// <summary>
    /// Gets the fragment type.
    /// </summary>
    /// <returns>The type of the fragment.</returns>
    public FragmentTypes GetTypeOfFragment()
    {
        return fragmentType;
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
