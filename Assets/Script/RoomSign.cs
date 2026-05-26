using UnityEngine;

public class RoomSign : MonoBehaviour
{
    // Drag your 001, 002, 101, etc., objects into this list in the Inspector
    public GameObject[] signModels;

    public void SetLevelSign(int level)
    {
        // Safety check to ensure we don't go out of bounds
        if (level < 0 || level >= signModels.Length) return;

        // Turn off all signs, turn on only the one that matches the current level
        for (int i = 0; i < signModels.Length; i++)
        {
            if (signModels[i] != null)
            {
                signModels[i].SetActive(i == level);
            }
        }
    }
}