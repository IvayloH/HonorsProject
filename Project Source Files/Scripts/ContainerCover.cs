using UnityEngine;

/// <summary>
/// This class is used in combination with the Container class when the container model is made of multiple objects.
/// </summary>
public class ContainerCover : MonoBehaviour
{
    [SerializeField] private float rotateCoverByOnX = -25.0f;
    [SerializeField] private float rotateCoverByOnY = 0.0f;
    [SerializeField] private float rotateCoverByOnZ = 0.0f;

    public Vector3 RotateBy() { return new Vector3(rotateCoverByOnX, rotateCoverByOnY, rotateCoverByOnZ); }
}
