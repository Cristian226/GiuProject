using UnityEngine;

/// <summary>
/// The final-quest giver, placed on the little boy NPC in the city. He is always
/// there to talk to:
///   • before every collectible is gathered → he asks you to come back later,
///   • once everything is collected           → he offers the Grand Final Challenge,
///   • after you finish it                     → he congratulates you.
///
/// Adds a fitted collider on Awake if the model has none, so the interaction raycast
/// can hit it (the CityPeople kid prefab ships without colliders).
/// </summary>
public class FinaleGiver : MonoBehaviour, IInteractable
{
    public string npcName = "Andrei";
    public string finaleSceneName = "FinaleScene";
    public string finaleMissionId = "finale";

    public string Prompt => $"Talk to {npcName}";

    void Awake()
    {
        if (GetComponentInChildren<Collider>() == null)
            AddFittedCollider();
    }

    public void Interact()
    {
        if (DialogueManager.Instance == null) return;

        GameProgress p = GameProgress.Instance;

        // 1) Already finished the whole game.
        if (p != null && p.FinalQuestComplete)
        {
            DialogueManager.Instance.Show(npcName, new[]
            {
                "You're a real expert on Romania now — well done!",
                "Safe travels on the rest of your journey, my friend."
            });
            return;
        }

        // 2) Not everything collected yet → come back later.
        if (p == null || !p.AllCollected())
        {
            int have = p != null ? p.CollectedCount : 0;
            int need = Catalog.Required().Count;
            DialogueManager.Instance.Show(npcName, new[]
            {
                "Hi! I'm the guardian of the city's final challenge.",
                $"But you haven't gathered all the cultural treasures yet ({have}/{need}).",
                "Finish the cuisine, geography, history and music missions, then come back to me!"
            });
            return;
        }

        // 3) Everything collected → offer the final quest.
        DialogueManager.Instance.Show(npcName, new[]
        {
            "Amazing — you've collected every treasure: the dictionary, the compass, the scroll and the musical note!",
            "Only one challenge remains: the Grand Final Challenge. Are you ready?"
        }, OnAccept);
    }

    private void OnAccept()
    {
        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.StartMission(finaleSceneName, finaleMissionId);
    }

    // Box collider fitted to the model's renderers (mirrors InfoObject).
    private void AddFittedCollider()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        BoxCollider box = gameObject.AddComponent<BoxCollider>();
        if (renderers.Length == 0) { box.size = new Vector3(0.8f, 1.6f, 0.8f); box.center = new Vector3(0, 0.8f, 0); return; }

        Bounds b = renderers[0].bounds;
        foreach (Renderer r in renderers) b.Encapsulate(r.bounds);

        box.center = transform.InverseTransformPoint(b.center);
        Vector3 ls = transform.lossyScale;
        box.size = new Vector3(
            b.size.x / Mathf.Max(Mathf.Abs(ls.x), 0.0001f),
            b.size.y / Mathf.Max(Mathf.Abs(ls.y), 0.0001f),
            b.size.z / Mathf.Max(Mathf.Abs(ls.z), 0.0001f));
    }
}
