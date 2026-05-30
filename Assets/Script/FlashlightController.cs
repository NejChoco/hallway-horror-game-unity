using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public Light flashlightBeam;
    public AudioSource clickSound;
    public bool startsTurnedOn = true;

    private void Start()
    {
        // Turn the light on or off based on your setting
        flashlightBeam.enabled = startsTurnedOn;

        // If the game starts with the light ON, play the click sound instantly!
        if (startsTurnedOn && clickSound != null && clickSound.clip != null)
        {
            clickSound.PlayOneShot(clickSound.clip);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Toggle the light
            flashlightBeam.enabled = !flashlightBeam.enabled;

            // Play the click sound when you press 'F'
            if (clickSound != null && clickSound.clip != null)
            {
                clickSound.PlayOneShot(clickSound.clip);
            }
        }
    }

    // THIS IS THE METHOD YOUR POWER SCRIPT WAS LOOKING FOR!
    public void ForceLightOn()
    {
        // Only turn it on if it is currently off
        if (!flashlightBeam.enabled)
        {
            flashlightBeam.enabled = true;

            if (clickSound != null && clickSound.clip != null)
            {
                clickSound.PlayOneShot(clickSound.clip);
            }
        }
    }
}