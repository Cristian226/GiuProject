using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor tools that generate the mission scenes for you. Each generated scene is
/// a real, walkable room with a first-person player, a mission station (the
/// learning mini-game) and an exit door — all wired up. Decorate them afterwards
/// with your own props (e.g. the Low-Poly Medieval Market pack).
///
/// Menu: Tools > Romania Game.
/// </summary>
public static class MissionSceneBuilder
{
    private const string ScenesFolder = "Assets/Scenes";
    private const string MainScenePath = "Assets/Scenes/MainScene.unity";

    [MenuItem("Tools/Romania Game/Build ALL Mission Scenes")]
    public static void BuildAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        BuildScene("CuisineScene", typeof(CuisineMission), "Cooking Station", CuisineExhibits());
        BuildScene("HistoryScene", typeof(HistoryMission), "Museum Plinth", HistoryExhibits());
        BuildScene("GeographyScene", typeof(GeographyMission), "Map Table", GeographyExhibits());
        BuildScene("MusicScene", typeof(MusicMission), "Stage", MusicExhibits());
        RegisterScenes();

        EditorUtility.DisplayDialog("Romania Game",
            "Built CuisineScene, HistoryScene, GeographyScene and MusicScene, and registered " +
            "them in Build Settings.\n\nOpen each scene to decorate it, then wire your island " +
            "NPCs with MissionGiver (see SETUP_MISSIONS.md).", "Great");
    }

    [MenuItem("Tools/Romania Game/Build Cuisine Scene")]
    public static void BuildCuisine()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        BuildScene("CuisineScene", typeof(CuisineMission), "Cooking Station", CuisineExhibits());
        RegisterScenes();
    }

    [MenuItem("Tools/Romania Game/Build History Scene")]
    public static void BuildHistory()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        BuildScene("HistoryScene", typeof(HistoryMission), "Museum Plinth", HistoryExhibits());
        RegisterScenes();
    }

    [MenuItem("Tools/Romania Game/Build Geography Scene")]
    public static void BuildGeography()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        BuildScene("GeographyScene", typeof(GeographyMission), "Map Table", GeographyExhibits());
        RegisterScenes();
    }

    [MenuItem("Tools/Romania Game/Build Music Scene")]
    public static void BuildMusic()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        BuildScene("MusicScene", typeof(MusicMission), "Stage", MusicExhibits());
        RegisterScenes();
    }

    [MenuItem("Tools/Romania Game/Register Scenes In Build Settings")]
    public static void RegisterScenes()
    {
        var paths = new List<string>
        {
            MainScenePath,
            $"{ScenesFolder}/CuisineScene.unity",
            $"{ScenesFolder}/HistoryScene.unity",
            $"{ScenesFolder}/GeographyScene.unity",
            $"{ScenesFolder}/MusicScene.unity",
        };

        var list = new List<EditorBuildSettingsScene>();
        foreach (string p in paths)
            if (File.Exists(p))
                list.Add(new EditorBuildSettingsScene(p, true));

        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log($"[Romania Game] Build Settings now has {list.Count} scene(s).");
    }

    // =========================================================
    //  Scene construction
    // =========================================================
    private static void BuildScene(string sceneName, Type contentType, string stationLabel,
        (string name, string info)[] exhibits)
    {
        if (!Directory.Exists(ScenesFolder)) Directory.CreateDirectory(ScenesFolder);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Lighting so the empty scene isn't pitch black ──
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.62f, 0.62f, 0.66f);
        GameObject sun = new GameObject("Directional Light");
        Light light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.0f;
        sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // ── Room (20 x 20, walls 4 high) ──
        CreatePlane("Floor", new Vector3(0, 0, 0), new Vector3(2, 1, 2));
        CreateCube("Wall_N", new Vector3(0, 2, 10), new Vector3(20, 4, 0.3f));
        CreateCube("Wall_S", new Vector3(0, 2, -10), new Vector3(20, 4, 0.3f));
        CreateCube("Wall_E", new Vector3(10, 2, 0), new Vector3(0.3f, 4, 20));
        CreateCube("Wall_W", new Vector3(-10, 2, 0), new Vector3(0.3f, 4, 20));

        // ── Player (first person) ──
        CreatePlayer(new Vector3(0, 1f, -7f));

        // ── Mission station (walk up + press E) ──
        GameObject station = CreateCube(stationLabel, new Vector3(0, 0.6f, 7f), new Vector3(2.4f, 1.2f, 1.2f));
        station.AddComponent(contentType);
        station.AddComponent<MissionStation>();

        // ── Exit door, in a back corner so the player doesn't trigger it on spawn ──
        GameObject door = CreateCube("ReturnDoor", new Vector3(-8f, 1.25f, -9f), new Vector3(1.6f, 2.5f, 0.3f));
        door.AddComponent<ReturnPortal>();

        // ── Inspectable exhibits along the east wall (look + press E) ──
        // Replace these placeholder cubes with real props later; keep the InfoObject.
        for (int i = 0; i < exhibits.Length; i++)
        {
            float z = -3f + i * 3f;
            GameObject pedestal = CreateCube($"Exhibit_{exhibits[i].name}", new Vector3(8f, 0.6f, z),
                new Vector3(1f, 1.2f, 1f));
            InfoObject io = pedestal.AddComponent<InfoObject>();
            io.objectName = exhibits[i].name;
            io.info = exhibits[i].info;
        }

        // ── Safety-net managers (deduped by the persistent ones when entered from the island) ──
        new GameObject("GameFlowManager").AddComponent<GameFlowManager>();
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();

        string path = $"{ScenesFolder}/{sceneName}.unity";
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"[Romania Game] Built {path}");
    }

    // =========================================================
    //  Helpers
    // =========================================================
    private static GameObject CreatePlane(string name, Vector3 pos, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = scale;
        return go;
    }

    private static GameObject CreateCube(string name, Vector3 pos, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = scale;
        return go;
    }

    private static GameObject CreatePlayer(Vector3 pos)
    {
        GameObject p = new GameObject("Player");
        p.tag = "Player";
        p.transform.position = pos;

        CharacterController cc = p.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0f, 0.9f, 0f);

        GameObject cam = new GameObject("PlayerCamera");
        cam.tag = "MainCamera";
        cam.transform.SetParent(p.transform, false);
        cam.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        cam.AddComponent<Camera>();
        cam.AddComponent<AudioListener>();

        // PlayerMovement & PlayerInteraction auto-find the child camera in Start().
        p.AddComponent<PlayerMovement>();
        p.AddComponent<PlayerInteraction>();
        return p;
    }

    // =========================================================
    //  Exhibit content (inspectable props placed in each room)
    // =========================================================
    private static (string, string)[] CuisineExhibits() => new (string, string)[]
    {
        ("Sarmale", "Cabbage rolls of minced meat and rice, slow-cooked for hours. A festive centrepiece, usually served with mămăligă and sour cream."),
        ("Mămăligă", "Boiled cornmeal, similar to polenta. A humble staple eaten with cheese (brânză), sour cream, or alongside stews."),
        ("Papanași", "Fried cheese doughnuts topped with sour cream and fruit jam — the most beloved Romanian dessert."),
    };

    private static (string, string)[] HistoryExhibits() => new (string, string)[]
    {
        ("Trajan's Column", "A monument in Rome carved with scenes of the Dacian Wars (101–106 AD), when Emperor Trajan conquered Dacia."),
        ("The Great Union", "On 1 December 1918, Transylvania united with Romania. It is celebrated today as the National Day."),
        ("1989 Revolution", "In December 1989 Romanians overthrew the communist regime, ending decades of dictatorship."),
    };

    private static (string, string)[] GeographyExhibits() => new (string, string)[]
    {
        ("Carpathian Mountains", "A great arc of mountains crossing Romania — home to brown bears, lynx, and the famous Transfăgărășan road."),
        ("Danube Delta", "Where the Danube meets the Black Sea: Europe's largest reed bed and a UNESCO biosphere reserve full of birds."),
        ("Bran Castle", "A medieval castle in Transylvania popularly tied to Bram Stoker's fictional Dracula, inspired by Vlad Țepeș."),
    };

    private static (string, string)[] MusicExhibits() => new (string, string)[]
    {
        ("Nai (Pan Flute)", "A Romanian pan flute of many tuned pipes. Virtuoso Gheorghe Zamfir made its breathy, haunting sound famous worldwide."),
        ("George Enescu", "Romania's greatest composer and violinist (1881–1955). His 'Romanian Rhapsodies' turn folk tunes into orchestral music; Bucharest's international festival bears his name."),
        ("Maria Tănase", "A legendary folk singer (1913–1963), often called 'the Romanian Édith Piaf', who brought traditional village songs to the concert stage."),
    };
}
