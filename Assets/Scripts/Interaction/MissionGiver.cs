using UnityEngine;

[RequireComponent(typeof(NPCInteraction))]
public class MissionGiver : MonoBehaviour
{
    [Header("Mission")]
    public string missionSceneName = "CuisineScene";
    public string missionId = "cuisine";

    [Header("Dialogue")]
    [TextArea(2, 5)]
    public string introText = "Want to learn something about Romania? Step through and let's begin!";

    [TextArea(2, 4)]
    public string alreadyDoneText = "Welcome back! Want to visit again?";

    public void BeginInteraction()
    {
        if (DialogueManager.Instance == null) return;

        bool done = GameFlowManager.Instance != null && GameFlowManager.Instance.IsComplete(missionId);
        string intro = done ? alreadyDoneText : introText;
        DialogueManager.Instance.Show(GetComponent<NPCInteraction>().npcName, intro, OnAccepted);
    }

    private void OnAccepted()
    {
        if (GameFlowManager.Instance == null)
        {
            Debug.LogError("[MissionGiver] No GameFlowManager in the scene.");
            return;
        }
        GameFlowManager.Instance.StartMission(missionSceneName, missionId);
    }
}
