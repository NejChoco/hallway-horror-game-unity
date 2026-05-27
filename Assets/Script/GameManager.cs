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
            endlessManager.SyncAllRoomNames();
        }

        if (currentLevel >= winLevel) Debug.Log("<color=orange><b>GAME FINISHED!</b></color>");
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
        bool anomalyVisible = leavingAnomalyRoom || endlessManager.IsAnomalyVisible(roomID, isTurningBack);
        bool expectedToTurnBack = anomalyVisible;

        bool isSuccess = (isTurningBack == expectedToTurnBack);

        if (isSuccess)
        {
            Debug.Log($"<color=green>Correct choice! Cleared Room: {roomName}</color>");

            if (anomalyVisible)
            {
                isFlowReversed = !isFlowReversed;
                if (endlessManager != null)
                {
                    endlessManager.ClearAllAnomalies();
                    // THE FIX: Tell the treadmill to delete the stale buffer!
                    endlessManager.FlushAndRebuildBuffer();
                }
            }

            IncreaseLevel();
        }
        else
        {
            Debug.Log("<color=red>Wrong choice! Resetting...</color>");
            if (isTurningBack) isFlowReversed = !isFlowReversed;

            // THE FIX: Flush the stale buffer if you fail, too!
            if (endlessManager != null) endlessManager.FlushAndRebuildBuffer();

            ResetLevel();
        }
    }
}