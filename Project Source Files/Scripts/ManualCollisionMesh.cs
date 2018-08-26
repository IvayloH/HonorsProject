using UnityEngine;

public class ManualCollisionMesh : MonoBehaviour
{
    /// <summary>
    /// Disable the colliders that are used for player collision.
    /// </summary>
    void Awake()
    {
        foreach (Collider objectCollider in gameObject.GetComponents<Collider>())
        {
            objectCollider.enabled = false;
        }
    }

    /// <summary>
    /// Enables the collider on the GameObject, provided there is one.
    /// </summary>
    public void EnableCollider()
    {
        foreach (Collider objectCollider in gameObject.GetComponents<Collider>())
        {
            if (objectCollider != null)
            {
                objectCollider.enabled = true;
                objectCollider.tag = "PlayerCollider";
                objectCollider.gameObject.layer = LayerMask.NameToLayer("CameraCollision");
            }
        }
    }
}