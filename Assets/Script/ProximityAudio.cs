using UnityEngine;

public class ProximityAudio : MonoBehaviour
{
    public AudioSource scareSound;
    private bool hasPlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        // Make sure your Player object has the tag "Player"!
        if (other.CompareTag("Player") && !hasPlayed)
        {
            scareSound.Play();
            hasPlayed = true; // Prevents the sound from spamming if they walk back and forth
        }
    }
}