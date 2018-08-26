using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class is responsible for the random sequences generation.
/// Utilizes both Unity and C# libraries to generate the same sequences every time, or a different sequence every time.
/// </summary>
public class RandomItem
{
    private System.Random random;

    /// <summary>
    /// Use to set the seed for the UnityEngine.Random class
    /// </summary>
    /// <param name="seed"></param>
    public RandomItem(int seed)
    {
        UnityEngine.Random.InitState((seed * 5 * 642) % 79 + 7);
    }

    /// <summary>
    /// Use when the random sequence needs to be different every time - using System.Random.
    /// </summary>
    public RandomItem()
    {
        random = new System.Random();
    }

    /// <summary>
    /// Updates the seed of UnityEngine.Random.
    /// </summary>
    /// <param name="seed">The seed to be used.</param>
    public void UpdateSeed(int seed)
    {
        UnityEngine.Random.InitState((seed * 5 * 642) % 79 + 7);
    }

    /// <summary>
    ///  Uses the UnityEngine.Random class to return a value between the min [inclusive] and max [exclusive].
    /// </summary>
    /// <param name="min">Minimum value.</param>
    /// <param name="max">Maximum value.</param>
    /// <returns>A value between min and max.</returns>
    public int GetUnityEngineRandom(int min, int max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    /// <summary>
    ///  Uses the System.Random class to return a value between the min [inclusive] and max [exclusive].
    /// </summary>
    /// <param name="min">Minimum value.</param>
    /// <param name="max">Maximum value.</param>
    /// <returns>A value between min and max.</returns>
    public int GetSystemRandom(int min, int max)
    {
        return random.Next(min, max);
    }

    /// <summary>
    /// Get a random item from the passed in array.
    /// </summary>
    /// <typeparam name="TItem">Any type.</typeparam>
    /// <param name="array">Array from which to get a random item.</param>
    /// <param name="additionalSeed">Optional parameter which is added to the random seed generator.</param>
    /// <returns>Random item from array.</returns>
    public TItem GetRandom<TItem>(TItem[] array)
    {
        if (array.Length == 0)
        {
            Debug.LogError("GetRandom argument has length 0! Returning default.");
            return default(TItem); // returns null for reference type/0 for int/ '/0' for char, etc.
        }
        return array[UnityEngine.Random.Range(0, array.Length)];
    }

    /// <summary>
    /// Retrieves a random element from the passed in list of rooms based on the specified type.
    /// </summary>
    /// <param name="rooms">Rooms to choose from.</param>
    /// <param name="roomTypeToMatch">The room type that needs to match the selected room.</param>
    /// <param name="additionalSeed">Optional parameter which is added to the random seed generator.</param>
    /// <returns>Returns a random room from the rooms list matching the room type.</returns>
    public Room GetRandomByRoomType(IEnumerable<Room> rooms, Room.RoomTypes roomTypeToMatch)
    {
        var matchingRooms = rooms.Where(m => m.GetRoomType() == roomTypeToMatch).ToArray();
        return GetRandom(matchingRooms);
    }
}