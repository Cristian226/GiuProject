using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One reward collectible: the trophy a player earns for finishing a mission.
/// <see cref="missionId"/> ties it to a mission (the same id used by
/// <see cref="MissionGiver"/> and <see cref="GameFlowManager"/>). <see cref="glyph"/>
/// is a short text token shown on the inventory tile and <see cref="color"/> its
/// accent — so each item has a visual identity without needing any sprite assets
/// (the default font has no emoji, so plain text tokens are used on purpose).
/// </summary>
public class Collectible
{
    public string id;          // stable id stored in the save file
    public string missionId;   // the mission that awards it
    public string displayName;
    public string description;
    public string glyph;       // short token shown in the inventory tile
    public Color color;        // accent colour for the tile

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

/// <summary>
/// The fixed catalog of reward collectibles. One per learning category. The
/// inventory, reward messages and the final-quest unlock all read from here, so the
/// progression loop is defined in a single place.
///
/// <para>Add a new mission by adding a <see cref="Collectible"/> with a matching
/// mission id; the reward, inventory slot and completion maths update for free.</para>
/// </summary>
public static class Catalog
{
    public static readonly List<Collectible> All = new List<Collectible>
    {
        new Collectible("dictionary", "cuisine",       "Dicționar",        "DEX", new Color(0.30f, 0.55f, 0.95f),
            "A Romanian dictionary — proof you can name the dishes and words you met in the kitchen."),
        new Collectible("compass",    "geography",     "Busolă",           "N",   new Color(0.25f, 0.75f, 0.55f),
            "A traveller's compass — earned for mapping Romania's regions, mountains and rivers."),
        new Collectible("scroll",     "history",       "Hrisov Vechi",     "Hr",  new Color(0.80f, 0.62f, 0.22f),
            "An ancient scroll — earned for placing Romania's history in order, from Dacia to 1989."),
        new Collectible("note",       "music",         "Notă Muzicală",    "Mz",  new Color(0.66f, 0.42f, 0.90f),
            "A golden musical note — earned for knowing the doina, the hora and Romania's great composers."),
        new Collectible("ticket",     "transport",     "Bilet de Tren",    "CFR", new Color(0.90f, 0.55f, 0.20f),
            "A train ticket — earned for reading timetables and choosing the right train."),
        new Collectible("badge",      "accessibility", "Insignă de Acces", "Ac",  new Color(0.30f, 0.78f, 0.85f),
            "An accessibility badge — earned for making the city welcoming to everyone."),
    };

    /// <summary>The collectible a mission awards, or null if that mission has none.</summary>
    public static Collectible ForMission(string missionId)
    {
        foreach (Collectible c in All)
            if (c.missionId == missionId) return c;
        return null;
    }

    public static Collectible ById(string id)
    {
        foreach (Collectible c in All)
            if (c.id == id) return c;
        return null;
    }

    /// <summary>
    /// The collectibles required to unlock the final quest. Only the missions that
    /// actually exist in the build count, so the game stays winnable while extra
    /// categories (transport, accessibility) are still being authored.
    /// </summary>
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
