using System.Collections;
using UnityEngine;

public class PowerOutage : MonoBehaviour
{
    // 1. We changed this from "Light" to "FlashlightController" and made it private!
    private FlashlightController playerFlashlight;

    public float timeBeforeBlackout = 10f; // 10 seconds of safe, lit hallway

    void Start()
    {
        // 2. Automatically find the flashlight script in the scene!
        playerFlashlight = FindObjectOfType<FlashlightController>();

        // Ensure the game starts with the power ON
        AutoLight.isPowerOut = false;

        // Note: We removed the code that forces the light off here, 
        // because your FlashlightController handles its own starting state perfectly now!

        StartCoroutine(BlackoutSequence());
    }

    IEnumerator BlackoutSequence()
    {
        // Wait for the player to get comfortable
        yield return new WaitForSeconds(timeBeforeBlackout);

        // The Power Dies! Tell all future generated rooms to spawn in the dark.
        AutoLight.isPowerOut = true;

        // Find every cloned light currently turned on in Level 0 and shut them off instantly
        AutoLight[] allActiveLights = FindObjectsOfType<AutoLight>();
        foreach (AutoLight light in allActiveLights)
        {
            light.GetComponent<Light>().enabled = false;
        }

        // Wait 2 terrifying seconds in pitch black
        yield return new WaitForSeconds(2f);

        // 3. Turn on the flashlight USING OUR NEW SOUND-ENABLED METHOD!
        if (playerFlashlight != null)
        {
            playerFlashlight.ForceLightOn();
        }
    }
}