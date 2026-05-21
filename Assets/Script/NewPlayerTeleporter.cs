using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class NewPlayerTeleporter : MonoBehaviour
{
    [Header("Scene To Load")]
    public string targetSceneName;

    [Header("Spawn Offset in next scene (tweak if player clips into wall)")]
    public Vector3 spawnOffset = new Vector3(0, 0, 1f);

    [Header("Cooldown Settings")]
    public float teleportCooldown = 1.5f;

    private static bool isOnCooldown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isOnCooldown) return;
        if (!other.CompareTag("Player")) return;

        StartCoroutine(TeleportToScene(other));
    }

    private IEnumerator TeleportToScene(Collider other)
    {
        isOnCooldown = true;

        // Save player's position and rotation into PlayerData
        if (PlayerData.Instance != null)
        {
            // Offset pushes them slightly forward so they don't re-trigger
            PlayerData.Instance.spawnPosition = other.transform.position
                                              + other.transform.TransformDirection(spawnOffset);
            PlayerData.Instance.spawnRotation = other.transform.rotation;
            PlayerData.Instance.hasCustomSpawn = true;
        }

        yield return null;

        // Load the next scene
        SceneManager.LoadScene(targetSceneName);
    }
}