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

            transform.position = PlayerData.Instance.spawnPosition;
            transform.rotation = PlayerData.Instance.spawnRotation;
            PlayerData.Instance.hasCustomSpawn = false;

            if (cc != null) cc.enabled = true;
            if (fps != null) fps.ResetMovement();
        }
    }
}