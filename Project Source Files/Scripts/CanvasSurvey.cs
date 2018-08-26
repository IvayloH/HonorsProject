using UnityEngine;

/// <summary>
/// This script is a basic version to 'CanvasEvents'. It is more restrictive as it's only purpose is to reset the level with specific parameters and quit the application.
/// This script will be deleted after the survey is done.
/// </summary>
public class CanvasSurvey : MonoBehaviour
{
    private GameObject dungeonGenerator;
    private RoomBasedGenerator worldGeneratorScript;

    /// <summary>
    /// Initialise the worldGeneratorScript object.
    /// Get the number of rooms and algorithm iterations integeres from the script and set the values for the input fields.
    /// </summary>
    void Awake()
    {
        dungeonGenerator = GameObject.FindGameObjectWithTag("DungeonGenerator");
        if (dungeonGenerator != null)
        {
            if (dungeonGenerator.GetComponent<RoomBasedGenerator>() != null && dungeonGenerator.GetComponent<RoomBasedGenerator>().enabled)
                worldGeneratorScript = dungeonGenerator.GetComponent<RoomBasedGenerator>();
        }
    }

    /// <summary>
    /// Sets a random value for the parameters in the script, after which it updates the parameters in the UI.
    /// Level reset function is called after the parameters are updated.
    /// </summary>
    public void RandomizeParameters()
    {
        RandomItem random = new RandomItem();
        worldGeneratorScript.SetNumberOfExpansionRooms(random.GetSystemRandom(5, 15));
        worldGeneratorScript.SetNumberOfRooms(random.GetSystemRandom(5, 15));
        int newSeed = random.GetSystemRandom(1, 9999);
        while (worldGeneratorScript.GetSeed() == newSeed)
            newSeed = random.GetSystemRandom(1, 9999);
        worldGeneratorScript.SetSeed(newSeed);
        worldGeneratorScript.ResetLevel();
    }

    public void QuitApp()
    {
        Application.Quit();
    }
}
