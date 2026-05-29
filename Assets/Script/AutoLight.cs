using UnityEngine;

public class AutoLight : MonoBehaviour
{
    // The word "static" means every single light in the game shares this exact same rule
    public static bool isPowerOut = false;

    void Start()
    {
        // When your EndlessManager clones a new room, the light instantly checks the rule
        if (isPowerOut)
        {
            GetComponent<Light>().enabled = false; // Spawn in the dark!
        }
    }
}