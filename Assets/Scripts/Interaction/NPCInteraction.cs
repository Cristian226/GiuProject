using UnityEngine;

/// <summary>
/// Makes an NPC interactable. <see cref="PlayerInteraction"/> calls
/// <see cref="Interact"/> when the player looks at this NPC and presses E.
///
/// Behaviour:
///   • <see cref="MissionGiver"/> present → intro line then travel to a mission scene,
///   • otherwise                          → say the inline <see cref="lines"/> (if any).
/// </summary>
public class NPCInteraction : MonoBehaviour, IInteractable
{
    [Header("NPC Identity")]
    public string npcName = "NPC";

    [Header("Dialogue (no asset needed)")]
    [Tooltip("Lines this NPC says when talked to, shown one page at a time. " +
             "Ignored if a MissionGiver is attached.")]
    [TextArea(2, 4)]
    public string[] lines;

    /// <summary>Hint verb shown by <see cref="PlayerInteraction"/>.</summary>
    public string Prompt => $"Talk to {npcName}";

    public void Interact()
    {
        MissionGiver mg = GetComponent<MissionGiver>();
        if (mg != null) { mg.BeginInteraction(); return; }

        if (DialogueManager.Instance != null && lines != null && lines.Length > 0)
            DialogueManager.Instance.Show(npcName, lines);
    }
}
