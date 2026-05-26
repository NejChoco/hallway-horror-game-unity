using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    public Segment parentSegment;
    public bool isEntrance = false;

    private void OnTriggerEnter(Collider other)
    {
        if (parentSegment.gameObject.name.Contains("Corner")) return;
        if (!other.CompareTag("Player")) return;

        // THE FIX: Only let the room the player is actually STANDING IN trigger events
        if (EndlessManager.instance.currentPlayerIndex != -1)
        {
            Segment currentSeg = EndlessManager.instance.activeSegments[EndlessManager.instance.currentPlayerIndex];
            if (currentSeg != parentSegment) return;
        }

        Vector3 hallwayForward = parentSegment.socketOut.position - parentSegment.socketIn.position;
        hallwayForward.Normalize();
        Vector3 playerDirection = other.transform.forward;
        float dot = Vector3.Dot(playerDirection, hallwayForward);

        int roomID = parentSegment.globalIndex;
        string roomName = parentSegment.gameObject.name;

        // PROGRESSING
        if (dot > 0.3f)
        {
            bool isLeaving = GameManager.instance.isFlowReversed ? isEntrance : !isEntrance;
            if (isLeaving)
            {
                GameManager.instance.EvaluateTransition(false, parentSegment.isAnomaly, roomID, roomName);
            }
        }
        // RETREATING
        else if (dot < -0.3f)
        {
            GameManager.instance.EvaluateTransition(true, parentSegment.isAnomaly, roomID, roomName);
        }
    }
}