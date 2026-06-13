using UnityEngine;

/// <summary>
/// A prop that plays a synthesised instrument tone when interacted with — used by
/// the Music environment so the player can "hear" each instrument (the tone is
/// generated at runtime by <see cref="AudioManager"/>, no audio files required).
/// </summary>
public class SoundProp : MonoBehaviour, IInteractable
{
    [Tooltip("Instrument key understood by AudioManager (nai, vioara, tambal, cobza, cimpoi, fluier).")]
    public string instrument = "nai";
    public string displayName = "Nai";
    public string promptVerb = "Play";

    public string Prompt => $"{promptVerb} {displayName}";

    void Awake()
    {
        if (GetComponentInChildren<Collider>() == null)
            gameObject.AddComponent<BoxCollider>();
    }

    public void Interact()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayTone(instrument);
        ScreenPrompt.Toast($"Playing: {displayName}", 1.6f);
    }
}
