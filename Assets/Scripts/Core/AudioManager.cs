using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persistent audio hub. Owns a looping music source and a one-shot effects source,
/// keeps their volumes in sync with <see cref="GameSettings"/> (master · music ·
/// effects), and can synthesise simple instrument tones at runtime so the music
/// mission works even with no audio files imported.
///
/// <para>Drop real clips in <c>Assets/Resources/Music</c> (e.g. the anthem, a folk
/// tune, a Zamfir pan-flute piece) and they become available to <see cref="PlayMusicNamed"/>
/// and the in-game jukebox by file name. See <c>Assets/Resources/Music/README</c>.</para>
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource music;
    private AudioSource sfx;

    private readonly Dictionary<string, AudioClip> toneCache = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> playlist;   // name -> clip from Resources/Music

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance == null)
            new GameObject("AudioManager (auto)").AddComponent<AudioManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        music = gameObject.AddComponent<AudioSource>();
        music.loop = true;
        music.playOnAwake = false;
        music.spatialBlend = 0f;   // 2D

        sfx = gameObject.AddComponent<AudioSource>();
        sfx.loop = false;
        sfx.playOnAwake = false;
        sfx.spatialBlend = 0f;

        GameSettings.EnsureLoaded();
        ApplyVolumes();
        GameSettings.Changed += ApplyVolumes;
    }

    void OnDestroy()
    {
        if (Instance == this) GameSettings.Changed -= ApplyVolumes;
    }

    /// <summary>Master via the global listener; music/effects via their own sources.</summary>
    public void ApplyVolumes()
    {
        AudioListener.volume = GameSettings.MasterVolume;
        if (music != null) music.volume = GameSettings.MusicVolume;
        if (sfx != null)   sfx.volume   = GameSettings.EffectsVolume;
    }

    // ── Music ─────────────────────────────────────────────────────────────────
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null || music == null) return;
        music.clip = clip;
        music.loop = loop;
        music.volume = GameSettings.MusicVolume;
        music.Play();
    }

    public void StopMusic() { if (music != null) music.Stop(); }

    public bool IsMusicPlaying => music != null && music.isPlaying;

    /// <summary>Play a clip from <c>Resources/Music</c> by file name (case-insensitive).</summary>
    public bool PlayMusicNamed(string clipName, bool loop = true)
    {
        EnsurePlaylist();
        if (playlist.TryGetValue(clipName.ToLowerInvariant(), out AudioClip clip))
        {
            PlayMusic(clip, loop);
            return true;
        }
        return false;
    }

    /// <summary>All music clip names found under <c>Resources/Music</c>.</summary>
    public List<string> PlaylistNames()
    {
        EnsurePlaylist();
        return new List<string>(playlist.Keys);
    }

    private void EnsurePlaylist()
    {
        if (playlist != null) return;
        playlist = new Dictionary<string, AudioClip>();
        foreach (AudioClip clip in Resources.LoadAll<AudioClip>("Music"))
            if (clip != null) playlist[clip.name.ToLowerInvariant()] = clip;
    }

    // ── Effects ───────────────────────────────────────────────────────────────
    public void PlaySfx(AudioClip clip)
    {
        if (clip != null && sfx != null) sfx.PlayOneShot(clip, GameSettings.EffectsVolume);
    }

    /// <summary>Play a synthesised instrument tone (used by the music mini-game).</summary>
    public void PlayTone(string instrument, float seconds = 1.4f)
    {
        AudioClip clip = GetTone(instrument, seconds);
        if (clip != null && sfx != null) sfx.PlayOneShot(clip, GameSettings.EffectsVolume);
    }

    // ── Procedural instrument tones ───────────────────────────────────────────
    // Each instrument is a base pitch plus a harmonic mix, giving it a distinct
    // timbre without needing any recorded samples.
    private struct Voice { public float freq; public float[] harmonics; public float vibrato; }

    private static Voice VoiceFor(string instrument)
    {
        switch (instrument.ToLowerInvariant())
        {
            case "nai":        // pan flute — breathy, near-pure with light vibrato
            case "pan flute":
                return new Voice { freq = 587f, harmonics = new[] { 1f, 0.12f, 0.06f }, vibrato = 5f };
            case "vioara":     // violin — rich odd/even harmonics
            case "violin":
                return new Voice { freq = 440f, harmonics = new[] { 1f, 0.6f, 0.4f, 0.25f, 0.15f }, vibrato = 6f };
            case "tambal":     // cimbalom — bright, metallic, fast decay
            case "cimbalom":
                return new Voice { freq = 523f, harmonics = new[] { 1f, 0.8f, 0.7f, 0.5f, 0.35f, 0.2f }, vibrato = 0f };
            case "cobza":      // lute — plucked, warm
            case "lute":
                return new Voice { freq = 330f, harmonics = new[] { 1f, 0.5f, 0.25f, 0.1f }, vibrato = 0f };
            case "cimpoi":     // bagpipe — buzzy, strong upper harmonics, drone
            case "bagpipe":
                return new Voice { freq = 392f, harmonics = new[] { 1f, 0.9f, 0.8f, 0.6f, 0.5f, 0.4f, 0.3f }, vibrato = 0f };
            case "fluier":     // flute — pure with a touch of 2nd harmonic
            case "flute":
                return new Voice { freq = 698f, harmonics = new[] { 1f, 0.2f }, vibrato = 4f };
            default:
                return new Voice { freq = 440f, harmonics = new[] { 1f, 0.3f, 0.15f }, vibrato = 4f };
        }
    }

    public AudioClip GetTone(string instrument, float seconds)
    {
        string key = instrument.ToLowerInvariant() + "@" + seconds.ToString("0.0");
        if (toneCache.TryGetValue(key, out AudioClip cached)) return cached;

        Voice v = VoiceFor(instrument);
        const int sampleRate = 44100;
        int samples = Mathf.Max(1, (int)(sampleRate * seconds));
        float[] data = new float[samples];

        // Normalise harmonic gain so mixes don't clip.
        float gain = 0f;
        foreach (float h in v.harmonics) gain += h;
        gain = 0.6f / Mathf.Max(0.0001f, gain);

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float vib = v.vibrato > 0f ? 1f + 0.004f * Mathf.Sin(2f * Mathf.PI * v.vibrato * t) : 1f;

            float sample = 0f;
            for (int h = 0; h < v.harmonics.Length; h++)
                sample += v.harmonics[h] * Mathf.Sin(2f * Mathf.PI * v.freq * (h + 1) * t * vib);

            data[i] = sample * gain * Envelope(t, seconds);
        }

        AudioClip clip = AudioClip.Create("tone_" + key, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        toneCache[key] = clip;
        return clip;
    }

    // Soft attack + gentle exponential decay so tones don't click.
    private static float Envelope(float t, float total)
    {
        const float attack = 0.04f;
        float a = t < attack ? t / attack : 1f;
        float release = Mathf.Exp(-3f * (t / Mathf.Max(0.01f, total)));
        return a * release;
    }
}
