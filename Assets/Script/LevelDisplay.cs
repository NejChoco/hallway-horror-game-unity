using UnityEngine;
using TMPro;

public class LevelDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI levelText;

    void Update()
    {
        // Constantly update the text by reading the GameManager's current level directly!
        if (GameManager.instance != null && levelText != null)
        {
            levelText.text = "Level " + GameManager.instance.currentLevel.ToString();
        }
    }
}