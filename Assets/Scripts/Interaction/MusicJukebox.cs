using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A concert-hall jukebox. Each interaction advances to the next track found in
/// <c>Resources/Music</c> (the anthem, a folk tune, a Zamfir piece, …). If no music
/// files are present it says so, so the scene still works empty-handed.
/// </summary>
public class MusicJukebox : MonoBehaviour, IInteractable
{
    private int index = -1;

    public string Prompt => "Play next song";

    void Awake()
    {
        if (GetComponentInChildren<Collider>() == null)
            gameObject.AddComponent<BoxCollider>();
    }

    public void Interact()
    {
        if (AudioManager.Instance == null) return;

        List<string> names = AudioManager.Instance.PlaylistNames();
        if (names.Count == 0)
        {
            ScreenPrompt.Toast("No music files yet — add clips to Assets/Resources/Music.", 3.5f);
            return;
        }

        index = (index + 1) % names.Count;
        AudioManager.Instance.PlayMusicNamed(names[index]);
        ScreenPrompt.Toast($"Now playing: {names[index]}", 3f);
    }
}
