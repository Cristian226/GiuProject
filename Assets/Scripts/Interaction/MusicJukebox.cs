using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A concert-hall jukebox. While silent, each interaction plays the next track found
/// in <c>Resources/Music</c> (the anthem, a folk tune, a Zamfir piece, …); while a
/// track is playing, interacting again stops it. The music plays through the
/// persistent <see cref="AudioManager"/>, so the jukebox also silences itself when
/// the theatre scene unloads — the song never follows the player into the next scene.
/// If no music files are present it says so, so the scene still works empty-handed.
/// </summary>
public class MusicJukebox : MonoBehaviour, IInteractable
{
    private int index = -1;   // last track played; advances on each "play"

    public string Prompt =>
        AudioManager.Instance != null && AudioManager.Instance.IsMusicPlaying
            ? "Stop the music"
            : "Play music";

    void Awake()
    {
        if (GetComponentInChildren<Collider>() == null)
            gameObject.AddComponent<BoxCollider>();
    }

    public void Interact()
    {
        if (AudioManager.Instance == null) return;

        // Playing → stop. This is the "off switch" the jukebox needs.
        if (AudioManager.Instance.IsMusicPlaying)
        {
            AudioManager.Instance.StopMusic();
            ScreenPrompt.Toast("Music stopped.", 2.5f);
            return;
        }

        // Silent → play the next track in the playlist.
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

    // Leaving the theatre destroys this object (the scene unloads); silence the
    // persistent music source so it doesn't keep playing in the city or the menu.
    void OnDestroy()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.IsMusicPlaying)
            AudioManager.Instance.StopMusic();
    }
}
