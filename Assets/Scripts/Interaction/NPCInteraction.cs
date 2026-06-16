using UnityEngine;

public class NPCInteraction : Interactable
{
    [Header("NPC Identity")]
    public string npcName = "NPC";

    [Header("Dialogue (ignored if a MissionGiver is attached)")]
    [TextArea(2, 4)]
    public string[] lines;

    public override string Prompt => $"Talk to {npcName}";

    public override void Interact()
    {
        MissionGiver mg = GetComponent<MissionGiver>();
        if (mg != null) { mg.BeginInteraction(); return; }

        if (DialogueManager.Instance != null && lines != null && lines.Length > 0)
            DialogueManager.Instance.Show(npcName, lines);
    }
}
