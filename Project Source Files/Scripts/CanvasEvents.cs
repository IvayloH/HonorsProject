using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Holds all of the function that are called from certain canvas events.
/// </summary>
[AddComponentMenu("Room-Based Generator Scripts/Canvas Scripts/Canvas Events Script")]
public class CanvasEvents : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private InputField numOfRoomsInputField;
    [SerializeField] private InputField expansionRoomsNumberInputField;
    [SerializeField] private InputField seedInputField;
    [SerializeField] private Toggle allowLoops;

    private GameObject dungeonGenerator;
    private RoomBasedGenerator worldGeneratorScript;
    private bool noDungeonGenerator = false;

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
            else
                noDungeonGenerator = true;
        }
        else
            noDungeonGenerator = true;

        if (!noDungeonGenerator)
        {
            numOfRoomsInputField.text = worldGeneratorScript.GetNumberOfRooms().ToString();
            expansionRoomsNumberInputField.text = worldGeneratorScript.GetNumberOfExpansionRooms().ToString();
            seedInputField.text = worldGeneratorScript.GetSeed().ToString();
            allowLoops.isOn = worldGeneratorScript.GetAllowLoops();
            allowLoops.enabled = true;
        }
        else
        {
            numOfRoomsInputField.text = "0";
            expansionRoomsNumberInputField.text = "0";
            seedInputField.text = "0";
        }
        DisableInput();
    }

    /// <summary>
    /// Invokes the UpdateNumberOfRooms function from the Dungeon Generator's WorldGenerator Script.
    /// This updates the value of the number of rooms the algorithm generates.
    /// After which InvokeResetDungeon is called.
    /// </summary>
    public void NumOfRoomsChanged()
    {
        if (numOfRoomsInputField.text.Length == 0 || noDungeonGenerator || worldGeneratorScript.InitialisingLevel)
            return;
        int num = -1;
        int.TryParse(numOfRoomsInputField.text, out num);
        if(num>0 && num<=worldGeneratorScript.MaxNumberOfRooms)
            worldGeneratorScript.SetNumberOfRooms(num);
    }

    /// <summary>
    /// Invokes the UpdateAlgorithmIterations function from the Dungeon Generator's WorldGenerator Script.
    /// This updates the value of the number of iterations the algorithm goes through.
    /// After which InvokeResetDungeon is called.
    /// </summary>
    public void ExpansionRoomsNumberChanged()
    {
        if (expansionRoomsNumberInputField.text.Length == 0 || noDungeonGenerator || worldGeneratorScript.InitialisingLevel)
            return;
        int num = -1;
        int.TryParse(expansionRoomsNumberInputField.text, out num);
        if (num > 0 && num <= worldGeneratorScript.MaxNumberOfExpansionRooms)
            worldGeneratorScript.SetNumberOfExpansionRooms(num);
    }

    /// <summary>
    /// Invokes the SetSeed function from the Dungeon Generator's WorldGenerator Script.
    /// This updates the value of the seed the algorithm uses.
    /// After which InvokeResetDungeon is called.
    /// </summary>
    public void SeedChanged()
    {
        if (seedInputField.text.Length == 0 || noDungeonGenerator || worldGeneratorScript.InitialisingLevel)
            return;
        int num = -1;
        int.TryParse(seedInputField.text, out num);
        if (num > 0)
            worldGeneratorScript.SetSeed(num);
    }

    /// <summary>
    /// Invokes the SetAllowLoops function from the Dungeon Generator's WorldGenerator Script.
    /// After which InvokeResetDungeon is called.
    /// </summary>
    public void LoopsAllowedChanged()
    {
        if (noDungeonGenerator || worldGeneratorScript.InitialisingLevel)
            return;
        worldGeneratorScript.SetAllowLoops(allowLoops.isOn);
    }

    /// <summary>
    /// Calls the ResetLevel function from the WorldGenerator Script so the level is generated again.
    /// </summary>
    public void InvokeResetDungeon()
    {
        if (noDungeonGenerator || worldGeneratorScript.InitialisingLevel)
            return;
        DisableInput();
        worldGeneratorScript.ResetLevel();
    }

    /// <summary>
    /// Forces the application to exit.
    /// </summary>
    public void QuitApp()
    {
        Application.Quit();
    }

    /// <summary>
    /// Dsiable InputField/Toggle Scripts.
    /// </summary>
    private void DisableInput()
    {
        foreach(var button in gameObject.GetComponentsInChildren<Button>())
        {
            button.interactable = false;
        }
        foreach (var inputField in gameObject.GetComponentsInChildren<InputField>())
        {
            inputField.interactable = false;
        }
        allowLoops.GetComponent<Toggle>().interactable = false;
    }

    /// <summary>
    /// Enable InputField/Toggle Scripts.
    /// This is used as using the InitialisingLevel flag will not always return on time.
    /// </summary>
    public void EnableInput()
    {
        foreach (var button in gameObject.GetComponentsInChildren<Button>())
        {
            button.interactable = true;
        }
        foreach (var inputField in gameObject.GetComponentsInChildren<InputField>())
        {
            inputField.interactable = true;
        }
        allowLoops.GetComponent<Toggle>().interactable = true;
    }

    /// <summary>
    /// Sets a random value for the parameters in the script, after which it updates the parameters in the UI.
    /// Level reset function is called after the parameters are updated.
    /// </summary>
    public void RandomizeParameters()
    {
        DisableInput();
        RandomItem random = new RandomItem();
        worldGeneratorScript.SetNumberOfRooms(random.GetSystemRandom(5, 250));
        worldGeneratorScript.SetNumberOfExpansionRooms(random.GetSystemRandom(5, worldGeneratorScript.GetNumberOfRooms()));

        int newSeed = random.GetSystemRandom(1, 9999);
        while (worldGeneratorScript.GetSeed() == newSeed)
            newSeed = random.GetSystemRandom(1, 9999);
        worldGeneratorScript.SetSeed(newSeed);
        UpdateParameters();
        worldGeneratorScript.ResetLevel();
    }

    /// <summary>
    /// Takes the values from the script for the parameters and sets the text of the input fields to those values.
    /// </summary>
    private void UpdateParameters()
    {
        numOfRoomsInputField.text = worldGeneratorScript.GetNumberOfRooms().ToString();
        expansionRoomsNumberInputField.text = worldGeneratorScript.GetNumberOfExpansionRooms().ToString();
        seedInputField.text = worldGeneratorScript.GetSeed().ToString();
        allowLoops.isOn = worldGeneratorScript.GetAllowLoops();
        allowLoops.enabled = true;
    }

    /// <summary>
    /// Hides the panel.
    /// </summary>
    public void HideUIPanel()
    {
        panel.SetActive(!panel.activeSelf);
    }
}