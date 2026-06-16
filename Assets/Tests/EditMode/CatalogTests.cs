using System.Collections.Generic;
using NUnit.Framework;

public class CatalogTests
{
    [Test]
    public void ForMission_ReturnsMatchingCollectible()
    {
        Collectible result = Catalog.ForMission("geography");

        Assert.IsNotNull(result);
        Assert.AreEqual("compass", result.id);
    }

    [Test]
    public void ForMission_UnknownId_ReturnsNull()
    {
        Collectible result = Catalog.ForMission("not-a-real-mission");

        Assert.IsNull(result);
    }

    [Test]
    public void Required_OnlyContainsRequiredMissionIds()
    {
        List<Collectible> required = Catalog.Required();

        Assert.IsTrue(required.Count > 0);
        foreach (Collectible c in required)
            Assert.IsTrue(Catalog.RequiredMissionIds.Contains(c.missionId));
    }

    [Test]
    public void Required_DoesNotContainNonRequiredMissions()
    {
        List<Collectible> required = Catalog.Required();

        foreach (Collectible c in required)
            Assert.AreNotEqual("transport", c.missionId);
    }
}
