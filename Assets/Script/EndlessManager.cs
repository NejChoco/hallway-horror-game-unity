using System.Collections.Generic;
using UnityEngine;

public class EndlessManager : MonoBehaviour
{
    [Header("Debug Options")]
    public bool forceAnomaliesForTesting = false; // Check this box in Unity to test!

    [Header("Prefabs")]
    public Segment normalHallway;
    public Segment backwardHallway;
    public Segment[] corners;

    public int currentPlayerIndex = 0;
    public List<Segment> activeSegments = new List<Segment>();

    private Transform playerTransform;
    private Segment currentStandingSegment = null;

    // THE NEW TRACKER: Keeps count of how many boring rooms you've seen
    private int normalStreak = 0;

    // THE UPGRADED MEMORY: Now uses strict ID numbers instead of 3D coordinates!
    private Dictionary<int, bool> roomMemory = new Dictionary<int, bool>();
    private int frontGlobalIndex = 0;
    private int backGlobalIndex = 0;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        StartGame();
    }

    public void StartGame()
    {
        roomMemory.Clear();
        frontGlobalIndex = 0;
        backGlobalIndex = 0;

        // Manually spawn the very first room as Room #0
        Segment firstSeg = Instantiate(normalHallway, Vector3.zero, Quaternion.identity);
        firstSeg.globalIndex = 0;
        SetupSegment(firstSeg);
        activeSegments.Add(firstSeg);

        for (int i = 0; i < 4; i++) SpawnForward();
        PlacePlayerAtStart();
    }

    void SpawnForward()
    {
        bool lastWasCorner = activeSegments[activeSegments.Count - 1].gameObject.name.Contains("Corner");
        Segment prefab = lastWasCorner ? normalHallway : corners[Random.Range(0, corners.Length)];

        Segment newSeg = Instantiate(prefab);
        Transform lastOut = activeSegments[activeSegments.Count - 1].socketOut;

        newSeg.transform.rotation = lastOut.rotation * Quaternion.Inverse(newSeg.socketIn.localRotation);
        newSeg.transform.position = lastOut.position - (newSeg.socketIn.position - newSeg.transform.position);

        // Stamp this room with the next positive ID (1, 2, 3...)
        frontGlobalIndex++;
        newSeg.globalIndex = frontGlobalIndex;

        SetupSegment(newSeg);
        activeSegments.Add(newSeg);
    }

    void SpawnBackward()
    {
        bool firstWasCorner = activeSegments[0].gameObject.name.Contains("Corner");
        Segment prefab = firstWasCorner ? backwardHallway : corners[Random.Range(0, corners.Length)];

        Segment newSeg = Instantiate(prefab);
        Transform firstIn = activeSegments[0].socketIn;

        newSeg.transform.rotation = firstIn.rotation * Quaternion.Inverse(newSeg.socketOut.localRotation);
        newSeg.transform.position = firstIn.position - (newSeg.socketOut.position - newSeg.transform.position);

        // Stamp this room with the next negative ID (-1, -2, -3...)
        backGlobalIndex--;
        newSeg.globalIndex = backGlobalIndex;

        SetupSegment(newSeg);
        activeSegments.Insert(0, newSeg);
    }

    void SetupSegment(Segment newSeg)
    {
        bool isCorner = newSeg.gameObject.name.Contains("Corner");
        bool isAnomaly = false;

        newSeg.gameObject.name = newSeg.gameObject.name.Replace("(Clone)", "") + "_Lvl_" + GameManager.instance.currentLevel;

        // MEMORY CHECK: Ask the bank if it has seen this exact ID number before!
        // MEMORY CHECK: Ask the bank if it has seen this exact ID number before!
        // MEMORY CHECK: Ask the bank if it has seen this exact ID number before!
        if (roomMemory.ContainsKey(newSeg.globalIndex))
        {
            isAnomaly = roomMemory[newSeg.globalIndex];
        }
        else
        {
            if (!isCorner && GameManager.instance.currentLevel > 0)
            {
                // THE DEBUG FIX: If the box is checked, force an anomaly!
                if (forceAnomaliesForTesting)
                {
                    isAnomaly = true;
                }
                // THE PITY TIMER: If you just walked through 2 normal rooms, force an anomaly!
                else if (normalStreak >= 2)
                {
                    isAnomaly = true;
                }
                else
                {
                    // Otherwise, flip a heavy 45% coin
                    isAnomaly = Random.value < 0.45f;
                }

                // Keep track of the streak!
                if (isAnomaly) normalStreak = 0;
                else normalStreak++;
            }

            // Save the decision to the ID number
            roomMemory.Add(newSeg.globalIndex, isAnomaly);
        }

        newSeg.isAnomaly = isAnomaly;

        AnomalyController ctrl = newSeg.GetComponent<AnomalyController>();
        if (ctrl != null) ctrl.Setup(isAnomaly, newSeg.gameObject.name);

        RoomSign sign = newSeg.GetComponentInChildren<RoomSign>();
        if (sign != null && !isCorner) sign.SetLevelSign(GameManager.instance.currentLevel);
    }

    void PlacePlayerAtStart()
    {
        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        playerTransform.position = activeSegments[2].socketIn.position + Vector3.up * 1.5f;
        playerTransform.rotation = activeSegments[2].socketIn.rotation;
        if (cc != null) cc.enabled = true;
    }

    void Update()
    {
        if (playerTransform == null) return;

        activeSegments.RemoveAll(item => item == null);
        if (activeSegments.Count == 0) return;

        int playerIndex = 0;
        float closestDist = Mathf.Infinity;

        for (int i = 0; i < activeSegments.Count; i++)
        {
            if (activeSegments[i] == null) continue;
            float dist = Vector3.Distance(playerTransform.position, activeSegments[i].transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                playerIndex = i;
            }
        }

        currentPlayerIndex = playerIndex;
        Segment actualRoom = activeSegments[currentPlayerIndex];

        if (actualRoom != currentStandingSegment)
        {
            currentStandingSegment = actualRoom;

            if (actualRoom != null && !actualRoom.gameObject.name.Contains("Corner"))
            {
                string anomalyText = actualRoom.isAnomaly ? "<color=red><b>YES</b></color>" : "<color=white>NONE</color>";
                Debug.Log($"<color=yellow><b>[CURRENT ROOM]</b></color> Standing in: {actualRoom.gameObject.name} | Anomaly: {anomalyText}");
            }
        }

        if (currentPlayerIndex >= 4)
        {
            SpawnForward();
            if (activeSegments.Count > 7) RemoveSegment(0);
        }
        else if (currentPlayerIndex <= 2)
        {
            SpawnBackward();
            if (activeSegments.Count > 7) RemoveSegment(activeSegments.Count - 1);
        }
    }

    void RemoveSegment(int index)
    {
        if (index >= 0 && index < activeSegments.Count)
        {
            if (activeSegments[index] != null) Destroy(activeSegments[index].gameObject);
            activeSegments.RemoveAt(index);
        }
    }

    // THE FIX: Now strictly uses the ID number to find the exact starting room
    // THE FIX: We added 'playerIsRetreating' to the parameters
    public bool IsAnomalyVisible(int startRoomID, bool playerIsRetreating)
    {
        int step = GameManager.instance.isFlowReversed ? -1 : 1;
        int startIndex = -1;

        for (int i = 0; i < activeSegments.Count; i++)
        {
            if (activeSegments[i] != null && activeSegments[i].globalIndex == startRoomID)
            {
                startIndex = i;
                break;
            }
        }

        if (startIndex == -1) return false;

        bool hasPassedCorner = false;

        for (int i = startIndex; i >= 0 && i < activeSegments.Count; i += step)
        {
            if (activeSegments[i] != null)
            {
                if (activeSegments[i].isAnomaly) return true;

                // When the scanner hits a corner...
                if (activeSegments[i].gameObject.name.Contains("Corner"))
                {
                    // If you are retreating, we allow the scanner to peek ONE room past the corner!
                    if (playerIsRetreating && !hasPassedCorner)
                    {
                        hasPassedCorner = true;
                    }
                    else
                    {
                        // Otherwise, standard strict vision block.
                        break;
                    }
                }
            }
        }
        return false;
    }

    // NEW METHOD: Cleans up the "Frankenstein" names so your console stops lying!
    public void SyncAllRoomNames()
    {
        foreach (Segment seg in activeSegments)
        {
            if (seg != null)
            {
                string baseName = seg.gameObject.name;
                int trimIndex = baseName.IndexOf("_Lvl_");

                // Strip off the old level tag
                if (trimIndex > 0) baseName = baseName.Substring(0, trimIndex);
                else baseName = baseName.Replace("(Clone)", "");

                // Stamp it with the correct current level
                seg.gameObject.name = baseName + "_Lvl_" + GameManager.instance.currentLevel;
            }
        }
    }

    public void WipeWorldMemory()
    {
        roomMemory.Clear();
        normalStreak = 0; // Reset streak on Level Up!
    }

    public void ClearAllAnomalies()
    {
        roomMemory.Clear();
        normalStreak = 0; // Reset streak when you fail!

        foreach (Segment seg in activeSegments)
        {
            if (seg != null && seg.isAnomaly)
            {
                seg.isAnomaly = false;

                AnomalyController ctrl = seg.GetComponent<AnomalyController>();
                if (ctrl != null) ctrl.Setup(false, seg.gameObject.name);

                RoomSign sign = seg.GetComponentInChildren<RoomSign>();
                if (sign != null) sign.SetLevelSign(0);
            }
        }
    }

    public static EndlessManager instance; // Add this!

    void Awake()
    {
        instance = this; // Add this!
    }
}