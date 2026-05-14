using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A single node in a dialogue tree.
/// Can be an NPC line, a player choice, or a terminal node that grants rewards.
/// </summary>
[CreateAssetMenu(fileName = "NewDialogueNode", menuName = "Dialogue/Dialogue Node")]
public class DialogueNode : ScriptableObject
{
    public enum Speaker { NPC, Player }

    [Header("Content")]
    public Speaker speaker = Speaker.NPC;

    [TextArea(2, 6)]
    public string text;

    [Header("Choices / Continuations")]
    [Tooltip("If empty and no reward, dialogue ends. If one entry, it auto-advances (linear). If multiple, player picks.")]
    public List<DialogueChoice> choices = new List<DialogueChoice>();

    [Header("Reward (on reaching this node)")]
    [Tooltip("Optional quest to start when this node is reached.")]
    public QuestData questToGrant;

    [Tooltip("Optional item prefab to give the player when this node is reached.")]
    public GameObject itemToGrant;

    [Tooltip("Optional display name of the item, shown in the UI.")]
    public string itemDisplayName;
}

[System.Serializable]
public class DialogueChoice
{
    [Tooltip("The label shown on the player's choice button (keep it short).")]
    public string choiceLabel;

    [Tooltip("The next node to go to when this choice is picked. Leave null to end dialogue.")]
    public DialogueNode nextNode;
}