using UnityEngine;

public class PlayerSpawnHandler : MonoBehaviour
{
    void Start()
    {
        if (PlayerData.Instance != null && PlayerData.Instance.hasCustomSpawn)
        {
            CharacterController cc = GetComponent<CharacterController>();
            FPSController fps = GetComponent<FPSController>();

            if (cc != null) cc.enabled = false;

            // Just slam the player to the EXACT same position and rotation
            transform.position = PlayerData.Instance.spawnPosition;
            transform.rotation = PlayerData.Instance.spawnRotation;

            PlayerData.Instance.hasCustomSpawn = false;

            if (cc != null) cc.enabled = true;
            if (fps != null) fps.ResetMovement();
        }
    }
}