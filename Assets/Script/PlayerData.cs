using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;

    public Vector3 spawnPosition;
    public Quaternion spawnRotation;
    public bool hasCustomSpawn = false;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}