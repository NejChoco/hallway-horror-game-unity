using UnityEngine;

public class AutoScatter : MonoBehaviour
{
    public GameObject anomalyPrefab;
    public int amountToSpawn = 30;
    public Vector2 spreadArea = new Vector2(8.5f, 35f);

    [Header("Rotation Settings")]
    [Tooltip("Floor = (0,0,0). Ceiling = (180,0,0). Walls = (90,0,0) or (-90,0,0)")]
    public Vector3 baseRotation = new Vector3(0, 0, 0);
    public bool spinRandomly = true; // True for messy piles, False for neat rows

    [ContextMenu("Spawn Anomalies Now")]
    void SpawnObjects()
    {
        for (int i = 0; i < amountToSpawn; i++)
        {
            // Pick random position
            float randomX = Random.Range(-spreadArea.x / 2, spreadArea.x / 2);
            float randomZ = Random.Range(-spreadArea.y / 2, spreadArea.y / 2);
            Vector3 spawnPos = transform.position + new Vector3(randomX, 0, randomZ);

            // Calculate rotation based on your Inspector settings
            float spin = spinRandomly ? Random.Range(0f, 360f) : 0f;
            Quaternion finalRot = Quaternion.Euler(baseRotation.x, baseRotation.y + spin, baseRotation.z);

            // Spawn the object
            GameObject newObj = Instantiate(anomalyPrefab, spawnPos, finalRot, transform);

            // Randomize size
            float randomScale = Random.Range(0.8f, 1.2f);
            newObj.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
        }
    }
}