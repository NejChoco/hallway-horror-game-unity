using UnityEngine;
using System.Collections;

public class NewPlayerTeleporter : MonoBehaviour
{
    public Transform TeleportZoneObject;

    [Header("Cooldown Settings")]
    public float teleportCooldown = 5f;

    // static = shared across ALL teleporters in the scene
    // So if Portal A fires, Portal B is also blocked for 5 seconds
    private static bool isOnCooldown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isOnCooldown) return;

        if (other.CompareTag("Player"))
        {
            if (TeleportZoneObject == null)
            {
                Debug.LogWarning("TeleportZoneObject is not assigned!");
                return;
            }

            StartCoroutine(TeleportRoutine(other));
        }
    }

    private IEnumerator TeleportRoutine(Collider other)
    {
        isOnCooldown = true;

        FPSController fps = other.GetComponent<FPSController>();
        CharacterController cc = other.GetComponent<CharacterController>();

        // Step 1: Disable CharacterController
        if (cc != null) cc.enabled = false;

        // Step 2: Reset movement BEFORE moving position
        if (fps != null) fps.ResetMovement();

        // Step 3: Teleport position and rotation
        // Small Y offset (+0.1f) prevents floor collision snapping
        Vector3 destination = TeleportZoneObject.position + Vector3.up * 0.1f;
        other.transform.position = destination;
        other.transform.rotation = TeleportZoneObject.rotation;

        // Step 4: Wait one frame so Unity registers the new position cleanly
        yield return null;

        // Step 5: Re-enable CharacterController AFTER frame update
        if (cc != null) cc.enabled = true;

        // Step 6: Reset camera after re-enable
        if (fps != null) fps.ResetCameraRotation();

        // Step 7: Wait for cooldown
        yield return new WaitForSeconds(teleportCooldown);
        isOnCooldown = false;
    }
}