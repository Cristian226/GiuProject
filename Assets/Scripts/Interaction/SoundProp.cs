using UnityEngine;

public class SoundProp : Interactable
{
    public string instrument = "nai";   // key understood by AudioManager
    public string displayName = "Nai";
    public string promptVerb = "Play";

    public override string Prompt => $"{promptVerb} {displayName}";

    void Awake()
    {
        if (GetComponentInChildren<Collider>() == null)
            gameObject.AddComponent<BoxCollider>();
    }

    public override void Interact()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayTone(instrument);
        ScreenPrompt.Toast($"Playing: {displayName}", 1.6f);
    }
}
