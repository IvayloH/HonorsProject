using UnityEngine;

/// <summary>
/// Class defines a container GameObject. Keeping track of whether it is open or not, using a bool flag.
/// Uses a reference of a ContainerCover GameObject to rotate the cover if there is one.
/// (Class functionallity would need to be extended as it currently only offers simplistic functionallity)
/// </summary>
public class Container : MonoBehaviour
{
    private bool opened = false;
    private ContainerCover cover = null;

    void Awake()
    {
        cover = GetComponentInChildren<ContainerCover>();
    }

    void OnMouseDown()
    {
        if (cover == null)
            return;

        if (!opened)
        {
            cover.transform.Rotate(cover.RotateBy());
            opened = true;
        }
        else
        {
            cover.transform.Rotate(-cover.RotateBy());
            opened = false;
        }
    }
}
