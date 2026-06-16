using System.Collections.Generic;
using UnityEngine;

public class MusicJukebox : Interactable
{
    private int index = -1;   // last track played; advances on each "play"

    public override string Prompt =>
        AudioManager.Instance != null && AudioManager.Instance.IsMusicPlaying
            ? "Stop the music"
            : "Play music";

    void Awake()
    {
        if (GetComponentInChildren<Collider>() == null)
            gameObject.AddComponent<BoxCollider>();
    }

    public override void Interact()
    {
        if (AudioManager.Instance == null) return;

        if (AudioManager.Instance.IsMusicPlaying)
        {
            AudioManager.Instance.StopMusic();
            ScreenPrompt.Toast("Music stopped.", 2.5f);
            return;
        }

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

    void OnDestroy()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.IsMusicPlaying)
            AudioManager.Instance.StopMusic();
    }
}
