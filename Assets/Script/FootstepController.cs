using UnityEngine;

public class FootstepController : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource footstepSource;

    [Header("Player Reference")]
    public CharacterController playerController;

    void Update()
    {
        // Failsafe just in case things aren't hooked up
        if (playerController == null || footstepSource == null) return;

        // Check if the player is physically moving across the floor
        bool isMoving = Mathf.Abs(playerController.velocity.x) > 0.1f || Mathf.Abs(playerController.velocity.z) > 0.1f;

        if (isMoving)
        {
            // If we are moving, and the sound ISN'T playing yet, start it!
            if (!footstepSource.isPlaying)
            {
                footstepSource.Play();
            }
        }
        else
        {
            // If we STOP moving, and the sound is still playing, kill it instantly!
            if (footstepSource.isPlaying)
            {
                footstepSource.Stop();
                // Note: You can change .Stop() to .Pause() if you want the sound to 
                // resume from the exact millisecond it left off when you walk again.
            }
        }
    }
}