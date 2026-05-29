using System.Collections;
using UnityEngine;

public class PowerOutage : MonoBehaviour
{
    public Light playerFlashlight;
    public float timeBeforeBlackout = 10f; // 10 seconds of safe, lit hallway

    void Start()
    {
        // 1. Ensure the game starts with the power ON
        AutoLight.isPowerOut = false;
        if (playerFlashlight != null) playerFlashlight.enabled = false;

        StartCoroutine(BlackoutSequence());
    }

    IEnumerator BlackoutSequence()
    {
        // Wait for the player to get comfortable
        yield return new WaitForSeconds(timeBeforeBlackout);

        // 2. The Power Dies! Tell all future generated rooms to spawn in the dark.
        AutoLight.isPowerOut = true;

        // 3. Find every cloned light currently turned on in Level 0 and shut them off instantly
        AutoLight[] allActiveLights = FindObjectsOfType<AutoLight>();
        foreach (AutoLight light in allActiveLights)
        {
            light.GetComponent<Light>().enabled = false;
        }

        // Wait 2 terrifying seconds in pitch black
        yield return new WaitForSeconds(2f);

        // 4. Turn on the flashlight
        if (playerFlashlight != null) playerFlashlight.enabled = true;
    }
}