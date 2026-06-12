using UnityEngine;

/// <summary>
/// The door inside a mission scene. Look at it and press E to go back to the
/// island. You can leave whether or not you finished the mini-game — completing a
/// mission no longer teleports you out automatically.
/// Needs a collider so the interaction raycast can hit it.
/// </summary>
public class ReturnPortal : MonoBehaviour, IInteractable
{
    public string Prompt => "Return to the island";

    public void Interact()
    {
        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.LeaveMission();
        else
            Debug.LogWarning("[ReturnPortal] No GameFlowManager — cannot return.");
    }
}
