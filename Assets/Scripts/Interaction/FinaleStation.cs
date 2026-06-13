using UnityEngine;

/// <summary>
/// The station that runs the final quest. Like <see cref="MissionStation"/> but on
/// success it marks the final quest complete (rather than a catalog mission) and
/// shows a victory message. Put it on the same object as <see cref="FinaleMission"/>.
/// </summary>
[DisallowMultipleComponent]
public class FinaleStation : MonoBehaviour, IInteractable
{
    private IMissionContent content;

    public string Prompt => "Begin the Final Quest";

    void Awake()
    {
        content = GetComponent<IMissionContent>();
        if (content == null)
            Debug.LogError("[FinaleStation] No IMissionContent (e.g. FinaleMission) on this object.");
    }

    public void Interact()
    {
        if (content == null) return;
        MissionMiniGame.Get().Run(content.Title, content.BuildSteps(), OnSuccess, null);
    }

    private void OnSuccess()
    {
        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.CompleteFinalQuest();
            GameProgress.Instance.AddXp(300);
        }
        CelebrationManager.Get().PlayFinale();
    }
}
