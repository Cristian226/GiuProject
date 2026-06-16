using UnityEngine;

[DisallowMultipleComponent]
public class MissionStation : Interactable
{
    private MissionContent content;

    public override string Prompt => "Begin";

    void Awake()
    {
        content = GetComponent<MissionContent>();
        if (content == null)
            Debug.LogError("[MissionStation] No MissionContent (e.g. CuisineMission) on this object.");
    }

    public override void Interact()
    {
        if (content == null) return;
        // onAbort is null: Esc just closes the mini-game and leaves the player in the room.
        MissionMiniGame.Get().Run(content.Title, content.BuildSteps(), OnSuccess, null);
    }

    private void OnSuccess()
    {
        // Record completion but stay in the room; the player leaves via the ReturnPortal.
        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.MarkCurrentMissionComplete();
    }
}
