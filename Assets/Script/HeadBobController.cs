using UnityEngine;

public class HeadBobController : MonoBehaviour
{
    [Header("Bob Settings")]
    public float walkBobSpeed = 10f;
    public float bobAmount = 0.05f;

    [Header("References")]
    public CharacterController playerController;

    private float defaultPosY = 0;
    private float timer = 0;

    void Start()
    {
        // Store the camera's starting height so we can always return to it
        defaultPosY = transform.localPosition.y;
    }

    void Update()
    {
        if (playerController == null) return;

        // Check if the player is actually moving (velocity > 0.1)
        if (Mathf.Abs(playerController.velocity.x) > 0.1f || Mathf.Abs(playerController.velocity.z) > 0.1f)
        {
            // The player is moving! Advance the sine wave timer.
            timer += Time.deltaTime * walkBobSpeed;

            // Apply the mathematical up-and-down motion to the camera
            transform.localPosition = new Vector3(
                transform.localPosition.x,
                defaultPosY + Mathf.Sin(timer) * bobAmount,
                transform.localPosition.z);
        }
        else
        {
            // The player stopped. Smoothly slide the camera back to the resting height.
            timer = 0;
            transform.localPosition = new Vector3(
                transform.localPosition.x,
                Mathf.Lerp(transform.localPosition.y, defaultPosY, Time.deltaTime * walkBobSpeed),
                transform.localPosition.z);
        }
    }
}