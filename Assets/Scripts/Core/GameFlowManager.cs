using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }
    public string mainSceneName = "MainScene";
    public string menuSceneName = "MenuScene";
    private string currentMissionId;
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

    public static Transform FindPlayer()
    {
        if (PlayerMovement.Current != null) return PlayerMovement.Current.transform;
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        return go != null ? go.transform : null;
    }

    public void NewGame()
    {
        if (GameProgress.Instance != null) GameProgress.Instance.NewGame();
        currentMissionId = null;
        hasReposition = false;
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

    // Remember the player's spot, then load a mission scene.
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UIBlocker.Reset();
        Time.timeScale = 1f;

        if (scene.name == mainSceneName && hasReposition)
            StartCoroutine(RepositionPlayerNextFrame());
    }

    private IEnumerator RepositionPlayerNextFrame()
    {
        yield return null;

        Transform player = FindPlayer();
        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;   // CharacterController fights direct moves
            player.SetPositionAndRotation(repositionPos, repositionRot);
            if (cc != null) cc.enabled = true;

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
