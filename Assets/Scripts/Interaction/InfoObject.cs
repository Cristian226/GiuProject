using UnityEngine;

public class InfoObject : Interactable
{
    [Header("Inspect Info")]
    public string objectName = "Object";

    [TextArea(3, 10)]
    public string info = "Describe this object here.";

    public string promptVerb = "Inspect";

    public override string Prompt => promptVerb;

    void Awake()
    {
        if (GetComponentInChildren<Collider>() == null)
            AddFittedCollider();
    }

    public override void Interact()
    {
        InfoPopup.Get().Show(objectName, info);
    }

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
