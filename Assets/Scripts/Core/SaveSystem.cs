using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// The on-disk shape of a saved game. Plain serializable fields only, so Unity's
/// <see cref="JsonUtility"/> can round-trip it to a file. Lists hold ids (mission
/// ids, collectible ids, unlocked-area ids) rather than object references.
/// </summary>
[Serializable]
public class SaveData
{
    public int saveVersion = 1;
    public string savedAtIso;            // ISO-8601 timestamp of the last write

    public List<string> completedMissions = new List<string>();
    public List<string> collectedItems = new List<string>();   // collectible ids
    public List<string> unlockedAreas = new List<string>();

    public bool finalQuestUnlocked;
    public bool finalQuestComplete;

    public int xp;   // experience points earned from completing missions

    // Where the player stood on the island (only meaningful when hasPlayerPos).
    public bool hasPlayerPos;
    public float playerX, playerY, playerZ, playerYaw;
}

/// <summary>
/// Reads and writes the single save slot as JSON under
/// <c>Application.persistentDataPath</c>. Pure file IO — no game state lives here;
/// the live progression is held by <see cref="GameProgress"/>, which uses this.
/// </summary>
public static class SaveSystem
{
    private const string FileName = "primii-pasi-save.json";

    private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    /// <summary>True if a saved game exists on disk (drives the Continue button).</summary>
    public static bool HasSave() => File.Exists(FilePath);

    public static void Save(SaveData data)
    {
        if (data == null) return;
        data.savedAtIso = DateTime.Now.ToString("o");
        try
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
            Debug.Log($"[SaveSystem] Saved to {FilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Save failed: {e.Message}");
        }
    }

    /// <summary>Load the save, or null if none exists / it is unreadable.</summary>
    public static SaveData Load()
    {
        if (!HasSave()) return null;
        try
        {
            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
            return data ?? null;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Load failed: {e.Message}");
            return null;
        }
    }

    public static void Delete()
    {
        try { if (HasSave()) File.Delete(FilePath); }
        catch (Exception e) { Debug.LogError($"[SaveSystem] Delete failed: {e.Message}"); }
    }

    /// <summary>Human-friendly timestamp of the existing save (for the menu), or null.</summary>
    public static string SavedAtDisplay()
    {
        SaveData data = Load();
        if (data == null || string.IsNullOrEmpty(data.savedAtIso)) return null;
        return DateTime.TryParse(data.savedAtIso, out DateTime dt)
            ? dt.ToString("dd MMM yyyy, HH:mm")
            : data.savedAtIso;
    }
}
