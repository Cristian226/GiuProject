using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The rebindable player actions. Movement is split into four directions so each
/// can be rebound independently (the controller turns them back into an axis).
/// Named <c>GameAction</c> rather than "InputAction" to avoid clashing with the new
/// Input System package type.
/// </summary>
public enum GameAction
{
    MoveForward, MoveBack, MoveLeft, MoveRight,
    Sprint, Jump, Interact, Inventory, Pause
}

/// <summary>
/// Central, persistent player settings: control bindings, mouse, audio, graphics.
/// Values are cached in static fields (so per-frame reads in the movement code are
/// free) and persisted to <see cref="PlayerPrefs"/>. Loaded once at startup and
/// re-applied whenever changed; <see cref="Changed"/> lets live systems react.
/// </summary>
public static class GameSettings
{
    /// <summary>Raised after any setting changes (and after load / reset).</summary>
    public static event Action Changed;

    // ── Cached values (read directly by gameplay code) ────────────────────────
    public static float MouseSensitivity = 2f;
    public static bool  InvertY          = false;
    public static float MasterVolume     = 1f;
    public static float MusicVolume      = 0.8f;
    public static float EffectsVolume    = 1f;
    public static int   QualityLevel     = 2;
    public static bool  ShowFps          = false;

    private static readonly Dictionary<GameAction, KeyCode> Binds = new Dictionary<GameAction, KeyCode>();

    private static readonly Dictionary<GameAction, KeyCode> Defaults = new Dictionary<GameAction, KeyCode>
    {
        { GameAction.MoveForward, KeyCode.W },
        { GameAction.MoveBack,    KeyCode.S },
        { GameAction.MoveLeft,    KeyCode.A },
        { GameAction.MoveRight,   KeyCode.D },
        { GameAction.Sprint,      KeyCode.LeftShift },
        { GameAction.Jump,        KeyCode.Space },
        { GameAction.Interact,    KeyCode.E },
        { GameAction.Inventory,   KeyCode.I },
        { GameAction.Pause,       KeyCode.Escape },
    };

    private static bool loaded;

    private const string P = "pp_";   // PlayerPrefs key prefix

    // Load the very first time anything is read, even before Boot runs.
    public static void EnsureLoaded() { if (!loaded) Load(); }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot() => EnsureLoaded();

    // ── Bindings ──────────────────────────────────────────────────────────────
    public static KeyCode GetKey(GameAction action)
    {
        EnsureLoaded();
        return Binds.TryGetValue(action, out KeyCode k) ? k : Defaults[action];
    }

    public static bool Held(GameAction action)    => Input.GetKey(GetKey(action));
    public static bool Pressed(GameAction action) => Input.GetKeyDown(GetKey(action));

    public static void SetKey(GameAction action, KeyCode key)
    {
        EnsureLoaded();
        Binds[action] = key;
        PlayerPrefs.SetInt(P + "bind_" + action, (int)key);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    public static string KeyLabel(GameAction action)
    {
        KeyCode k = GetKey(action);
        switch (k)
        {
            case KeyCode.LeftShift:  return "L-Shift";
            case KeyCode.RightShift: return "R-Shift";
            case KeyCode.Space:      return "Space";
            case KeyCode.Return:     return "Enter";
            case KeyCode.Escape:     return "Esc";
            case KeyCode.Mouse0:     return "Mouse L";
            case KeyCode.Mouse1:     return "Mouse R";
            default:                 return k.ToString();
        }
    }

    // ── Setters for the sliders / toggles (persist immediately) ───────────────
    public static void SetMouseSensitivity(float v) { MouseSensitivity = Mathf.Clamp(v, 0.2f, 10f); PlayerPrefs.SetFloat(P + "sens", MouseSensitivity); Persist(); }
    public static void SetInvertY(bool v)           { InvertY = v; PlayerPrefs.SetInt(P + "invY", v ? 1 : 0); Persist(); }
    public static void SetMasterVolume(float v)     { MasterVolume = Mathf.Clamp01(v); PlayerPrefs.SetFloat(P + "volM", MasterVolume); Persist(); }
    public static void SetMusicVolume(float v)      { MusicVolume = Mathf.Clamp01(v); PlayerPrefs.SetFloat(P + "volMus", MusicVolume); Persist(); }
    public static void SetEffectsVolume(float v)    { EffectsVolume = Mathf.Clamp01(v); PlayerPrefs.SetFloat(P + "volFx", EffectsVolume); Persist(); }
    public static void SetShowFps(bool v)           { ShowFps = v; PlayerPrefs.SetInt(P + "fps", v ? 1 : 0); Persist(); }

    public static void SetQualityLevel(int level)
    {
        QualityLevel = Mathf.Clamp(level, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
        PlayerPrefs.SetInt(P + "qual", QualityLevel);
        QualitySettings.SetQualityLevel(QualityLevel, true);
        Persist();
    }

    // ── Load / save / reset / apply ───────────────────────────────────────────
    public static void Load()
    {
        loaded = true;   // set first so EnsureLoaded() can't recurse

        Binds.Clear();
        foreach (var kv in Defaults)
            Binds[kv.Key] = (KeyCode)PlayerPrefs.GetInt(P + "bind_" + kv.Key, (int)kv.Value);

        MouseSensitivity = PlayerPrefs.GetFloat(P + "sens", 2f);
        InvertY          = PlayerPrefs.GetInt(P + "invY", 0) == 1;
        MasterVolume     = PlayerPrefs.GetFloat(P + "volM", 1f);
        MusicVolume      = PlayerPrefs.GetFloat(P + "volMus", 0.8f);
        EffectsVolume    = PlayerPrefs.GetFloat(P + "volFx", 1f);
        ShowFps          = PlayerPrefs.GetInt(P + "fps", 0) == 1;
        QualityLevel     = PlayerPrefs.GetInt(P + "qual", QualitySettings.GetQualityLevel());

        Apply();
        Changed?.Invoke();
    }

    private static void Persist()
    {
        PlayerPrefs.Save();
        Apply();
        Changed?.Invoke();
    }

    /// <summary>Push settings that affect engine state (quality) live.</summary>
    public static void Apply()
    {
        QualityLevel = Mathf.Clamp(QualityLevel, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
        QualitySettings.SetQualityLevel(QualityLevel, true);
        // Audio volumes are applied by AudioManager (it listens to Changed).
    }

    /// <summary>Restore every setting to its default and persist the result.</summary>
    public static void ResetToDefaults()
    {
        foreach (var kv in Defaults)
        {
            Binds[kv.Key] = kv.Value;
            PlayerPrefs.SetInt(P + "bind_" + kv.Key, (int)kv.Value);
        }

        MouseSensitivity = 2f;
        InvertY          = false;
        MasterVolume     = 1f;
        MusicVolume      = 0.8f;
        EffectsVolume    = 1f;
        ShowFps          = false;
        QualityLevel     = Mathf.Clamp(2, 0, Mathf.Max(0, QualitySettings.names.Length - 1));

        PlayerPrefs.SetFloat(P + "sens", MouseSensitivity);
        PlayerPrefs.SetInt(P + "invY", 0);
        PlayerPrefs.SetFloat(P + "volM", MasterVolume);
        PlayerPrefs.SetFloat(P + "volMus", MusicVolume);
        PlayerPrefs.SetFloat(P + "volFx", EffectsVolume);
        PlayerPrefs.SetInt(P + "fps", 0);
        PlayerPrefs.SetInt(P + "qual", QualityLevel);
        PlayerPrefs.Save();

        Apply();
        Changed?.Invoke();
    }
}
