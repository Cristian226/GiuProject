using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    //[TextArea(3, 8)]
    //public string dialogueText = "Hello! How can I help you?";

    [Tooltip("Where the player will be teleported when they accept.")]
    public Transform teleportTarget;

    [Tooltip("Max distance from which the player can trigger this NPC.")]
    public float interactionDistance = 4f;

    private Transform playerTransform;

    [Header("NPC Identity")]
    public string npcName = "NPC";

    [Header("Dialogue")]
    public DialogueNode rootNode;

    [Header("Interaction")]
    public GameObject interactPromptUI;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    public bool PlayerInRange()
    {
        if (playerTransform == null) return false;
        return Vector3.Distance(transform.position, playerTransform.position) <= interactionDistance;
    }

    public void Interact()
    {
        PoliceDialogue pd = GetComponent<PoliceDialogue>();
        if (pd != null) { pd.BeginInteraction(); return; }

        //if (!PlayerInRange()) return;
        if (DialogueManager.Instance == null) return;
        DialogueManager.Instance.StartDialogue(npcName, rootNode);
    }

    private void OnAccept()
    {
        if (teleportTarget == null)
        {
            Debug.LogWarning($"[NPCInteraction] No teleport target set on {gameObject.name}.");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Disable CharacterController before moving, re-enable after — Unity requirement
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = teleportTarget.position;
        if (cc != null) cc.enabled = true;
    }

    void Update()
    {
        if (interactPromptUI != null)
            interactPromptUI.SetActive(PlayerInRange());
    }
}
