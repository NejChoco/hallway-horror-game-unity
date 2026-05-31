using UnityEngine;

public class ProximityAudio : MonoBehaviour
{
    public AudioSource scareSound;

    [Header("The Actual 3D Model")]
    public GameObject visualModel; // We will plug the 3D model into this!

    private bool hasPlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        // 1. If the visual 3D model is turned OFF (hidden), do absolutely nothing.
        if (visualModel != null && !visualModel.activeInHierarchy)
        {
            return;
        }

        // 2. If the model IS on, and the player hits the wire, play the scare!
        if (other.CompareTag("Player") && !hasPlayed)
        {
            scareSound.Play();
            hasPlayed = true;
        }
    }
}