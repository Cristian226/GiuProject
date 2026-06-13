using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent director of scene flow: the main-menu → island → mission → island
/// loop. Survives scene loads (DontDestroyOnLoad) and is a singleton.
///
/// Progression (completed missions, collectibles, unlocks) is owned by
/// <see cref="GameProgress"/>; this class drives <em>scene transitions</em> and the
/// repositioning that makes the round-trips feel seamless.
///
/// Flow:
///   1. <see cref="StartMission"/> remembers where the player stood, then loads the
///      mission scene.
///   2. Finishing the mini-game calls <see cref="MarkCurrentMissionComplete"/> — it
///      records completion (+ awards a collectible) and toasts, but the player STAYS
///      in the room to keep exploring.
///   3. The player leaves through the ReturnPortal, which calls <see cref="LeaveMission"/>;
///      we load the island and drop them where they left.
///   4. The main menu uses <see cref="NewGame"/> / <see cref="ContinueGame"/>.
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Tooltip("Scene name of the main island. Must be in File > Build Settings.")]
    public string mainSceneName = "MainScene";

    [Tooltip("Scene name of the main menu. Must be in File > Build Settings.")]
    public string menuSceneName = "MenuScene";

    private string currentMissionId;

    // A single "put the player here after the next island load" mechanism, shared by
    // mission-return and Continue.
    private bool hasReposition;
    private Vector3 repositionPos;
    private Quaternion repositionRot;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance == null)
            new GameObject("GameFlowManager (auto)").AddComponent<GameFlowManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>The active player, tag-independent (see <see cref="PlayerMovement.Current"/>).</summary>
    public static Transform FindPlayer()
    {
        if (PlayerMovement.Current != null) return PlayerMovement.Current.transform;
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        return go != null ? go.transform : null;
    }

    // ── Menu entry points ─────────────────────────────────────────────────────
    public void NewGame()
    {
        if (GameProgress.Instance != null) GameProgress.Instance.NewGame();
        currentMissionId = null;
        hasReposition = false;            // spawn at the scene's authored position
        UIBlocker.Reset();
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainSceneName);
    }

    public void ContinueGame()
    {
        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.LoadFromDisk();
            if (GameProgress.Instance.TryGetSavedPlayerPose(out Vector3 pos, out Quaternion rot))
            {
                repositionPos = pos;
                repositionRot = rot;
                hasReposition = true;
            }
        }
        currentMissionId = null;
        UIBlocker.Reset();
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainSceneName);
    }

    public void LoadMainMenu()
    {
        currentMissionId = null;
        hasReposition = false;
        UIBlocker.Reset();
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    // ── Mission round-trip ────────────────────────────────────────────────────
    /// <summary>Remember the player's spot, then load a mission scene.</summary>
    public void StartMission(string sceneName, string missionId)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[GameFlow] StartMission called with no scene name.");
            return;
        }

        currentMissionId = missionId;

        Transform player = FindPlayer();
        if (player != null)
        {
            repositionPos = player.position;
            repositionRot = Quaternion.Euler(0f, player.eulerAngles.y, 0f);
            hasReposition = true;
        }

        UIBlocker.Reset();
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Record the active mission as done, award its collectible and toast — but stay
    /// in the room so the player can keep exploring. Auto-saves (via GameProgress).
    /// </summary>
    public void MarkCurrentMissionComplete()
    {
        if (string.IsNullOrEmpty(currentMissionId)) return;

        bool wasUnlocked = GameProgress.Instance != null && GameProgress.Instance.FinalQuestUnlocked;
        bool first = GameProgress.Instance != null && GameProgress.Instance.CompleteMission(currentMissionId);
        bool nowUnlocked = GameProgress.Instance != null && GameProgress.Instance.FinalQuestUnlocked;

        Collectible reward = Catalog.ForMission(currentMissionId);
        if (first)
        {
            const int xpGain = 100;
            int newXp = GameProgress.Instance != null ? GameProgress.Instance.AddXp(xpGain) : xpGain;
            CelebrationManager.Get().PlayMissionComplete(reward != null ? reward.displayName : null, xpGain, newXp);
            ScreenPrompt.Toast("Use the door to return when you're ready.", 4.5f);
        }
        else
        {
            ScreenPrompt.Toast("Completed again!  Use the door to return.", 4.5f);
        }

        Debug.Log($"[GameFlow] Mission complete: {currentMissionId}");

        if (!wasUnlocked && nowUnlocked)
            StartCoroutine(ToastDelayed(
                "All treasures collected!  The little guide in the city now has a final challenge for you.", 5.5f, 6f));
    }

    /// <summary>Leave the current mission and go back to the island.</summary>
    public void LeaveMission()
    {
        currentMissionId = null;
        // hasReposition is already set from StartMission to the spot we left.
        UIBlocker.Reset();
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainSceneName);
    }

    public bool IsComplete(string missionId) =>
        GameProgress.Instance != null && GameProgress.Instance.IsMissionComplete(missionId);

    // ── Scene-load repositioning ──────────────────────────────────────────────
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UIBlocker.Reset();
        Time.timeScale = 1f;

        if (scene.name == mainSceneName && hasReposition)
            StartCoroutine(RepositionPlayerNextFrame());
    }

    private IEnumerator RepositionPlayerNextFrame()
    {
        // Wait one frame so the island's player has run its own Start() first.
        yield return null;

        Transform player = FindPlayer();
        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;    // CharacterController fights direct moves
            player.SetPositionAndRotation(repositionPos, repositionRot);
            if (cc != null) cc.enabled = true;

            // Persist the island position so Continue resumes here next time.
            if (GameProgress.Instance != null) GameProgress.Instance.Save(player);
        }
        hasReposition = false;
    }

    private IEnumerator ToastDelayed(string text, float seconds, float duration)
    {
        yield return new WaitForSeconds(seconds);
        ScreenPrompt.Toast(text, duration);
    }
}
