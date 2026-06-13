using UnityEngine;

/// <summary>
/// Central interaction driver. Each frame it raycasts from the centre of the
/// screen (the crosshair); if it hits an <see cref="IInteractable"/> within range
/// it shows a "[E] &lt;verb&gt;" hint and runs it on E / left-click.
///
/// This single system handles NPCs, mission stations, exit doors and inspectable
/// props alike, in every scene.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionRange = 4.5f;

    private Camera playerCamera;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null) playerCamera = Camera.main;
        ScreenPrompt.Ensure();   // crosshair + HUD exist in this scene
    }

    void Update()
    {
        if (UIBlocker.IsBlocked) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) return;

        IInteractable target = FindTarget();
        if (target == null) return;

        ScreenPrompt.Request($"<color=#FFE08A>[{GameSettings.KeyLabel(GameAction.Interact)}]</color> {target.Prompt}");

        if (GameSettings.Pressed(GameAction.Interact) || Input.GetMouseButtonDown(0))
            target.Interact();
    }

    private IInteractable FindTarget()
    {
        if (playerCamera == null) return null;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
            return hit.collider.GetComponentInParent<IInteractable>();

        return null;
    }
}
