using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionRange = 4.5f;

    private Camera playerCamera;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null) playerCamera = Camera.main;
        ScreenPrompt.Ensure();
    }

    void Update()
    {
        if (UIBlocker.IsBlocked) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) return;

        Interactable target = FindTarget();
        if (target == null) return;

        ScreenPrompt.Request($"<color=#FFE08A>[{GameSettings.KeyLabel(GameAction.Interact)}]</color> {target.Prompt}");

        if (GameSettings.Pressed(GameAction.Interact) || Input.GetMouseButtonDown(0))
            target.Interact();
    }

    private Interactable FindTarget()
    {
        if (playerCamera == null) return null;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
            return hit.collider.GetComponentInParent<Interactable>();

        return null;
    }
}
