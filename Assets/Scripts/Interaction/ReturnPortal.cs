using UnityEngine;

// The door inside a mission scene: press E to go back to the island. Needs a collider.
public class ReturnPortal : Interactable
{
    public override string Prompt => "Return to the island";

    public override void Interact()
    {
        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.LeaveMission();
        else
            Debug.LogWarning("[ReturnPortal] No GameFlowManager — cannot return.");
    }
}
