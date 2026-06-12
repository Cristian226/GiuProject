using UnityEngine;

/// <summary>
/// Attach next to <see cref="NPCInteraction"/> on an island NPC to turn that NPC
/// into a mission giver. When the player interacts, an intro line is shown; on
/// Accept the player is sent to the mission scene via <see cref="GameFlowManager"/>.
/// The NPC's name comes from the sibling <see cref="NPCInteraction"/>.
/// </summary>
[RequireComponent(typeof(NPCInteraction))]
public class MissionGiver : MonoBehaviour
{
    [Header("Mission")]
    [Tooltip("Scene to load for this mission. Must be in File > Build Settings.")]
    public string missionSceneName = "CuisineScene";

    [Tooltip("Unique id used to remember whether this mission was completed.")]
    public string missionId = "cuisine";

    [Header("Dialogue")]
    [TextArea(2, 5)]
    public string introText = "Want to learn something about Romania? Step through and let's begin!";

    [Tooltip("Shown when revisiting a mission you already finished. Accepting still takes you back in.")]
    [TextArea(2, 4)]
    public string alreadyDoneText = "Welcome back! Want to visit again?";

    /// <summary>Entry point — called by <see cref="NPCInteraction.Interact"/>.</summary>
    public void BeginInteraction()
    {
        if (DialogueManager.Instance == null) return;

        // Completed missions can be re-entered; only the intro line changes.
        bool done = GameFlowManager.Instance != null && GameFlowManager.Instance.IsComplete(missionId);
        string intro = done ? alreadyDoneText : introText;
        DialogueManager.Instance.Show(GetComponent<NPCInteraction>().npcName, intro, OnAccepted);
    }

    private void OnAccepted()
    {
        if (GameFlowManager.Instance == null)
        {
            Debug.LogError("[MissionGiver] No GameFlowManager in the scene. " +
                           "Add the GameFlowManager object to MainScene.");
            return;
        }
        GameFlowManager.Instance.StartMission(missionSceneName, missionId);
    }
}
