using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionRange = 4f;

    private Camera playerCamera;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null) playerCamera = Camera.main;
    }

    void Update()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
            TryInteract();
    }

    private void TryInteract()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            NPCInteraction npc = hit.collider.GetComponentInParent<NPCInteraction>();
            if (npc != null) npc.Interact();
        }
    }
}
