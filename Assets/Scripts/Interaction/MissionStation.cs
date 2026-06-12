using UnityEngine;

/// <summary>
/// The interactive spot inside a mission scene (a stove, a museum plinth, a map
/// table...). Look at it, press E, and the learning mini-game opens.
///
/// Put this on the same GameObject as a mission-content component that implements
/// <see cref="IMissionContent"/> (e.g. CuisineMission). The object needs a collider
/// so the interaction raycast can hit it. Finishing the mini-game records the
/// mission as complete but leaves the player in the room (they return via the
/// ReturnPortal door); it can be replayed any time.
/// </summary>
[DisallowMultipleComponent]
public class MissionStation : MonoBehaviour, IInteractable
{
    private IMissionContent content;

    public string Prompt => "Begin";

    void Awake()
    {
        content = GetComponent<IMissionContent>();
        if (content == null)
            Debug.LogError("[MissionStation] No IMissionContent (e.g. CuisineMission) " +
                           "found on this object. Add one next to MissionStation.");
    }

    public void Interact()
    {
        if (content == null) return;
        // onAbort is null: pressing Esc just closes the mini-game and leaves the
        // player in the room (they exit via the ReturnPortal door).
        MissionMiniGame.Get().Run(content.Title, content.BuildSteps(), OnSuccess, null);
    }

    private void OnSuccess()
    {
        // Record completion + toast, but stay in the room. The player leaves on
        // their own through the ReturnPortal whenever they're ready.
        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.MarkCurrentMissionComplete();
    }
}
