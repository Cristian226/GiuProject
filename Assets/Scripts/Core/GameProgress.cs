using System.Collections.Generic;
using UnityEngine;

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance { get; private set; }
    private SaveData data = new SaveData();
    public bool FinalQuestUnlocked => data.finalQuestUnlocked;
    public bool FinalQuestComplete => data.finalQuestComplete;
    public const int XpPerLevel = 250;
    public int Xp => data.xp;
    public int Level => data.xp / XpPerLevel + 1;
    public int CollectedCount => data.collectedItems.Count;

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

    public void NewGame()
    {
        data = new SaveData();
        SaveSystem.Delete();
    }

    public void LoadFromDisk()
    {
        data = SaveSystem.Load() ?? new SaveData();
    }

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

    public bool IsMissionComplete(string missionId) =>
        !string.IsNullOrEmpty(missionId) && data.completedMissions.Contains(missionId);

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

        Save();
        return firstTime;
    }

    public bool HasItem(string collectibleId) => data.collectedItems.Contains(collectibleId);

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

    public bool AllCollected()
    {
        foreach (Collectible c in Catalog.Required())
            if (!HasItem(c.id)) return false;
        return Catalog.Required().Count > 0;
    }

    private void RefreshFinalQuestUnlock()
    {
        if (!data.finalQuestUnlocked && AllCollected())
            data.finalQuestUnlocked = true;
    }

    public void CompleteFinalQuest()
    {
        data.finalQuestComplete = true;
        Save();
    }

    public int AddXp(int amount)
    {
        if (amount <= 0) return data.xp;
        data.xp += amount;
        Save();
        return data.xp;
    }

    public bool TryGetSavedPlayerPose(out Vector3 position, out Quaternion rotation)
    {
        position = new Vector3(data.playerX, data.playerY, data.playerZ);
        rotation = Quaternion.Euler(0f, data.playerYaw, 0f);
        return data.hasPlayerPos;
    }
}
