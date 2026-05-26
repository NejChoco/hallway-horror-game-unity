using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public EndlessManager endlessManager;
    public int currentLevel = 0;
    public bool isFlowReversed = false;

    [Header("Game Settings")]
    public int winLevel = 5;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void IncreaseLevel()
    {
        currentLevel++;
        Debug.Log($"<color=yellow>Level Up! Current: {currentLevel}</color>");

        if (endlessManager != null)
        {
            endlessManager.WipeWorldMemory();

            // (We removed the aggressive ClearAllAnomalies from here so it doesn't delete your future!)

            endlessManager.SyncAllRoomNames();
        }

        if (currentLevel >= winLevel)
        {
            Debug.Log("<color=orange><b>GAME FINISHED!</b></color>");
        }
    }

    public void ResetLevel()
    {
        currentLevel = 0;
        if (endlessManager != null)
        {
            endlessManager.ClearAllAnomalies();
            endlessManager.SyncAllRoomNames();
        }
        Debug.Log("Reset to Level 0");
    }

    public void EvaluateTransition(bool isTurningBack, bool leavingAnomalyRoom, int roomID, string roomName)
    {
        bool expectedToTurnBack = isFlowReversed;
        bool anomalyVisible = leavingAnomalyRoom || endlessManager.IsAnomalyVisible(roomID, isTurningBack);

        if (anomalyVisible) expectedToTurnBack = !expectedToTurnBack;

        bool isSuccess = (isTurningBack == expectedToTurnBack);

        if (isSuccess)
        {
            Debug.Log($"<color=green>Correct choice! Cleared Room: {roomName}</color>");

            if (anomalyVisible)
            {
                isFlowReversed = !isFlowReversed;

                // THE FIX: We ONLY scrub the physical world if you just fled from an anomaly!
                // This deletes the ghost behind you, but leaves normal future rooms perfectly safe.
                if (endlessManager != null) endlessManager.ClearAllAnomalies();
            }

            IncreaseLevel();
        }
        else
        {
            Debug.Log("<color=red>Wrong choice! Resetting...</color>");
            isFlowReversed = isTurningBack;
            ResetLevel();
        }
    }
}