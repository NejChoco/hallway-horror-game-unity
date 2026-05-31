using UnityEngine;

public class MenuController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject mainMenuUI;

    [Header("Player Reference")]
    public FPSController playerScript; // Explicitly asking for your FPSController now!

    private void Start()
    {
        OpenMenu();
    }

    public void OpenMenu()
    {
        mainMenuUI.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Safely freeze the player's movement and looking using your built-in variable!
        if (playerScript != null) playerScript.canMove = false;
    }

    public void StartGame()
    {
        // 1. Unfreeze time FIRST
        Time.timeScale = 1f;

        // 2. Give the player back control FIRST
        if (playerScript != null)
        {
            playerScript.canMove = true;
        }

        // 3. Lock and hide the mouse cursor FIRST
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 4. NOW disable the UI LAST! (Safe to unplug its brain now)
        mainMenuUI.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}