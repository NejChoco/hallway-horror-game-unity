using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnomalyPair
{
    public string anomalyName;
    public GameObject normalObject;
    public GameObject anomalyObject;
}

public class AnomalyController : MonoBehaviour
{
    [Header("List of Possible Anomalies")]
    public List<AnomalyPair> possibleAnomalies = new List<AnomalyPair>();

    // We added 'string roomName' so the controller knows its own identity!
    public void Setup(bool isAnomaly, string roomName)
    {
        foreach (var pair in possibleAnomalies)
        {
            if (pair.normalObject != null) pair.normalObject.SetActive(true);
            if (pair.anomalyObject != null) pair.anomalyObject.SetActive(false);
        }

        if (isAnomaly && possibleAnomalies.Count > 0)
        {
            int randomIndex = Random.Range(0, possibleAnomalies.Count);
            AnomalyPair chosenAnomaly = possibleAnomalies[randomIndex];

            if (chosenAnomaly.normalObject != null) chosenAnomaly.normalObject.SetActive(false);
            if (chosenAnomaly.anomalyObject != null) chosenAnomaly.anomalyObject.SetActive(true);

            // THE FIX: Explicitly state that this is happening in the distance!
            Debug.Log($"<color=magenta><b>[BACKGROUND GEN]</b> {roomName} secretly spawned Anomaly: <b>{chosenAnomaly.anomalyName}</b></color>");
        }
    }
}