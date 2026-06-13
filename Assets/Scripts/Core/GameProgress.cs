using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The live game progression: which missions are done, which collectibles the
/// player owns, which areas are unlocked, and the final-quest state. A persistent,
/// auto-created singleton (like <see cref="GameFlowManager"/>) and the single source
/// of truth that <see cref="GameFlowManager"/>, the inventory UI and the main menu
/// all read from.
///
/// <para>It starts empty (a fresh session). The main menu chooses what happens next:
/// <see cref="NewGame"/> wipes the slot, <see cref="LoadFromDisk"/> continues a save.
/// Completing a mission awards its collectible and auto-saves.</para>
/// </summary>
public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance { get; private set; }

    /// <summary>Raised whenever progression changes (mission done, item earned, …).</summary>
    public event Action Changed;

    private SaveData data = new SaveData();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance == null)
            new GameObject("GameProgress (auto)").AddComponent<GameProgress>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Session setup (called by the main menu) ───────────────────────────────
    /// <summary>Begin a brand-new game: clears memory and the save file.</summary>
    public void NewGame()
    {
        data = new SaveData();
        SaveSystem.Delete();
        Changed?.Invoke();
    }

    /// <summary>Continue: load the saved slot into memory (no-op if none exists).</summary>
    public void LoadFromDisk()
    {
        data = SaveSystem.Load() ?? new SaveData();
        Changed?.Invoke();
    }

    /// <summary>Persist the current progression. Pass a player to also store its spot.</summary>
    public void Save(Transform player = null)
    {
        if (player != null)
        {
            data.hasPlayerPos = true;
            data.playerX = player.position.x;
            data.playerY = player.position.y;
            data.playerZ = player.position.z;
            data.playerYaw = player.eulerAngles.y;
        }
        SaveSystem.Save(data);
    }

    // ── Missions ──────────────────────────────────────────────────────────────
    public bool IsMissionComplete(string missionId) =>
        !string.IsNullOrEmpty(missionId) && data.completedMissions.Contains(missionId);

    /// <summary>
    /// Record a mission as complete, award its collectible (if any) and auto-save.
    /// Returns true if this was the first time it was completed.
    /// </summary>
    public bool CompleteMission(string missionId)
    {
        if (string.IsNullOrEmpty(missionId)) return false;

        bool firstTime = !data.completedMissions.Contains(missionId);
        if (firstTime)
        {
            data.completedMissions.Add(missionId);

            Collectible reward = Catalog.ForMission(missionId);
            if (reward != null && !data.collectedItems.Contains(reward.id))
                data.collectedItems.Add(reward.id);

            RefreshFinalQuestUnlock();
        }

        Save();                 // auto-save after every completion
        Changed?.Invoke();
        return firstTime;
    }

    public int CompletedMissionCount => data.completedMissions.Count;

    // ── Inventory / collectibles ──────────────────────────────────────────────
    public bool HasItem(string collectibleId) => data.collectedItems.Contains(collectibleId);

    public int CollectedCount => data.collectedItems.Count;

    /// <summary>How far through the required collection the player is, 0..1.</summary>
    public float CompletionFraction()
    {
        List<Collectible> required = Catalog.Required();
        if (required.Count == 0) return 0f;
        int have = 0;
        foreach (Collectible c in required)
            if (HasItem(c.id)) have++;
        return (float)have / required.Count;
    }

    public int CompletionPercent() => Mathf.RoundToInt(CompletionFraction() * 100f);

    /// <summary>True once every required collectible is owned.</summary>
    public bool AllCollected()
    {
        foreach (Collectible c in Catalog.Required())
            if (!HasItem(c.id)) return false;
        return Catalog.Required().Count > 0;
    }

    // ── Unlockable areas ──────────────────────────────────────────────────────
    public bool IsAreaUnlocked(string areaId) => data.unlockedAreas.Contains(areaId);

    public void UnlockArea(string areaId)
    {
        if (string.IsNullOrEmpty(areaId) || data.unlockedAreas.Contains(areaId)) return;
        data.unlockedAreas.Add(areaId);
        Save();
        Changed?.Invoke();
    }

    // ── Final quest ───────────────────────────────────────────────────────────
    public bool FinalQuestUnlocked => data.finalQuestUnlocked;
    public bool FinalQuestComplete => data.finalQuestComplete;

    private void RefreshFinalQuestUnlock()
    {
        if (!data.finalQuestUnlocked && AllCollected())
            data.finalQuestUnlocked = true;   // toast handled by the caller (GameFlowManager)
    }

    public void CompleteFinalQuest()
    {
        data.finalQuestComplete = true;
        Save();
        Changed?.Invoke();
    }

    // ── Restore the player's saved spot (used by Continue) ─────────────────────
    public bool TryGetSavedPlayerPose(out Vector3 position, out Quaternion rotation)
    {
        position = new Vector3(data.playerX, data.playerY, data.playerZ);
        rotation = Quaternion.Euler(0f, data.playerYaw, 0f);
        return data.hasPlayerPos;
    }
}
