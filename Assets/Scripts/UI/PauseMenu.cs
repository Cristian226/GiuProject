using UnityEngine;
using UnityEngine.UI;
using TMPro;

// In-game pause menu. A persistent, auto-created overlay that watches for the Pause
// key during gameplay and offers Resume · Settings · Save · Main Menu · Quit. Pausing
// sets Time.timeScale = 0 and shows the cursor. Yields the key while another panel is
// open, and stays dormant in the main-menu scene (where there is no player).
public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }
    private static PauseMenu instance;

    private GameObject root;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance == null)
            new GameObject("PauseMenu (auto)").AddComponent<PauseMenu>();
    }

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        IsPaused = false;
        DontDestroyOnLoad(gameObject);
        BuildUI();
    }

    void Update()
    {
        if (IsPaused)
        {
            if (SettingsMenu.IsOpen) return;   // settings owns the key now
            if (GameSettings.Pressed(GameAction.Pause)) Resume();
            return;
        }

        if (!CanOpen()) return;
        if (GameSettings.Pressed(GameAction.Pause)) Pause();
    }

    private bool CanOpen()
    {
        if (UIBlocker.IsBlocked) return false;             // a dialogue / popup is up
        if (PlayerMovement.Current == null) return false;  // no gameplay (e.g. main menu)
        return true;
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        UIKit.EnsureEventSystem();
        root.SetActive(true);
        UIBlocker.Push();
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        root.SetActive(false);
        UIBlocker.Pop();
    }

    // ── Button actions ──
    private void OpenSettings() => SettingsMenu.Get().Open(null);

    private void SaveGame()
    {
        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.Save(PlayerMovement.Current != null ? PlayerMovement.Current.transform : null);
            ScreenPrompt.Toast("Game saved.", 2.5f);
        }
    }

    private void ToMainMenu()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        root.SetActive(false);
        UIBlocker.Reset();
        if (GameFlowManager.Instance != null) GameFlowManager.Instance.LoadMainMenu();
    }

    private void QuitGame()
    {
        if (GameProgress.Instance != null)
            GameProgress.Instance.Save(PlayerMovement.Current != null ? PlayerMovement.Current.transform : null);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── UI construction ──
    private void BuildUI()
    {
        Canvas canvas = UIKit.Canvas("PauseCanvas", 35, transform);
        root = canvas.gameObject;
        UIKit.Panel(canvas.transform, "Dim", Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 0.7f));

        GameObject panel = UIKit.Panel(canvas.transform, "Panel",
            new Vector2(0.36f, 0.24f), new Vector2(0.64f, 0.80f), UITheme.PanelBg).gameObject;

        UIKit.Label(panel.transform, new Vector2(0.06f, 0.86f), new Vector2(0.94f, 0.98f),
            44, FontStyles.Bold, TextAlignmentOptions.Center, UITheme.Title).text = "Paused";

        GameObject list = new GameObject("Buttons");
        list.transform.SetParent(panel.transform, false);
        UIKit.Rect(list, new Vector2(0.1f, 0.06f), new Vector2(0.9f, 0.82f));
        VerticalLayoutGroup vlg = list.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 14; vlg.childControlWidth = true; vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = true;

        MenuButton(list.transform, "Resume", UITheme.ButtonAction, Resume);
        MenuButton(list.transform, "Settings", UITheme.ButtonNormal, OpenSettings);
        MenuButton(list.transform, "Save Game", UITheme.ButtonNormal, SaveGame);
        MenuButton(list.transform, "Main Menu", UITheme.ButtonNormal, ToMainMenu);
        MenuButton(list.transform, "Quit", new Color(0.45f, 0.20f, 0.22f, 1f), QuitGame);

        root.SetActive(false);
    }

    private void MenuButton(Transform parent, string label, Color color, System.Action onClick)
    {
        Button b = UIKit.Button(parent, label, color, onClick, 28);
        b.gameObject.AddComponent<LayoutElement>().preferredHeight = 64;
    }
}
