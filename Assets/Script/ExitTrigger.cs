using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    public Segment parentSegment;
    public bool isEntrance = false;

    private static float globalLastTriggerTime = 0f;
    private const float COOLDOWN = 1.0f;

    private void OnTriggerEnter(Collider other)
    {
        if (parentSegment.gameObject.name.Contains("Corner")) return;
        if (!other.CompareTag("Player") || Time.time < globalLastTriggerTime + COOLDOWN) return;

        Vector3 hallwayForward = parentSegment.socketOut.position - parentSegment.socketIn.position;
        hallwayForward.Normalize();

        bool reversed = GameManager.instance.isFlowReversed;
        Vector3 safeFlowDirection = reversed ? -hallwayForward : hallwayForward;
        Vector3 playerDirection = other.transform.forward;

        float dot = Vector3.Dot(playerDirection, safeFlowDirection);

        int roomID = parentSegment.globalIndex;
        string roomName = parentSegment.gameObject.name;

        string triggerType = isEntrance ? "<color=orange>[ENTRANCE]</color>" : "<color=magenta>[EXIT]</color>";

        if (dot > 0.3f)
        {
            bool isLeavingForward = reversed ? isEntrance : !isEntrance;
            if (isLeavingForward)
            {
                globalLastTriggerTime = Time.time;
                Debug.Log($"{triggerType} <color=cyan>[TRIGGER INFO]</color> Progressing | Room: {roomName}");
                GameManager.instance.EvaluateTransition(false, parentSegment.isAnomaly, roomID, roomName);
            }
        }
        else if (dot < -0.3f)
        {
            bool isLeavingBackward = reversed ? !isEntrance : isEntrance;
            if (isLeavingBackward)
            {
                globalLastTriggerTime = Time.time;
                Debug.Log($"{triggerType} <color=cyan>[TRIGGER INFO]</color> Retreating | Room: {roomName}");
                GameManager.instance.EvaluateTransition(true, parentSegment.isAnomaly, roomID, roomName);
            }
        }
    }
}