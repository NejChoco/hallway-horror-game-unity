using System.Collections.Generic;
using UnityEngine;

public class EndlessManager : MonoBehaviour
{
    [Header("Debug Options")]
    public bool forceAnomaliesForTesting = false;

    [Header("Prefabs")]
    public Segment normalHallway;
    public Segment backwardHallway;
    public Segment[] corners;

    public int currentPlayerIndex = 0;
    public List<Segment> activeSegments = new List<Segment>();

    private Transform playerTransform;
    private Segment currentStandingSegment = null;

    private int normalStreak = 0;
    private Dictionary<int, bool> roomMemory = new Dictionary<int, bool>();
    private int frontGlobalIndex = 0;
    private int backGlobalIndex = 0;

    // The Grace Period flag
    private bool isFlushingBuffer = false;

    public static EndlessManager instance;

    void Awake() { instance = this; }

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
        Segment basePrefab = GameManager.instance.isFlowReversed ? backwardHallway : normalHallway;
        Segment prefab = lastWasCorner ? basePrefab : corners[Random.Range(0, corners.Length)];

        Segment newSeg = Instantiate(prefab);
        Transform lastOut = activeSegments[activeSegments.Count - 1].socketOut;

        newSeg.transform.rotation = lastOut.rotation * Quaternion.Inverse(newSeg.socketIn.localRotation);
        newSeg.transform.position = lastOut.position - (newSeg.socketIn.position - newSeg.transform.position);

        frontGlobalIndex++;
        newSeg.globalIndex = frontGlobalIndex;

        SetupSegment(newSeg);
        activeSegments.Add(newSeg);
    }

    void SpawnBackward()
    {
        bool firstWasCorner = activeSegments[0].gameObject.name.Contains("Corner");
        Segment basePrefab = GameManager.instance.isFlowReversed ? backwardHallway : normalHallway;
        Segment prefab = firstWasCorner ? basePrefab : corners[Random.Range(0, corners.Length)];

        Segment newSeg = Instantiate(prefab);
        Transform firstIn = activeSegments[0].socketIn;

        newSeg.transform.rotation = firstIn.rotation * Quaternion.Inverse(newSeg.socketOut.localRotation);
        newSeg.transform.position = firstIn.position - (newSeg.socketOut.position - newSeg.transform.position);

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

        if (roomMemory.ContainsKey(newSeg.globalIndex)) isAnomaly = roomMemory[newSeg.globalIndex];
        else
        {
            if (!isCorner && GameManager.instance.currentLevel > 0)
            {
                if (forceAnomaliesForTesting) isAnomaly = true;

                // THE PARANOIA MECHANIC: 
                // Instead of 0% (safe), there is a rare 10% chance of a back-to-back anomaly!
                else if (isFlushingBuffer) isAnomaly = Random.value < 0.10f;

                else if (normalStreak >= 2) isAnomaly = true;
                else isAnomaly = Random.value < 0.35f; // Standard 35% chance

                if (isAnomaly) normalStreak = 0;
                else normalStreak++;
            }
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

        Transform entrance = activeSegments[2].socketIn;

        // NEGATIVE X-AXIS: Subtracting the right vector pushes you safely inside the room!
        playerTransform.position = entrance.position - (entrance.right * 4f) + (Vector3.up * 1.5f);

        playerTransform.rotation = entrance.rotation;

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

            Vector3 trueCenter = (activeSegments[i].socketIn.position + activeSegments[i].socketOut.position) / 2f;
            float dist = Vector3.Distance(playerTransform.position, trueCenter);

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

                if (activeSegments[i].gameObject.name.Contains("Corner"))
                {
                    if (playerIsRetreating && !hasPassedCorner) hasPassedCorner = true;
                    else break;
                }
            }
        }
        return false;
    }

    public void SyncAllRoomNames()
    {
        foreach (Segment seg in activeSegments)
        {
            if (seg != null)
            {
                string baseName = seg.gameObject.name;
                int trimIndex = baseName.IndexOf("_Lvl_");

                if (trimIndex > 0) baseName = baseName.Substring(0, trimIndex);
                else baseName = baseName.Replace("(Clone)", "");

                seg.gameObject.name = baseName + "_Lvl_" + GameManager.instance.currentLevel;
            }
        }
    }

    public void WipeWorldMemory() { roomMemory.Clear(); }

    public void ClearAllAnomalies()
    {
        roomMemory.Clear();
        normalStreak = 0;

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

    public void FlushAndRebuildBuffer()
    {
        if (activeSegments.Count == 0) return;

        Segment currentRoom = activeSegments[currentPlayerIndex];

        for (int i = activeSegments.Count - 1; i >= 0; i--)
        {
            if (i != currentPlayerIndex)
            {
                if (activeSegments[i] != null) Destroy(activeSegments[i].gameObject);
                activeSegments.RemoveAt(i);
            }
        }

        frontGlobalIndex = currentRoom.globalIndex;
        backGlobalIndex = currentRoom.globalIndex;

        // Activate the Grace Period before building the new buffer!
        isFlushingBuffer = true;
        for (int i = 0; i < 3; i++) SpawnForward();
        for (int i = 0; i < 3; i++) SpawnBackward();
        isFlushingBuffer = false;

        currentPlayerIndex = 3;
    }
}