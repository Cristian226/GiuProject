using System.IO;
using NUnit.Framework;
using UnityEngine;

// SaveSystem reads/writes the real save file on disk, so each test backs it up
// and restores it afterward to avoid touching the player's actual save.
public class SaveSystemTests
{
    private static readonly string SavePath =
        Path.Combine(Application.persistentDataPath, "primii-pasi-save.json");

    private string backup;
    private bool hadBackup;

    [SetUp]
    public void SetUp()
    {
        hadBackup = File.Exists(SavePath);
        if (hadBackup) backup = File.ReadAllText(SavePath);
    }

    [TearDown]
    public void TearDown()
    {
        if (hadBackup) File.WriteAllText(SavePath, backup);
        else if (File.Exists(SavePath)) File.Delete(SavePath);
    }

    [Test]
    public void Save_ThenLoad_RoundTripsData()
    {
        var data = new SaveData { xp = 250, finalQuestUnlocked = true };
        data.completedMissions.Add("geography");

        SaveSystem.Save(data);
        SaveData loaded = SaveSystem.Load();

        Assert.IsNotNull(loaded);
        Assert.AreEqual(250, loaded.xp);
        Assert.IsTrue(loaded.finalQuestUnlocked);
        Assert.Contains("geography", loaded.completedMissions);
    }

    [Test]
    public void HasSave_TrueAfterSave_FalseAfterDelete()
    {
        SaveSystem.Save(new SaveData());
        Assert.IsTrue(SaveSystem.HasSave());

        SaveSystem.Delete();
        Assert.IsFalse(SaveSystem.HasSave());
    }

    [Test]
    public void Load_WhenNoSaveExists_ReturnsNull()
    {
        SaveSystem.Delete();

        Assert.IsNull(SaveSystem.Load());
    }
}
