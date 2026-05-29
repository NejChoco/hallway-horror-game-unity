using UnityEngine;
using TMPro; // Assuming you are using TextMeshPro!

public class RoomSign : MonoBehaviour
{
    private TMP_Text[] allSigns;

    void Awake()
    {
        // This automatically finds all 4 text objects inside your "Room numbers" folder!
        allSigns = GetComponentsInChildren<TMP_Text>();
    }

    public void SetLevelSign(int currentLevel)
    {
        if (allSigns == null || allSigns.Length == 0) return;

        // "D3" formats the number to always have 3 digits (001, 002, 010, etc.)
        string levelString = currentLevel.ToString("D3");

        // Loop through all 4 signs and update them simultaneously
        foreach (TMP_Text sign in allSigns)
        {
            sign.text = levelString;
        }
    }
}