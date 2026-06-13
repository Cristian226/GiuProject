using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor generators for the polished, themed environments and app scenes —
/// a sibling of the original <see cref="MissionSceneBuilder"/> (which is left
/// untouched so the colleague's <b>kitchen</b> scene is never overwritten).
///
/// Builds: the Train Station (Geography), the Castle Museum (History), the Theatre
/// (Music), the Main Menu scene, the Finale scene, and fits the world boundary onto
/// whatever scene is open. Everything is constructed from primitives + embedded
/// materials, so no art import is required.
///
/// Menu: <b>Tools ▸ Romania Game ▸ …</b>
/// </summary>
public static class RomaniaGameTools
{
    private const string ScenesFolder = "Assets/Scenes";

    // =====================================================================
    //  Menu entry points
    // =====================================================================
    [MenuItem("Tools/Romania Game/Build EVERYTHING (Menu + Environments + Finale)")]
    public static void BuildEverything()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        BuildMenuSceneInternal();
        BuildTrainStationInternal();
        BuildCastleMuseumInternal();
        BuildTheatreInternal();
        BuildFinaleInternal();
        RegisterScenes();
        EditorUtility.DisplayDialog("Romania Game",
            "Built the Menu, Train Station (Geography), Castle Museum (History), Theatre (Music) " +
            "and Finale scenes, and registered all scenes in Build Settings.\n\n" +
            "The kitchen (Cuisine) scene was left untouched.\n\n" +
            "Open MainScene and run Tools ▸ Romania Game ▸ World ▸ Add / Fit Boundary to fence the city.",
            "Great");
    }

    [MenuItem("Tools/Romania Game/Environments/Build Train Station (Geography)")]
    public static void BuildTrainStation()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        BuildTrainStationInternal(); RegisterScenes();
    }

    [MenuItem("Tools/Romania Game/Environments/Build Castle Museum (History)")]
    public static void BuildCastleMuseum()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        BuildCastleMuseumInternal(); RegisterScenes();
    }

    [MenuItem("Tools/Romania Game/Environments/Build Theatre (Music)")]
    public static void BuildTheatre()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        BuildTheatreInternal(); RegisterScenes();
    }

    [MenuItem("Tools/Romania Game/Environments/Build ALL Themed Environments")]
    public static void BuildAllEnvironments()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        BuildTrainStationInternal();
        BuildCastleMuseumInternal();
        BuildTheatreInternal();
        RegisterScenes();
    }

    [MenuItem("Tools/Romania Game/App/Build Menu Scene")]
    public static void BuildMenuScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        BuildMenuSceneInternal(); RegisterScenes();
    }

    [MenuItem("Tools/Romania Game/App/Build Finale Scene")]
    public static void BuildFinale()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        BuildFinaleInternal(); RegisterScenes();
    }

    [MenuItem("Tools/Romania Game/World/Add or Fit Boundary In Open Scene")]
    public static void FitBoundary()
    {
        WorldBounds wb = UnityEngine.Object.FindObjectOfType<WorldBounds>();
        if (wb == null)
            wb = new GameObject("WorldBoundary").AddComponent<WorldBounds>();

        if (TryComputeSceneBounds(out Bounds b))
        {
            wb.areaCenter = new Vector3(b.center.x, b.min.y, b.center.z);
            wb.areaSize = new Vector3(b.size.x * 1.06f + 4f, Mathf.Max(24f, b.size.y + 12f), b.size.z * 1.06f + 4f);
            wb.wallHeight = wb.areaSize.y;
            wb.fallY = b.min.y - 8f;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Romania Game",
            $"World boundary fitted to the open scene.\nCenter {wb.areaCenter}, size {wb.areaSize}.\n\n" +
            "Tweak the box in the Inspector if needed, then save the scene.", "OK");
    }

    [MenuItem("Tools/Romania Game/World/Place Final Quest On little_boy_B (open MainScene first)")]
    public static void PlaceFinaleGiver()
    {
        GameObject npc = FindInOpenScenes("little_boy_B");
        if (npc == null)
        {
            EditorUtility.DisplayDialog("Romania Game",
                "Couldn't find a GameObject named 'little_boy_B' in the open scene(s).\n\n" +
                "Open MainScene (the city) first, then run this again.", "OK");
            return;
        }

        // The final quest should live on exactly one NPC: remove any stray giver elsewhere.
        foreach (FinaleGiver stray in UnityEngine.Object.FindObjectsOfType<FinaleGiver>())
            if (stray.gameObject != npc) UnityEngine.Object.DestroyImmediate(stray);

        // Drop any plain chatter component so the FinaleGiver is the only thing the player triggers.
        NPCInteraction chatter = npc.GetComponent<NPCInteraction>();
        if (chatter != null) UnityEngine.Object.DestroyImmediate(chatter);

        FinaleGiver fg = npc.GetComponent<FinaleGiver>();
        if (fg == null) fg = npc.AddComponent<FinaleGiver>();
        fg.npcName = "Andrei";
        fg.finaleSceneName = "FinaleScene";
        fg.finaleMissionId = "finale";

        EditorUtility.SetDirty(npc);
        EditorSceneManager.MarkSceneDirty(npc.scene);
        EditorUtility.DisplayDialog("Romania Game",
            "The Grand Final Challenge is now on 'little_boy_B'.\n\n" +
            "Until the player finishes all four missions (cuisine, geography, history, music) " +
            "he asks them to come back later; once every treasure is collected he offers the " +
            "final quest.\n\nSave the scene (Ctrl+S) to keep it.", "Great");
    }

    /// <summary>Depth-first search for a GameObject by name across every loaded scene.</summary>
    private static GameObject FindInOpenScenes(string name)
    {
        for (int s = 0; s < SceneManager.sceneCount; s++)
        {
            Scene scene = SceneManager.GetSceneAt(s);
            if (!scene.isLoaded) continue;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform t = FindByName(root.transform, name);
                if (t != null) return t.gameObject;
            }
        }
        return null;
    }

    private static Transform FindByName(Transform t, string name)
    {
        if (string.Equals(t.name, name, StringComparison.OrdinalIgnoreCase)) return t;
        for (int i = 0; i < t.childCount; i++)
        {
            Transform r = FindByName(t.GetChild(i), name);
            if (r != null) return r;
        }
        return null;
    }

    [MenuItem("Tools/Romania Game/App/Register ALL Scenes In Build Settings")]
    public static void RegisterScenes()
    {
        // Order matters: the Menu must be first so the game boots to it.
        string[] ordered =
        {
            "MenuScene", "MainScene", "CuisineScene",
            "GeographyScene", "HistoryScene", "MusicScene", "FinaleScene",
        };

        var list = new List<EditorBuildSettingsScene>();
        foreach (string name in ordered)
        {
            string path = $"{ScenesFolder}/{name}.unity";
            if (File.Exists(path)) list.Add(new EditorBuildSettingsScene(path, true));
        }
        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log($"[Romania Game] Build Settings now has {list.Count} scene(s).");
    }

    // =====================================================================
    //  Train Station  (GeographyScene)
    // =====================================================================
    private static void BuildTrainStationInternal()
    {
        Scene scene = NewScene();
        Lighting(new Color(0.66f, 0.68f, 0.74f), 1.05f, new Vector3(48f, 20f, 0f), new Color(0.55f, 0.7f, 0.95f));

        Transform root = new GameObject("Station").transform;
        Material concrete = Mat(new Color(0.55f, 0.55f, 0.58f));
        Material platform = Mat(new Color(0.72f, 0.70f, 0.64f));
        Material metalDark = Mat(new Color(0.20f, 0.21f, 0.24f), 0.7f, 0.5f);
        Material glassMat = Mat(new Color(0.45f, 0.62f, 0.72f), 0.3f, 0.8f);
        Material wallMat = Mat(new Color(0.78f, 0.74f, 0.66f));
        Material signYellow = Mat(new Color(0.95f, 0.78f, 0.10f), 0f, 0.3f, new Color(0.3f, 0.24f, 0f));
        Material woodMat = Mat(new Color(0.40f, 0.26f, 0.14f));

        Vector3 spawn = new Vector3(0f, 1f, -16f);

        Floor(root, "Concourse", Vector3.zero, new Vector3(4.4f, 1f, 4.4f), concrete);
        // Station hall back wall + side walls
        Box(root, "Wall_N", new Vector3(0, 4, 21), new Vector3(46, 8, 0.6f), wallMat);
        Box(root, "Wall_W", new Vector3(-22, 4, 3), new Vector3(0.6f, 8, 38), wallMat);
        Box(root, "Wall_E", new Vector3(22, 4, 3), new Vector3(0.6f, 8, 38), wallMat);
        Box(root, "Wall_S", new Vector3(0, 4, -19), new Vector3(46, 8, 0.6f), wallMat);

        // Raised platform along the track
        Box(root, "Platform", new Vector3(0, 0.3f, 8.5f), new Vector3(40, 0.6f, 6f), platform);

        // Canopy on pillars over the platform
        Box(root, "Canopy", new Vector3(0, 5.4f, 8.5f), new Vector3(42, 0.4f, 7f), Mat(new Color(0.30f, 0.34f, 0.40f)));
        for (int i = -3; i <= 3; i++)
        {
            Cyl(root, $"Pillar_{i}", new Vector3(i * 6f, 2.85f, 11.4f), new Vector3(0.4f, 2.55f, 0.4f), metalDark);
        }

        // Rails + sleepers in the track zone (z ~ 2)
        Box(root, "RailA", new Vector3(0, 0.15f, 1.4f), new Vector3(44, 0.15f, 0.2f), metalDark);
        Box(root, "RailB", new Vector3(0, 0.15f, 3.0f), new Vector3(44, 0.15f, 0.2f), metalDark);
        Material sleeperMat = Mat(new Color(0.25f, 0.18f, 0.12f));
        for (int i = -10; i <= 10; i++)
            Box(root, $"Sleeper_{i}", new Vector3(i * 2f, 0.08f, 2.2f), new Vector3(0.4f, 0.12f, 2.4f), sleeperMat);

        // ── A detailed electric train in a calm, natural CFR-style livery (deep navy
        //    body, ivory waist band, grey roof). The player looks north, so the side
        //    facing them is the -z face (faceZ). ──
        Material trainBlue  = Mat(new Color(0.12f, 0.23f, 0.40f), 0.30f, 0.45f);  // deep navy steel
        Material trainCream = Mat(new Color(0.91f, 0.89f, 0.82f), 0.08f, 0.35f);  // ivory band
        Material trainRoof  = Mat(new Color(0.42f, 0.45f, 0.49f), 0.45f, 0.45f);  // grey roof
        Material chassis    = Mat(new Color(0.10f, 0.10f, 0.12f), 0.55f, 0.40f);  // near-black underframe
        Material trainYellow= Mat(new Color(0.93f, 0.78f, 0.16f), 0.10f, 0.40f);  // warning ends
        Material trainSteel = Mat(new Color(0.22f, 0.23f, 0.26f), 0.70f, 0.55f);  // pantograph / details
        Material headlight  = Mat(new Color(1f, 0.97f, 0.80f), 0f, 0.9f, new Color(0.85f, 0.78f, 0.45f));
        Material tailLamp   = Mat(new Color(0.85f, 0.12f, 0.10f), 0f, 0.7f, new Color(0.45f, 0.02f, 0.02f));
        Transform train = new GameObject("Train").transform; train.SetParent(root, false);
        const float rz = 2.2f;            // track centre line
        const float faceZ = rz - 1.30f;   // the body face toward the player

        // ── Locomotive (west end) ──
        float lx = -13.5f;
        Box(train, "Loco_Underframe", new Vector3(lx, 0.78f, rz), new Vector3(9.6f, 0.45f, 2.7f), chassis);
        Box(train, "Loco_Body",       new Vector3(lx, 1.85f, rz), new Vector3(9.0f, 1.85f, 2.45f), trainBlue);
        Box(train, "Loco_Skirt",      new Vector3(lx, 1.05f, faceZ + 0.02f), new Vector3(9.0f, 0.35f, 0.04f), chassis);
        Box(train, "Loco_Band",       new Vector3(lx, 1.55f, faceZ + 0.02f), new Vector3(9.02f, 0.32f, 0.04f), trainCream);
        Box(train, "Loco_Roof",       new Vector3(lx, 2.85f, rz), new Vector3(8.7f, 0.4f, 2.35f), trainRoof);
        Box(train, "Loco_RoofStep",   new Vector3(lx + 3.1f, 3.05f, rz), new Vector3(2.6f, 0.25f, 2.3f), trainRoof);
        // Driver windscreen + side windows
        Box(train, "Loco_CabWin",     new Vector3(lx + 4.3f, 2.35f, faceZ + 0.04f), new Vector3(1.7f, 0.85f, 0.05f), glassMat);
        for (int w = 0; w < 3; w++)
            Box(train, $"Loco_Win_{w}", new Vector3(lx - 2.8f + w * 1.9f, 2.15f, faceZ + 0.03f), new Vector3(1.2f, 0.8f, 0.05f), glassMat);
        // Roof grille + pantograph (raised arms toward the catenary)
        Box(train, "Loco_Vent",       new Vector3(lx - 2.2f, 3.08f, rz), new Vector3(3.4f, 0.18f, 1.6f), trainSteel);
        Box(train, "Panto_Base",      new Vector3(lx - 1.2f, 3.12f, rz), new Vector3(1.6f, 0.12f, 1.4f), trainSteel);
        Box(train, "Panto_ArmA",      new Vector3(lx - 1.55f, 3.55f, rz), new Vector3(0.08f, 0.95f, 0.08f), trainSteel, new Vector3(30, 0, 0));
        Box(train, "Panto_ArmB",      new Vector3(lx - 0.85f, 3.55f, rz), new Vector3(0.08f, 0.95f, 0.08f), trainSteel, new Vector3(-30, 0, 0));
        Box(train, "Panto_Bar",       new Vector3(lx - 1.2f, 4.0f, rz), new Vector3(1.7f, 0.08f, 0.12f), trainSteel);
        // Yellow warning nose + lamps + buffers (west end)
        float fx = lx - 4.85f;
        Box(train, "Loco_Nose",       new Vector3(fx, 1.85f, rz), new Vector3(0.4f, 1.85f, 2.45f), trainYellow);
        Box(train, "Head_L",          new Vector3(fx - 0.1f, 2.2f, rz - 0.8f), new Vector3(0.28f, 0.28f, 0.25f), headlight);
        Box(train, "Head_R",          new Vector3(fx - 0.1f, 2.2f, rz + 0.8f), new Vector3(0.28f, 0.28f, 0.25f), headlight);
        Box(train, "Tail_Lamp",       new Vector3(fx - 0.1f, 1.2f, faceZ), new Vector3(0.22f, 0.22f, 0.06f), tailLamp);
        Cyl(train, "Buffer_L",        new Vector3(fx - 0.2f, 1.0f, rz - 0.8f), new Vector3(0.22f, 0.2f, 0.22f), trainSteel, new Vector3(90, 0, 0));
        Cyl(train, "Buffer_R",        new Vector3(fx - 0.2f, 1.0f, rz + 0.8f), new Vector3(0.22f, 0.2f, 0.22f), trainSteel, new Vector3(90, 0, 0));
        Bogie(train, "Loco_BogieA", new Vector3(lx - 2.8f, 0f, rz), faceZ, chassis, trainSteel);
        Bogie(train, "Loco_BogieB", new Vector3(lx + 2.8f, 0f, rz), faceZ, chassis, trainSteel);

        // ── Two passenger carriages (ivory body, navy waist band) ──
        for (int c = 0; c < 2; c++)
        {
            float cx = -3.2f + c * 9.4f;
            Box(train, $"Car{c}_Underframe", new Vector3(cx, 0.78f, rz), new Vector3(8.8f, 0.4f, 2.6f), chassis);
            Box(train, $"Car{c}_Body",       new Vector3(cx, 1.95f, rz), new Vector3(8.6f, 2.05f, 2.4f), trainCream);
            Box(train, $"Car{c}_Band",       new Vector3(cx, 2.55f, faceZ + 0.02f), new Vector3(8.62f, 0.45f, 0.04f), trainBlue);
            Box(train, $"Car{c}_Skirt",      new Vector3(cx, 1.0f, faceZ + 0.02f), new Vector3(8.6f, 0.3f, 0.04f), chassis);
            Box(train, $"Car{c}_Roof",       new Vector3(cx, 3.05f, rz), new Vector3(8.3f, 0.4f, 2.3f), trainRoof);
            Box(train, $"Car{c}_RoofVent",   new Vector3(cx, 3.28f, rz), new Vector3(6.5f, 0.14f, 0.5f), trainSteel);
            for (int w = 0; w < 5; w++)
                Box(train, $"Car{c}_Win_{w}", new Vector3(cx - 3.2f + w * 1.6f, 2.15f, faceZ + 0.03f), new Vector3(1.15f, 0.85f, 0.05f), glassMat);
            Box(train, $"Car{c}_Door0", new Vector3(cx - 3.95f, 1.75f, faceZ + 0.03f), new Vector3(0.55f, 1.85f, 0.05f), trainBlue);
            Box(train, $"Car{c}_Door1", new Vector3(cx + 3.95f, 1.75f, faceZ + 0.03f), new Vector3(0.55f, 1.85f, 0.05f), trainBlue);
            Bogie(train, $"Car{c}_BogieA", new Vector3(cx - 3.0f, 0f, rz), faceZ, chassis, trainSteel);
            Bogie(train, $"Car{c}_BogieB", new Vector3(cx + 3.0f, 0f, rz), faceZ, chassis, trainSteel);
            Box(train, $"Car{c}_Coupler", new Vector3(cx - 4.7f, 0.85f, rz), new Vector3(0.6f, 0.2f, 0.25f), chassis);
        }

        // Departure board
        Sign(root, "DEPARTURE_BOARD",
            "<b>DEPARTURES</b>\nCluj-Napoca     13:40   Pl. 2\nIași            14:05   Pl. 3\nTimișoara       14:20   Pl. 1\nConstanța       14:55   Pl. 4\nBrașov          15:10   Pl. 2",
            new Vector3(-9, 3.6f, 20.4f), spawn, 1.5f, new Color(0.14f, 0.11f, 0.04f), new Vector2(11, 5), TextAlignmentOptions.TopLeft, signYellow);

        // ── Railway network map: a framed parchment board with a believable route
        //    network, the Black Sea and the Danube, plus a compass and legend. ──
        Vector3 mapC = new Vector3(10f, 3.9f, 20.5f);
        Box(root, "Map_Frame",   mapC, new Vector3(9.0f, 6.0f, 0.25f), woodMat);
        Box(root, "Map_Mat",     new Vector3(mapC.x, mapC.y, 20.44f), new Vector3(8.4f, 5.4f, 0.05f), Mat(new Color(0.70f, 0.64f, 0.50f)));
        Box(root, "Map_Surface", new Vector3(mapC.x, mapC.y, 20.42f), new Vector3(7.9f, 4.9f, 0.05f), Mat(new Color(0.90f, 0.85f, 0.73f)));

        // Black Sea panel (east edge) + Danube ribbon — calm, natural blues
        Material seaMat   = Mat(new Color(0.46f, 0.60f, 0.66f), 0.10f, 0.5f);
        Material riverMat = Mat(new Color(0.54f, 0.67f, 0.72f), 0.10f, 0.5f);
        Box(root, "Map_Sea",     new Vector3(mapC.x + 3.35f, mapC.y - 0.6f, 20.41f), new Vector3(1.0f, 3.6f, 0.04f), seaMat);
        Box(root, "Map_Danube1", new Vector3(mapC.x - 0.4f, mapC.y - 1.95f, 20.40f), new Vector3(4.6f, 0.09f, 0.04f), riverMat, new Vector3(0, 0, 4));
        Box(root, "Map_Danube2", new Vector3(mapC.x + 2.3f, mapC.y - 1.4f, 20.40f), new Vector3(1.7f, 0.09f, 0.04f), riverMat, new Vector3(0, 0, 62));
        Sign(root, "Map_Title", "<b>RAILWAY NETWORK · CFR</b>", new Vector3(mapC.x, mapC.y + 2.15f, 20.33f), spawn, 0.7f,
            new Color(0.18f, 0.13f, 0.08f), new Vector2(7.6f, 0.7f), TextAlignmentOptions.Center, null);

        (string name, float ox, float oy)[] cities =
        {
            ("București", 0.3f, -1.4f), ("Brașov", -0.1f, 0.3f), ("Cluj-Napoca", -2.7f, 1.4f),
            ("Iași", 2.6f, 1.5f), ("Timișoara", -3.2f, -0.2f), ("Constanța", 2.9f, -1.4f),
            ("Craiova", -1.7f, -1.7f),
        };
        // Network edges (indices into cities) — a believable rail web, not a star.
        (int a, int b)[] edges =
        {
            (0, 1), (1, 2), (2, 4), (4, 6), (6, 0), (1, 3), (0, 3), (0, 5), (1, 5),
        };
        Material routeMat = Mat(new Color(0.50f, 0.20f, 0.12f), 0f, 0.3f);
        Material dotMat   = Mat(new Color(0.74f, 0.16f, 0.14f), 0f, 0.3f, new Color(0.25f, 0.02f, 0.02f));
        foreach (var e in edges)
        {
            Vector3 pa = new Vector3(mapC.x + cities[e.a].ox, mapC.y + cities[e.a].oy, 20.38f);
            Vector3 pb = new Vector3(mapC.x + cities[e.b].ox, mapC.y + cities[e.b].oy, 20.38f);
            Vector3 d = pb - pa; float len = d.magnitude;
            float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            Box(root, $"Route_{e.a}_{e.b}", (pa + pb) * 0.5f, new Vector3(len, 0.06f, 0.03f), routeMat, new Vector3(0, 0, ang));
        }
        foreach (var ct in cities)
        {
            Vector3 dp = new Vector3(mapC.x + ct.ox, mapC.y + ct.oy, 20.36f);
            Box(root, $"Dot_{ct.name}", dp, new Vector3(0.26f, 0.26f, 0.07f), dotMat);
            Sign(root, $"City_{ct.name}", ct.name, new Vector3(dp.x, dp.y + 0.34f, 20.34f), spawn, 0.42f,
                new Color(0.12f, 0.08f, 0.04f), new Vector2(2.6f, 0.45f), TextAlignmentOptions.Center, null);
        }
        // Compass + legend
        Sign(root, "Map_Compass", "<b>N</b>", new Vector3(mapC.x - 3.2f, mapC.y + 1.85f, 20.34f), spawn, 0.6f,
            new Color(0.15f, 0.10f, 0.06f), new Vector2(0.8f, 0.7f), TextAlignmentOptions.Center, null);
        Sign(root, "Map_SeaLabel", "Black Sea", new Vector3(mapC.x + 3.35f, mapC.y + 1.4f, 20.34f), spawn, 0.34f,
            new Color(0.12f, 0.20f, 0.28f), new Vector2(1.1f, 0.8f), TextAlignmentOptions.Center, null);
        Sign(root, "Map_Legend", "Red dot = city     Line = railway", new Vector3(mapC.x, mapC.y - 2.25f, 20.34f), spawn, 0.36f,
            new Color(0.15f, 0.10f, 0.06f), new Vector2(6.5f, 0.5f), TextAlignmentOptions.Center, null);

        // Ticket machines near the entrance
        for (int i = 0; i < 3; i++)
        {
            Vector3 mp = new Vector3(-12 + i * 4f, 1.1f, -14f);
            Box(root, $"TicketMachine_{i}", mp, new Vector3(1.1f, 2.2f, 0.8f), metalDark);
            Box(root, $"TM_Screen_{i}", mp + new Vector3(0, 0.5f, 0.42f), new Vector3(0.8f, 0.6f, 0.05f),
                Mat(new Color(0.2f, 0.5f, 0.7f), 0.2f, 0.7f, new Color(0.1f, 0.3f, 0.45f)));
            Sign(root, $"TM_Label_{i}", "TICKETS", mp + new Vector3(0, 1.5f, 0.43f), spawn, 1.0f, Color.white, new Vector2(2.2f, 1.2f), TextAlignmentOptions.Center, null);
        }

        // Mission station: the ticket office desk
        Material deskMat = Mat(new Color(0.30f, 0.22f, 0.14f));
        Station(root, typeof(GeographyMission), "Information Desk\n[Identify your destination]",
            new Vector3(6, 0.9f, 6.5f), spawn, deskMat);

        // Inspectable geography exhibits on stands
        Exhibit(root, "Carpathian Mountains", "A great arc of mountains crossing Romania — home to brown bears, lynx and the famous Transfăgărășan road.", new Vector3(-18, 0.7f, -8), spawn, signYellow);
        Exhibit(root, "Danube Delta", "Where the Danube meets the Black Sea — Europe's largest reed bed and a UNESCO biosphere reserve full of birds.", new Vector3(-18, 0.7f, -3), spawn, signYellow);
        Exhibit(root, "Bran Castle", "A medieval castle in Transylvania, popularly linked to Dracula and inspired by Vlad the Impaler (Vlad Țepeș).", new Vector3(-18, 0.7f, 2), spawn, signYellow);

        // Exit / return
        DoorAt(root, "Exit", "EXIT", new Vector3(18, 1.25f, -18.6f), spawn, woodMat);

        Player(spawn, 0f);
        Bounds(new Vector3(0, 0, 1), new Vector3(46, 26, 44));
        Managers();
        Save(scene, "GeographyScene");
    }

    // =====================================================================
    //  Castle Museum  (HistoryScene)
    // =====================================================================
    private static void BuildCastleMuseumInternal()
    {
        Scene scene = NewScene();
        Lighting(new Color(0.30f, 0.28f, 0.27f), 0.55f, new Vector3(55f, -25f, 0f), new Color(0.5f, 0.42f, 0.32f));

        Transform root = new GameObject("Castle").transform;
        Material stone = Mat(new Color(0.42f, 0.41f, 0.39f));
        Material stoneDark = Mat(new Color(0.30f, 0.29f, 0.28f));
        Material floorStone = Mat(new Color(0.34f, 0.33f, 0.32f));
        Material plinthMat = Mat(new Color(0.80f, 0.78f, 0.72f));
        Material gold = Mat(new Color(0.85f, 0.66f, 0.18f), 0.85f, 0.7f, new Color(0.25f, 0.18f, 0.02f));
        Material steel = Mat(new Color(0.66f, 0.68f, 0.72f), 0.9f, 0.7f);
        Material wood = Mat(new Color(0.34f, 0.22f, 0.12f));
        Material blue = Mat(new Color(0.0f, 0.18f, 0.55f));
        Material yellow = Mat(new Color(0.95f, 0.78f, 0.05f));
        Material red = Mat(new Color(0.78f, 0.07f, 0.10f));

        Vector3 spawn = new Vector3(0f, 1f, -16f);

        Floor(root, "Floor", Vector3.zero, new Vector3(4f, 1f, 4f), floorStone);
        // Tall stone hall
        Box(root, "Wall_N", new Vector3(0, 5, 19), new Vector3(40, 10, 0.8f), stone);
        Box(root, "Wall_S", new Vector3(0, 5, -19), new Vector3(40, 10, 0.8f), stone);
        Box(root, "Wall_W", new Vector3(-19, 5, 0), new Vector3(0.8f, 10, 38), stone);
        Box(root, "Wall_E", new Vector3(19, 5, 0), new Vector3(0.8f, 10, 38), stone);
        Box(root, "Ceiling", new Vector3(0, 10, 0), new Vector3(40, 0.6f, 40), stoneDark);

        // Columns lining the hall, with a brazier light on each
        for (int side = -1; side <= 1; side += 2)
            for (int i = 0; i < 4; i++)
            {
                Vector3 cp = new Vector3(side * 11f, 4.5f, -9f + i * 6f);
                Cyl(root, $"Column_{side}_{i}", cp, new Vector3(1.1f, 4.5f, 1.1f), stoneDark);
                Cyl(root, $"ColTop_{side}_{i}", cp + new Vector3(0, 4.6f, 0), new Vector3(1.4f, 0.3f, 1.4f), stone);
                PointLight(root, $"Brazier_{side}_{i}", cp + new Vector3(0, 4.0f, 0), new Color(1f, 0.6f, 0.25f), 9f, 2.2f);
            }

        // Tricolor banners on the north wall
        Box(root, "Banner_B", new Vector3(-4, 6.5f, 18.4f), new Vector3(1.2f, 6f, 0.1f), blue);
        Box(root, "Banner_Y", new Vector3(0, 6.5f, 18.4f), new Vector3(1.2f, 6f, 0.1f), yellow);
        Box(root, "Banner_R", new Vector3(4, 6.5f, 18.4f), new Vector3(1.2f, 6f, 0.1f), red);

        // Timeline wall text
        Sign(root, "Timeline", "<b>TIMELINE</b>\n106 — Roman conquest of Dacia\n1859 — Union of the Principalities\n1918 — The Great Union\n1989 — The Revolution",
            new Vector3(10, 5.5f, 18.3f), spawn, 1.6f, new Color(1f, 0.92f, 0.7f), new Vector2(12, 5), TextAlignmentOptions.TopLeft, stoneDark);

        // Artifact plinths
        PlinthArtifact(root, "Royal Crown", "The steel crown of the Kingdom of Romania, cast from cannons captured at Plevna (1877).",
            new Vector3(-7, 0, -4), spawn, plinthMat, () => Box(root, "Crown", new Vector3(-7, 1.7f, -4), new Vector3(0.8f, 0.5f, 0.8f), gold));
        PlinthArtifact(root, "Trajan's Column", "A monument in Rome depicting the Dacian Wars (101–106), when Trajan conquered Dacia.",
            new Vector3(0, 0, -4), spawn, plinthMat, () => Cyl(root, "Column_art", new Vector3(0, 2.1f, -4), new Vector3(0.35f, 0.9f, 0.35f), plinthMat));
        PlinthArtifact(root, "Medieval Sword", "A sword from the age of the voivodes — a symbol of the fight for the principalities' independence.",
            new Vector3(7, 0, -4), spawn, plinthMat, () => Box(root, "Sword", new Vector3(7, 1.9f, -4), new Vector3(0.12f, 1.1f, 0.12f), steel));

        // Mission station: museum lectern
        Station(root, typeof(HistoryMission), "Museum Guide\n[Start the history trail]",
            new Vector3(0, 0.9f, 7f), spawn, wood);

        // Exit gate
        DoorAt(root, "Gate", "CASTLE GATE", new Vector3(0, 1.6f, -18.5f), spawn, wood);

        Player(spawn, 0f);
        Bounds(new Vector3(0, 0, 0), new Vector3(40, 26, 40));
        Managers();
        Save(scene, "HistoryScene");
    }

    // =====================================================================
    //  Theatre  (MusicScene)
    // =====================================================================
    private static void BuildTheatreInternal()
    {
        Scene scene = NewScene();
        Lighting(new Color(0.20f, 0.18f, 0.22f), 0.35f, new Vector3(60f, -10f, 0f), new Color(0.4f, 0.35f, 0.5f));

        Transform root = new GameObject("Theatre").transform;
        Material hallFloor = Mat(new Color(0.30f, 0.10f, 0.12f));
        Material stageWood = Mat(new Color(0.45f, 0.30f, 0.16f), 0.1f, 0.5f);
        Material curtain = Mat(new Color(0.55f, 0.05f, 0.10f), 0f, 0.2f);
        Material gold = Mat(new Color(0.85f, 0.66f, 0.18f), 0.85f, 0.7f, new Color(0.25f, 0.18f, 0.02f));
        Material wallMat = Mat(new Color(0.16f, 0.10f, 0.14f));
        Material seatMat = Mat(new Color(0.55f, 0.08f, 0.12f));
        Material black = Mat(new Color(0.05f, 0.05f, 0.06f), 0.3f, 0.6f);
        Material brown = Mat(new Color(0.42f, 0.26f, 0.12f));
        Material wood = Mat(new Color(0.34f, 0.22f, 0.12f));

        Vector3 spawn = new Vector3(0f, 1f, -16f);

        Floor(root, "HallFloor", Vector3.zero, new Vector3(4f, 1f, 4.2f), hallFloor);
        Box(root, "Wall_N", new Vector3(0, 5, 21), new Vector3(40, 12, 0.8f), wallMat);
        Box(root, "Wall_S", new Vector3(0, 5, -19), new Vector3(40, 12, 0.8f), wallMat);
        Box(root, "Wall_W", new Vector3(-19, 5, 1), new Vector3(0.8f, 12, 42), wallMat);
        Box(root, "Wall_E", new Vector3(19, 5, 1), new Vector3(0.8f, 12, 42), wallMat);
        Box(root, "Ceiling", new Vector3(0, 11, 1), new Vector3(40, 0.6f, 42), wallMat);

        // Stage
        Box(root, "Stage", new Vector3(0, 0.6f, 14), new Vector3(26, 1.2f, 12), stageWood);
        Box(root, "Backdrop", new Vector3(0, 6, 19.5f), new Vector3(26, 11, 0.4f), Mat(new Color(0.08f, 0.06f, 0.12f)));

        // Proscenium arch + curtains
        Box(root, "Arch_L", new Vector3(-13, 6, 8.5f), new Vector3(1.2f, 12, 1.2f), gold);
        Box(root, "Arch_R", new Vector3(13, 6, 8.5f), new Vector3(1.2f, 12, 1.2f), gold);
        Box(root, "Arch_Top", new Vector3(0, 11.4f, 8.5f), new Vector3(27, 1.2f, 1.2f), gold);
        Box(root, "Curtain_L", new Vector3(-10.5f, 6, 8.8f), new Vector3(5, 11, 0.5f), curtain);
        Box(root, "Curtain_R", new Vector3(10.5f, 6, 8.8f), new Vector3(5, 11, 0.5f), curtain);
        Box(root, "Valance", new Vector3(0, 10.5f, 8.8f), new Vector3(26, 2.5f, 0.5f), curtain);

        // Spotlights on the stage
        PointLight(root, "Spot_L", new Vector3(-7, 9, 10), new Color(1f, 0.95f, 0.8f), 16f, 2.6f);
        PointLight(root, "Spot_R", new Vector3(7, 9, 10), new Color(1f, 0.95f, 0.8f), 16f, 2.6f);
        PointLight(root, "Spot_C", new Vector3(0, 9, 15), new Color(1f, 0.92f, 0.75f), 18f, 2.2f);

        // Orchestra: chairs + stands in a shallow arc, plus playable instruments
        for (int i = -3; i <= 3; i++)
        {
            float ix = i * 3.0f;
            float iz = 15f + Mathf.Abs(i) * 0.6f;
            Box(root, $"Chair_{i}", new Vector3(ix, 1.45f, iz), new Vector3(0.7f, 0.9f, 0.7f), black);
            Box(root, $"Stand_{i}", new Vector3(ix, 1.9f, iz - 1f), new Vector3(0.5f, 0.05f, 0.4f), black, new Vector3(-30, 0, 0));
            Cyl(root, $"StandPole_{i}", new Vector3(ix, 1.55f, iz - 1f), new Vector3(0.06f, 0.5f, 0.06f), black);
        }

        // Grand piano (stage left)
        Box(root, "Piano", new Vector3(-8, 1.6f, 13), new Vector3(3.2f, 1.0f, 2.0f), black);
        Box(root, "Piano_Lid", new Vector3(-8, 2.2f, 13.6f), new Vector3(3.2f, 0.08f, 1.2f), black, new Vector3(-22, 0, 0));

        // Playable instruments (SoundProp) on small stands
        Instrument(root, "nai", "Nai (Pan Flute)", new Vector3(5, 1.7f, 13), gold, true);
        Instrument(root, "vioara", "Vioară (Violin)", new Vector3(8, 1.7f, 14.5f), brown, false);
        Instrument(root, "tambal", "Țambal (Cimbalom)", new Vector3(3, 1.55f, 11.5f), brown, false);
        Instrument(root, "cobza", "Cobză (Lute)", new Vector3(-3, 1.7f, 11.5f), brown, false);
        Instrument(root, "cimpoi", "Cimpoi (Bagpipe)", new Vector3(-5, 1.7f, 14.5f), Mat(new Color(0.5f,0.35f,0.2f)), false);

        // Audience seating rows
        for (int r = 0; r < 4; r++)
            for (int s = -5; s <= 5; s++)
                Box(root, $"Seat_{r}_{s}", new Vector3(s * 2.0f, 0.6f, -10 + r * 2.4f), new Vector3(1.4f, 1.0f, 1.2f), seatMat);

        // Jukebox podium
        GameObject jb = Box(root, "Jukebox", new Vector3(-9, 1.0f, -12), new Vector3(1.4f, 2.0f, 1.0f), gold);
        jb.AddComponent<MusicJukebox>();
        Sign(root, "Jukebox_Sign", "JUKEBOX\n[Play music]", new Vector3(-9, 2.4f, -11.4f), spawn, 1.0f, Color.white, new Vector2(3, 1.4f), TextAlignmentOptions.Center, null);

        // Mission station: conductor's podium at the front of the stage
        Station(root, typeof(MusicMission), "Conductor's Podium\n[Start the music lesson]",
            new Vector3(0, 1.5f, 10.5f), spawn, gold);

        // Inspectable music exhibits
        Exhibit(root, "George Enescu", "Romania's greatest composer (1881–1955). His 'Romanian Rhapsodies' turn folk melodies into orchestral music.", new Vector3(-16, 0.7f, -6), spawn, wood);
        Exhibit(root, "Maria Tănase", "A legendary folk singer (1913–1963), often called 'the Romanian Edith Piaf'.", new Vector3(-16, 0.7f, -1), spawn, wood);
        Exhibit(root, "Gheorghe Zamfir", "A world master of the pan flute (nai) — his distinctive sound made the instrument famous.", new Vector3(16, 0.7f, -6), spawn, wood);

        DoorAt(root, "Exit", "EXIT", new Vector3(16, 1.6f, -18.5f), spawn, wood);

        Player(spawn, 0f);
        Bounds(new Vector3(0, 0, 1), new Vector3(40, 28, 44));
        Managers();
        Save(scene, "MusicScene");
    }

    // =====================================================================
    //  Menu scene
    // =====================================================================
    private static void BuildMenuSceneInternal()
    {
        Scene scene = NewScene();
        Lighting(new Color(0.4f, 0.4f, 0.5f), 0.8f, new Vector3(45f, -20f, 0f), Color.white);

        GameObject camGo = new GameObject("MenuCamera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.06f, 0.12f);
        camGo.AddComponent<AudioListener>();
        camGo.transform.position = new Vector3(0, 2, -8);

        new GameObject("MainMenu").AddComponent<MainMenuController>();

        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();

        Save(scene, "MenuScene");
    }

    // =====================================================================
    //  Finale scene
    // =====================================================================
    private static void BuildFinaleInternal()
    {
        Scene scene = NewScene();
        Lighting(new Color(0.5f, 0.45f, 0.35f), 0.9f, new Vector3(50f, -15f, 0f), new Color(1f, 0.9f, 0.7f));

        Transform root = new GameObject("Finale").transform;
        Material marble = Mat(new Color(0.82f, 0.80f, 0.74f));
        Material gold = Mat(new Color(0.85f, 0.66f, 0.18f), 0.85f, 0.75f, new Color(0.3f, 0.22f, 0.03f));
        Material red = Mat(new Color(0.6f, 0.08f, 0.10f));

        Vector3 spawn = new Vector3(0f, 1f, -12f);

        Floor(root, "Floor", Vector3.zero, new Vector3(3f, 1f, 3f), marble);
        Box(root, "Wall_N", new Vector3(0, 4, 15), new Vector3(30, 8, 0.6f), marble);
        Box(root, "Wall_S", new Vector3(0, 4, -15), new Vector3(30, 8, 0.6f), marble);
        Box(root, "Wall_W", new Vector3(-15, 4, 0), new Vector3(0.6f, 8, 30), marble);
        Box(root, "Wall_E", new Vector3(15, 4, 0), new Vector3(0.6f, 8, 30), marble);

        for (int side = -1; side <= 1; side += 2)
            for (int i = 0; i < 3; i++)
                Cyl(root, $"Col_{side}_{i}", new Vector3(side * 8f, 3f, -6f + i * 6f), new Vector3(1f, 3f, 1f), gold);

        Box(root, "RedCarpet", new Vector3(0, 0.02f, 0), new Vector3(4, 0.04f, 26), red);
        PointLight(root, "Glow", new Vector3(0, 6, 8), new Color(1f, 0.85f, 0.5f), 20f, 2.5f);

        Sign(root, "Banner", "<b>THE GRAND FINAL CHALLENGE</b>", new Vector3(0, 6, 14.5f), spawn, 2.0f,
            new Color(1f, 0.9f, 0.55f), new Vector2(22, 3), TextAlignmentOptions.Center, gold);

        // Finale station
        GameObject st = Box(root, "FinaleStation", new Vector3(0, 1.0f, 7f), new Vector3(2.6f, 1.4f, 1.4f), gold);
        st.AddComponent<FinaleMission>();
        st.AddComponent<FinaleStation>();
        Sign(root, "FinaleStation_Sign", "Final Challenge\n[Start the final test]", new Vector3(0, 2.0f, 7f), spawn, 1.2f, Color.white, new Vector2(6, 1.8f), TextAlignmentOptions.Center, null);

        // Exit door — set clear of the south wall (the old build had it flush with the
        // wall, so the crosshair caught the wall instead and the door wouldn't trigger).
        // A green glow makes it easy to find after the challenge.
        DoorAt(root, "Exit", "BACK TO THE CITY", new Vector3(0, 1.6f, -13.0f), spawn, Mat(new Color(0.34f, 0.22f, 0.12f)));
        PointLight(root, "ExitGlow", new Vector3(0, 2.8f, -12.0f), new Color(0.55f, 1f, 0.75f), 7f, 1.6f);

        Player(spawn, 0f);
        Bounds(new Vector3(0, 0, 0), new Vector3(32, 22, 32));
        Managers();
        Save(scene, "FinaleScene");
    }

    // =====================================================================
    //  Building-block helpers
    // =====================================================================
    private static Scene NewScene() =>
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

    private static void Save(Scene scene, string name)
    {
        if (!Directory.Exists(ScenesFolder)) Directory.CreateDirectory(ScenesFolder);
        string path = $"{ScenesFolder}/{name}.unity";
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"[Romania Game] Built {path}");
    }

    private static void Lighting(Color ambient, float sunIntensity, Vector3 sunEuler, Color sunColor)
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambient;
        GameObject sun = new GameObject("Directional Light");
        Light l = sun.AddComponent<Light>();
        l.type = LightType.Directional;
        l.intensity = sunIntensity;
        l.color = sunColor;
        l.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(sunEuler);
    }

    private static Material Mat(Color c, float metallic = 0f, float smooth = 0.25f, Color? emission = null)
    {
        Material m = new Material(Shader.Find("Standard"));
        m.color = c;
        m.SetFloat("_Metallic", metallic);
        m.SetFloat("_Glossiness", smooth);
        if (emission.HasValue)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emission.Value);
        }
        return m;
    }

    private static GameObject Box(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat, Vector3 euler = default)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        if (parent != null) go.transform.SetParent(parent, true);
        go.transform.position = pos;
        go.transform.eulerAngles = euler;
        go.transform.localScale = scale;
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    private static GameObject Cyl(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat, Vector3 euler = default)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        if (parent != null) go.transform.SetParent(parent, true);
        go.transform.position = pos;
        go.transform.eulerAngles = euler;
        go.transform.localScale = scale;   // note: cylinder is 2 units tall at scale.y = 1
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    // A bogie (wheel truck): a low frame with two axles — one wheel set on the
    // player-facing side and one set behind — placed under a rail vehicle.
    private static void Bogie(Transform parent, string name, Vector3 center, float faceZ, Material frameMat, Material wheelMat)
    {
        Box(parent, name + "_frame", new Vector3(center.x, 0.62f, center.z), new Vector3(2.4f, 0.35f, 1.9f), frameMat);
        for (int w = 0; w < 2; w++)
        {
            float wx = center.x - 0.75f + w * 1.5f;
            Cyl(parent, $"{name}_w{w}f", new Vector3(wx, 0.42f, faceZ + 0.05f), new Vector3(0.72f, 0.12f, 0.72f), wheelMat, new Vector3(90, 0, 0));
            Cyl(parent, $"{name}_w{w}b", new Vector3(wx, 0.42f, center.z + 1.0f), new Vector3(0.72f, 0.12f, 0.72f), wheelMat, new Vector3(90, 0, 0));
        }
    }

    private static void Floor(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = name;
        if (parent != null) go.transform.SetParent(parent, true);
        go.transform.position = pos;
        go.transform.localScale = scale;
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static void PointLight(Transform parent, string name, Vector3 pos, Color color, float range, float intensity)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, true);
        go.transform.position = pos;
        Light l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = color;
        l.range = range;
        l.intensity = intensity;
    }

    private static GameObject Player(Vector3 pos, float yaw)
    {
        GameObject p = new GameObject("Player");
        p.tag = "Player";
        p.transform.position = pos;
        p.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        CharacterController cc = p.AddComponent<CharacterController>();
        cc.height = 1.8f; cc.radius = 0.3f; cc.center = new Vector3(0f, 0.9f, 0f);

        GameObject cam = new GameObject("PlayerCamera");
        cam.tag = "MainCamera";
        cam.transform.SetParent(p.transform, false);
        cam.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        cam.AddComponent<Camera>();
        cam.AddComponent<AudioListener>();

        p.AddComponent<PlayerMovement>();
        p.AddComponent<PlayerInteraction>();
        return p;
    }

    private static void Managers()
    {
        new GameObject("GameFlowManager").AddComponent<GameFlowManager>();
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    private static void Bounds(Vector3 center, Vector3 size)
    {
        WorldBounds wb = new GameObject("WorldBoundary").AddComponent<WorldBounds>();
        wb.areaCenter = center;
        wb.areaSize = size;
        wb.wallHeight = size.y;
        wb.fallY = center.y - 10f;
    }

    private static void Station(Transform parent, Type contentType, string sign, Vector3 pos, Vector3 spawn, Material mat)
    {
        GameObject s = Box(parent, "MissionStation", pos, new Vector3(2.4f, 1.2f, 1.2f), mat);
        s.AddComponent(contentType);
        s.AddComponent<MissionStation>();
        Sign(parent, "Station_Sign", sign, pos + new Vector3(0, 1.5f, 0), spawn, 1.1f, new Color(1f, 0.95f, 0.7f), new Vector2(6, 2), TextAlignmentOptions.Center, null);
    }

    private static void DoorAt(Transform parent, string name, string sign, Vector3 pos, Vector3 spawn, Material mat)
    {
        GameObject d = Box(parent, name, pos, new Vector3(2f, 2.5f, 0.4f), mat);
        d.AddComponent<ReturnPortal>();
        Sign(parent, name + "_Sign", sign, pos + new Vector3(0, 1.7f, 0), spawn, 1.0f, new Color(0.6f, 1f, 0.7f), new Vector2(5, 1.2f), TextAlignmentOptions.Center, null);
    }

    private static void Exhibit(Transform parent, string objName, string info, Vector3 pos, Vector3 spawn, Material standMat)
    {
        Box(parent, $"Stand_{objName}", pos, new Vector3(1f, 1.4f, 1f), standMat);
        GameObject art = Box(parent, $"Exhibit_{objName}", pos + new Vector3(0, 1.0f, 0), new Vector3(0.6f, 0.6f, 0.6f), standMat);
        InfoObject io = art.AddComponent<InfoObject>();
        io.objectName = objName;
        io.info = info;
        Sign(parent, $"{objName}_label", objName, pos + new Vector3(0, 1.9f, 0), spawn, 0.9f, Color.white, new Vector2(4, 0.8f), TextAlignmentOptions.Center, null);
    }

    private static void PlinthArtifact(Transform parent, string objName, string info, Vector3 basePos, Vector3 spawn, Material plinthMat, Action buildArt)
    {
        Box(parent, $"Plinth_{objName}", basePos + new Vector3(0, 0.8f, 0), new Vector3(1.2f, 1.6f, 1.2f), plinthMat);
        buildArt?.Invoke();
        // An invisible-ish info collider so the whole plinth is inspectable.
        GameObject info0 = Box(parent, $"Info_{objName}", basePos + new Vector3(0, 1.7f, 0), new Vector3(1.4f, 1.4f, 1.4f), null);
        info0.GetComponent<Renderer>().enabled = false;
        InfoObject io = info0.AddComponent<InfoObject>();
        io.objectName = objName;
        io.info = info;
        Sign(parent, $"{objName}_label", objName, basePos + new Vector3(0, 2.9f, 0), spawn, 1.0f, new Color(1f, 0.9f, 0.6f), new Vector2(5, 1), TextAlignmentOptions.Center, null);
    }

    private static void Instrument(Transform parent, string instrumentKey, string label, Vector3 pos, Material mat, bool pipes)
    {
        GameObject body;
        if (pipes)
        {
            // Pan-flute look: a row of pipes of varying height.
            body = new GameObject($"Instrument_{label}");
            body.transform.SetParent(parent, true);
            body.transform.position = pos;
            for (int i = 0; i < 6; i++)
                Cyl(body.transform, $"pipe_{i}", pos + new Vector3(-0.3f + i * 0.12f, 0.1f * i, 0), new Vector3(0.08f, 0.25f + 0.06f * i, 0.08f), mat);
        }
        else
        {
            body = Box(parent, $"Instrument_{label}", pos, new Vector3(0.6f, 0.5f, 0.25f), mat);
        }
        SoundProp sp = body.AddComponent<SoundProp>();
        sp.instrument = instrumentKey;
        sp.displayName = label;
        if (body.GetComponentInChildren<Collider>() == null) body.AddComponent<BoxCollider>();
    }

    /// <summary>A world-space TextMeshPro label that faces the viewer's spawn, with an
    /// optional backing plate.</summary>
    private static void Sign(Transform parent, string name, string text, Vector3 pos, Vector3 viewer,
        float fontSize, Color color, Vector2 size, TextAlignmentOptions align, Material plateMat)
    {
        Quaternion rot = FaceViewer(pos, viewer);

        if (plateMat != null)
        {
            GameObject plate = Box(parent, name + "_plate", pos, new Vector3(size.x, size.y, 0.1f), plateMat);
            plate.transform.rotation = rot;
        }

        // Create with RectTransform + TextMeshPro from the start (reliable 3D-text setup).
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshPro));
        go.transform.SetParent(parent, true);
        TextMeshPro tmp = go.GetComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        tmp.enableWordWrapping = true;
        ((RectTransform)go.transform).sizeDelta = size;
        go.transform.position = pos + rot * new Vector3(0, 0, -0.06f);   // float just in front of any plate
        go.transform.rotation = rot;
    }

    private static Quaternion FaceViewer(Vector3 textPos, Vector3 viewerPos)
    {
        Vector3 dir = viewerPos - textPos;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return Quaternion.identity;
        return Quaternion.LookRotation(dir.normalized, Vector3.up);
    }

    private static bool TryComputeSceneBounds(out Bounds bounds)
    {
        bounds = new Bounds();
        bool any = false;
        foreach (Renderer r in UnityEngine.Object.FindObjectsOfType<Renderer>())
        {
            // Skip absurdly large renderers (skyboxes, ground planes that span the world).
            if (r.bounds.size.magnitude > 600f) continue;
            if (!any) { bounds = r.bounds; any = true; }
            else bounds.Encapsulate(r.bounds);
        }
        return any;
    }
}
