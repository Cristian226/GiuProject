using System.IO;
using NUnit.Framework;
using UnityEngine;

// GameProgress.Save()/NewGame() write the real save file on disk, so each test
// backs it up and restores it afterward to avoid touching the player's actual save.
public class GameProgressTests
{
    private static readonly string SavePath =
        Path.Combine(Application.persistentDataPath, "primii-pasi-save.json");

    private string backup;
    private bool hadBackup;
    private GameObject go;
    private GameProgress progress;

    [SetUp]
    public void SetUp()
    {
        hadBackup = File.Exists(SavePath);
        if (hadBackup) backup = File.ReadAllText(SavePath);

        go = new GameObject("GameProgressTest");
        progress = go.AddComponent<GameProgress>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);

        if (hadBackup) File.WriteAllText(SavePath, backup);
        else if (File.Exists(SavePath)) File.Delete(SavePath);
    }

    [Test]
    public void CompleteMission_FirstTime_ReturnsTrueAndAwardsCollectible()
    {
        bool firstTime = progress.CompleteMission("geography");

        Assert.IsTrue(firstTime);
        Assert.IsTrue(progress.IsMissionComplete("geography"));
        Assert.IsTrue(progress.HasItem("compass"));
    }

    [Test]
    public void CompleteMission_SecondTime_ReturnsFalse()
    {
        progress.CompleteMission("geography");
        bool secondTime = progress.CompleteMission("geography");

        Assert.IsFalse(secondTime);
    }

    [Test]
    public void CompleteMission_EmptyId_ReturnsFalse()
    {
        Assert.IsFalse(progress.CompleteMission(""));
        Assert.IsFalse(progress.CompleteMission(null));
    }

    [Test]
    public void AddXp_IncreasesXpAndLevel()
    {
        progress.AddXp(GameProgress.XpPerLevel);

        Assert.AreEqual(GameProgress.XpPerLevel, progress.Xp);
        Assert.AreEqual(2, progress.Level);
    }

    [Test]
    public void AddXp_NonPositiveAmount_DoesNothing()
    {
        int before = progress.Xp;
        progress.AddXp(0);
        progress.AddXp(-10);

        Assert.AreEqual(before, progress.Xp);
    }

    [Test]
    public void AllCollected_FalseUntilEveryRequiredMissionDone()
    {
        Assert.IsFalse(progress.AllCollected());

        foreach (string missionId in Catalog.RequiredMissionIds)
            progress.CompleteMission(missionId);

        Assert.IsTrue(progress.AllCollected());
        Assert.IsTrue(progress.FinalQuestUnlocked);
    }

    [Test]
    public void CompletionFraction_ReflectsPartialProgress()
    {
        progress.CompleteMission("geography");

        float fraction = progress.CompletionFraction();

        Assert.Greater(fraction, 0f);
        Assert.Less(fraction, 1f);
    }

    [Test]
    public void NewGame_ResetsProgress()
    {
        progress.CompleteMission("geography");
        progress.AddXp(100);

        progress.NewGame();

        Assert.IsFalse(progress.IsMissionComplete("geography"));
        Assert.AreEqual(0, progress.Xp);
    }
}
