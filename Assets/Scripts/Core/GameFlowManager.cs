using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent controller for the "leave the island -> do a mission -> come back"
/// flow. Survives scene loads (DontDestroyOnLoad) and is a singleton.
///
/// Flow:
///   1. An NPC's <see cref="MissionGiver"/> calls StartMission(scene, id).
///      We remember where the player was standing, then load the mission scene.
///   2. Finishing the mini-game calls MarkCurrentMissionComplete() — it records the
///      completion and toasts, but the player STAYS in the room to keep exploring.
///   3. The player leaves whenever they like through the ReturnPortal door, which
///      calls LeaveMission(); we load the island and drop them where they left.
///
/// Completed missions are remembered for the session (<see cref="IsComplete"/>),
/// but completed missions can still be re-entered.
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Tooltip("Scene name of the main island. Must be in File > Build Settings.")]
    public string mainSceneName = "MainScene";

    private string currentMissionId;
    private Vector3 returnPosition;
    private Quaternion returnRotation;
    private bool hasReturnPoint;
    private bool repositionPending;

    private readonly HashSet<string> completedMissions = new HashSet<string>();

    // Guarantee a GameFlowManager exists before the first scene loads, so you
    // never have to remember to place one in the scene.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance == null)
            new GameObject("GameFlowManager (auto)").AddComponent<GameFlowManager>();
    }

    void Awake()
    {
        // Standard persistent-singleton guard. Any duplicate that ships inside a
        // freshly loaded scene destroys itself, leaving the original in charge.
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>Remember the player's spot, then load a mission scene.</summary>
    public void StartMission(string sceneName, string missionId)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[GameFlow] StartMission called with no scene name.");
            return;
        }

        currentMissionId = missionId;
        SaveReturnPoint();
        UIBlocker.Reset();
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Record the active mission as done and toast — but stay in the room so the
    /// player can keep exploring. They return on their own via the ReturnPortal.
    /// </summary>
    public void MarkCurrentMissionComplete()
    {
        if (!string.IsNullOrEmpty(currentMissionId))
        {
            bool first = completedMissions.Add(currentMissionId);
            Debug.Log($"[GameFlow] Mission complete: {currentMissionId}");
            ScreenPrompt.Toast(first
                ? "Mission complete! Explore, or use the door to return."
                : "Completed again! Use the door to return.", 4f);
        }
    }

    /// <summary>Leave the current mission and go back to the island.</summary>
    public void LeaveMission()
    {
        currentMissionId = null;
        repositionPending = hasReturnPoint;
        UIBlocker.Reset();
        SceneManager.LoadScene(mainSceneName);
    }

    public bool IsComplete(string missionId) => completedMissions.Contains(missionId);

    private void SaveReturnPoint()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) { hasReturnPoint = false; return; }

        returnPosition = player.transform.position;
        // Keep yaw only — pitch lives on the camera and resets cleanly on reload.
        returnRotation = Quaternion.Euler(0f, player.transform.eulerAngles.y, 0f);
        hasReturnPoint = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UIBlocker.Reset();

        if (scene.name == mainSceneName && repositionPending)
            StartCoroutine(RepositionPlayerNextFrame());
    }

    private IEnumerator RepositionPlayerNextFrame()
    {
        // Wait one frame so the island's player has run its own Start() first.
        yield return null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;    // CharacterController fights direct moves
            player.transform.SetPositionAndRotation(returnPosition, returnRotation);
            if (cc != null) cc.enabled = true;
        }
        repositionPending = false;
    }
}
