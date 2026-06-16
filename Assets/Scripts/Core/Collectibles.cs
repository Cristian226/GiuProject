using System.Collections.Generic;
using UnityEngine;

public class Collectible
{
    public string id;
    public string missionId;
    public string displayName;
    public string description;
    public string glyph;
    public Color color;

    public Collectible(string id, string missionId, string displayName, string glyph, Color color, string description)
    {
        this.id = id;
        this.missionId = missionId;
        this.displayName = displayName;
        this.glyph = glyph;
        this.color = color;
        this.description = description;
    }
}

// The fixed catalog of reward collectibles, one per learning category. The inventory, reward messages and final-quest unlock all read from here.
public static class Catalog
{
    public static readonly List<Collectible> All = new List<Collectible>
    {
        new Collectible("dictionary", "cuisine", "Dictionary", "Dict", new Color(0.30f, 0.55f, 0.95f),
            "A Romanian dictionary — proof you can name the dishes and words you met in the kitchen."),
        new Collectible("compass", "geography", "Compass", "N", new Color(0.25f, 0.75f, 0.55f),
            "A traveller's compass — earned for mapping Romania's regions, mountains and rivers."),
        new Collectible("scroll", "history", "Ancient Scroll", "Scr", new Color(0.80f, 0.62f, 0.22f),
            "An ancient scroll — earned for placing Romania's history in order, from Dacia to 1989."),
        new Collectible("note", "music", "Musical Note", "Mus", new Color(0.66f, 0.42f, 0.90f),
            "A golden musical note — earned for knowing the doina, the hora and Romania's great composers."),
    };

    public static Collectible ForMission(string missionId)
    {
        foreach (Collectible c in All)
            if (c.missionId == missionId) return c;
        return null;
    }

    public static readonly HashSet<string> RequiredMissionIds = new HashSet<string>
    {
        "cuisine", "geography", "history", "music",
    };

    public static List<Collectible> Required()
    {
        var list = new List<Collectible>();
        foreach (Collectible c in All)
            if (RequiredMissionIds.Contains(c.missionId)) list.Add(c);
        return list;
    }
}
