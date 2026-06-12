using UnityEngine;

/// <summary>
/// Drop this on any static prop (food, furniture, a painting, a statue...) to make
/// it inspectable. Look at it, press E, and a popup shows its name + description.
///
/// If the object has no collider, a fitted BoxCollider is added automatically so
/// it can be hit by the interaction raycast.
/// </summary>
public class InfoObject : MonoBehaviour, IInteractable
{
    [Header("Inspect Info")]
    public string objectName = "Object";

    [TextArea(3, 10)]
    public string info = "Describe this object here.";

    [Tooltip("Verb shown in the on-screen hint.")]
    public string promptVerb = "Inspect";

    public string Prompt => promptVerb;

    void Awake()
    {
        if (GetComponentInChildren<Collider>() == null)
            AddFittedCollider();
    }

    public void Interact()
    {
        InfoPopup.Get().Show(objectName, info);
    }

    // Adds a BoxCollider sized to the visible mesh so raycasts can hit the prop.
    private void AddFittedCollider()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        BoxCollider box = gameObject.AddComponent<BoxCollider>();
        if (renderers.Length == 0) return;

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
