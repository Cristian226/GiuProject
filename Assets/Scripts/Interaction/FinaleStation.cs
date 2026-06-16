using UnityEngine;

[DisallowMultipleComponent]
public class FinaleStation : Interactable
{
    private MissionContent content;

    public override string Prompt => "Begin the Final Quest";

    void Awake()
    {
        content = GetComponent<MissionContent>();
        if (content == null)
            Debug.LogError("[FinaleStation] No MissionContent (e.g. FinaleMission) on this object.");
    }

    public override void Interact()
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
