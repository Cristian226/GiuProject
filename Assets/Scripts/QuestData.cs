using UnityEngine;

/// <summary>
/// Simple quest data container. Assign in Inspector on a DialogueNode.
/// </summary>
[CreateAssetMenu(fileName = "NewQuest", menuName = "Dialogue/Quest")]
public class QuestData : ScriptableObject
{
    [Header("Quest Info")]
    public string questName = "Unnamed Quest";

    [TextArea(2, 4)]
    public string questDescription = "A new quest.";

    // Extend this with objectives, rewards, flags etc. as your game grows.
}