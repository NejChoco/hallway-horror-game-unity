using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessManager : MonoBehaviour
{
    [Header("Ending Setup")]
    public Segment endingHallwayPrefab;
    public int finalLevel = 10;
    private bool hasSpawnedEnding = false;

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
        if (hasSpawnedEnding) return;

        Segment prefab;
        bool lastWasCorner = activeSegments[activeSegments.Count - 1].gameObject.name.Contains("Corner");
        bool isEndingHallway = false;

        if (GameManager.instance.currentLevel >= finalLevel)
        {
            prefab = endingHallwayPrefab;
            hasSpawnedEnding = true;
            isEndingHallway = true;
            Debug.Log("<color=magenta><b>SPAWNING FINAL VOID TUNNEL (FORWARD)</b></color>");
        }
        else
        {
            Segment basePrefab = GameManager.instance.isFlowReversed ? backwardHallway : normalHallway;

            if (GameManager.instance.currentLevel >= finalLevel - 1)
            {
                // Level 9: Force straight line
                prefab = basePrefab;
            }
            else
            {
                // Normal random generation
                prefab = lastWasCorner ? basePrefab : corners[Random.Range(0, corners.Length)];
            }
        }

        Segment newSeg = Instantiate(prefab);
        Transform lastOut = activeSegments[activeSegments.Count - 1].socketOut;

        newSeg.transform.rotation = lastOut.rotation * Quaternion.Inverse(newSeg.socketIn.localRotation);
        newSeg.transform.position = lastOut.position - (newSeg.socketIn.position - newSeg.transform.position);

        frontGlobalIndex++;
        newSeg.globalIndex = frontGlobalIndex;

        SetupSegment(newSeg);
        activeSegments.Add(newSeg);

        // Fade in — slower and with lights for the ending hallway
        if (isEndingHallway)
            StartCoroutine(FadeInSegmentWithLights(newSeg, 0.6f));
        else
            StartCoroutine(FadeInSegment(newSeg, 0.3f));
    }

    void SpawnBackward()
    {
        if (hasSpawnedEnding) return;

        Segment prefab;
        bool isEndingHallway = false;

        if (GameManager.instance.currentLevel >= finalLevel)
        {
            prefab = endingHallwayPrefab;
            hasSpawnedEnding = true;
            isEndingHallway = true;
            Debug.Log("<color=magenta><b>SPAWNING FINAL VOID TUNNEL (BACKWARD)</b></color>");
        }
        else
        {
            Segment basePrefab = GameManager.instance.isFlowReversed ? backwardHallway : normalHallway;

            if (GameManager.instance.currentLevel >= finalLevel - 1)
            {
                prefab = basePrefab;
            }
            else
            {
                bool firstWasCorner = activeSegments[0].gameObject.name.Contains("Corner");
                prefab = firstWasCorner ? basePrefab : corners[Random.Range(0, corners.Length)];
            }
        }

        Segment newSeg = Instantiate(prefab, new Vector3(0, -1000, 0), Quaternion.identity);
        Collider[] colliders = newSeg.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders) col.enabled = false;

        Transform currentIn = activeSegments[0].socketIn;

        newSeg.transform.rotation = currentIn.rotation * Quaternion.Inverse(newSeg.socketOut.localRotation);
        newSeg.transform.position = currentIn.position - (newSeg.socketOut.position - newSeg.transform.position);

        StartCoroutine(EnableCollidersAfterDelay(colliders));

        // Fade in — slower and with lights for the ending hallway
        if (isEndingHallway)
            StartCoroutine(FadeInSegmentWithLights(newSeg, 0.6f));
        else
            StartCoroutine(FadeInSegment(newSeg, 0.3f));

        backGlobalIndex--;
        newSeg.globalIndex = backGlobalIndex;

        SetupSegment(newSeg);
        activeSegments.Insert(0, newSeg);

        // After inserting at index 0, everything shifted up by 1.
        // Correct the player index so Update() doesn't think they moved rooms.
        currentPlayerIndex++;
    }

    IEnumerator EnableCollidersAfterDelay(Collider[] colliders)
    {
        yield return new WaitForSeconds(0.1f);
        foreach (Collider col in colliders)
        {
            if (col != null) col.enabled = true;
        }
    }

    IEnumerator FadeInSegment(Segment seg, float duration)
    {
        if (seg == null) yield break;

        Renderer[] renderers = seg.GetComponentsInChildren<Renderer>();

        List<Material> mats = new List<Material>();
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                if (m.HasProperty("_Color"))
                {
                    Color c = m.color;
                    c.a = 0f;
                    m.color = c;
                    mats.Add(m);

                    m.SetFloat("_Mode", 2);
                    m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    m.SetInt("_ZWrite", 0);
                    m.DisableKeyword("_ALPHATEST_ON");
                    m.EnableKeyword("_ALPHABLEND_ON");
                    m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    m.renderQueue = 3000;
                }
            }
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);

            foreach (Material m in mats)
            {
                if (m != null)
                {
                    Color c = m.color;
                    c.a = alpha;
                    m.color = c;
                }
            }

            yield return null;
        }

        foreach (Material m in mats)
        {
            if (m != null)
            {
                Color c = m.color;
                c.a = 1f;
                m.color = c;

                m.SetFloat("_Mode", 0);
                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                m.SetInt("_ZWrite", 1);
                m.DisableKeyword("_ALPHATEST_ON");
                m.DisableKeyword("_ALPHABLEND_ON");
                m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                m.renderQueue = -1;
            }
        }
    }

    IEnumerator FadeInSegmentWithLights(Segment seg, float duration)
    {
        if (seg == null) yield break;

        Renderer[] renderers = seg.GetComponentsInChildren<Renderer>();
        Light[] lights = seg.GetComponentsInChildren<Light>();

        // Store and zero out light intensities
        float[] originalIntensities = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++)
        {
            originalIntensities[i] = lights[i].intensity;
            lights[i].intensity = 0f;
        }

        List<Material> mats = new List<Material>();
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                if (m.HasProperty("_Color"))
                {
                    Color c = m.color;
                    c.a = 0f;
                    m.color = c;

                    m.SetFloat("_Mode", 2);
                    m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    m.SetInt("_ZWrite", 0);
                    m.DisableKeyword("_ALPHATEST_ON");
                    m.EnableKeyword("_ALPHABLEND_ON");
                    m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    m.renderQueue = 3000;
                    mats.Add(m);
                }
            }
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);

            foreach (Material m in mats)
            {
                if (m != null)
                {
                    Color c = m.color;
                    c.a = alpha;
                    m.color = c;
                }
            }

            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                    lights[i].intensity = Mathf.Lerp(0f, originalIntensities[i], alpha);
            }

            yield return null;
        }

        foreach (Material m in mats)
        {
            if (m != null)
            {
                Color c = m.color;
                c.a = 1f;
                m.color = c;

                m.SetFloat("_Mode", 0);
                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                m.SetInt("_ZWrite", 1);
                m.DisableKeyword("_ALPHATEST_ON");
                m.DisableKeyword("_ALPHABLEND_ON");
                m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                m.renderQueue = -1;
            }
        }

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
                lights[i].intensity = originalIntensities[i];
        }
    }

    void SetupSegment(Segment newSeg)
    {
        bool isCorner = newSeg.gameObject.name.Contains("Corner");
        bool isAnomaly = false;

        newSeg.gameObject.name = newSeg.gameObject.name.Replace("(Clone)", "") + "_Lvl_" + GameManager.instance.currentLevel;

        if (GameManager.instance.currentLevel >= finalLevel)
        {
            isAnomaly = false;
        }
        else if (roomMemory.ContainsKey(newSeg.globalIndex))
        {
            isAnomaly = roomMemory[newSeg.globalIndex];
        }
        else
        {
            if (!isCorner && GameManager.instance.currentLevel > 0)
            {
                if (forceAnomaliesForTesting) isAnomaly = true;
                else if (isFlushingBuffer) isAnomaly = Random.value < 0.10f;
                else if (normalStreak >= 2) isAnomaly = true;
                else isAnomaly = Random.value < 0.35f;

                if (isAnomaly) normalStreak = 0;
                else normalStreak++;
            }
            else
            {
                isAnomaly = false;
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

        // SAFEGUARD: Once the ending hallway is spawned, keep a larger buffer
        // behind the player so turning around doesn't show an empty void.
        int bufferBehind = hasSpawnedEnding ? 4 : 2;
        int bufferAhead = hasSpawnedEnding ? 2 : 4;
        int maxSegments = bufferBehind + bufferAhead + 1; // +1 for the room player stands in

        if (currentPlayerIndex >= bufferAhead + 1)
        {
            SpawnForward();
            // Only remove the tail if we have enough segments behind the player
            if (activeSegments.Count > maxSegments && currentPlayerIndex > bufferBehind)
                RemoveSegment(0);
        }
        else if (currentPlayerIndex <= bufferBehind - 1)
        {
            SpawnBackward();
            // Only remove the head if we have enough segments ahead of the player
            if (activeSegments.Count > maxSegments && currentPlayerIndex < activeSegments.Count - bufferAhead)
                RemoveSegment(activeSegments.Count - 1);
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

        isFlushingBuffer = true;

        if (GameManager.instance.isFlowReversed)
        {
            for (int i = 0; i < 3; i++) SpawnBackward();
            for (int i = 0; i < 3; i++) SpawnForward();
        }
        else
        {
            for (int i = 0; i < 3; i++) SpawnForward();
            for (int i = 0; i < 3; i++) SpawnBackward();
        }

        isFlushingBuffer = false;
        currentPlayerIndex = 3;
    }
}