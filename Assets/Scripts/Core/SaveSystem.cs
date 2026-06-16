using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int saveVersion = 1;
    public string savedAtIso;
    public List<string> completedMissions = new List<string>();
    public List<string> collectedItems = new List<string>();
    public bool finalQuestUnlocked;
    public bool finalQuestComplete;
    public int xp;
    public bool hasPlayerPos;
    public float playerX, playerY, playerZ, playerYaw;
}

public static class SaveSystem
{
    private const string FileName = "primii-pasi-save.json";
    private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);
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
    
    public static SaveData Load()
    {
        if (!HasSave()) return null;
        try
        {
            return JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
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

    public static string SavedAtDisplay()
    {
        SaveData data = Load();
        if (data == null || string.IsNullOrEmpty(data.savedAtIso)) return null;
        return DateTime.TryParse(data.savedAtIso, out DateTime dt)
            ? dt.ToString("dd MMM yyyy, HH:mm")
            : data.savedAtIso;
    }
}
