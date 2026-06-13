using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// The main-menu screen. Lives on a GameObject in the MenuScene and builds its UI
/// in code: Continue (enabled only when a save exists, with its timestamp), New Game
/// (asks before overwriting a save), Settings and Quit. The menu is silent — music
/// only ever plays from the theatre jukebox.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    private GameObject mainButtons;
    private GameObject confirmPanel;

    void Start()
    {
        UIKit.EnsureEventSystem();
        UIBlocker.Reset();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        BuildUI();

        // The menu is silent: music only plays from the theatre jukebox, so make sure
        // nothing is left ringing if we arrived here straight from a scene.
        if (AudioManager.Instance != null) AudioManager.Instance.StopMusic();
    }

    void Update()
    {
        // The menu always wants a visible cursor (settings manages its own while open).
        if (!SettingsMenu.IsOpen)
        {
            if (!Cursor.visible) Cursor.visible = true;
            if (Cursor.lockState != CursorLockMode.None) Cursor.lockState = CursorLockMode.None;
        }
    }

    // ── Actions ───────────────────────────────────────────────────────────────
    private void OnContinue()
    {
        if (!SaveSystem.HasSave()) return;
        if (GameFlowManager.Instance != null) GameFlowManager.Instance.ContinueGame();
    }

    private void OnNewGame()
    {
        if (SaveSystem.HasSave()) { ShowConfirm(true); return; }
        StartNewGame();
    }

    private void StartNewGame()
    {
        if (GameFlowManager.Instance != null) GameFlowManager.Instance.NewGame();
    }

    private void OnSettings() => SettingsMenu.Get().Open(null);

    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ShowConfirm(bool show)
    {
        confirmPanel.SetActive(show);
        mainButtons.SetActive(!show);
    }

    // ── UI construction ───────────────────────────────────────────────────────
    private void BuildUI()
    {
        Canvas canvas = UIKit.Canvas("MainMenuCanvas", 10, transform);
        UIKit.Panel(canvas.transform, "Bg", Vector2.zero, Vector2.one, new Color(0.05f, 0.06f, 0.12f, 1f));

        // Title block
        UIKit.Label(canvas.transform, new Vector2(0.1f, 0.74f), new Vector2(0.9f, 0.9f),
            72, FontStyles.Bold, TextAlignmentOptions.Center, UITheme.Title).text = "Primii Pași în România";
        UIKit.Label(canvas.transform, new Vector2(0.1f, 0.68f), new Vector2(0.9f, 0.74f),
            30, FontStyles.Italic, TextAlignmentOptions.Center, UITheme.TextSubtle)
            .text = "An educational journey through Romania";

        // Main buttons column
        mainButtons = new GameObject("MainButtons");
        mainButtons.transform.SetParent(canvas.transform, false);
        UIKit.Rect(mainButtons, new Vector2(0.37f, 0.16f), new Vector2(0.63f, 0.6f));
        VerticalLayoutGroup vlg = mainButtons.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 16; vlg.childControlWidth = true; vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = true;

        bool hasSave = SaveSystem.HasSave();
        string contLabel = hasSave ? "Continue" : "Continue (no save)";
        Button cont = Col(mainButtons.transform, contLabel, hasSave ? UITheme.ButtonAction : UITheme.ButtonNormal, OnContinue);
        cont.interactable = hasSave;
        if (hasSave)
        {
            string when = SaveSystem.SavedAtDisplay();
            if (!string.IsNullOrEmpty(when))
                UIKit.Label(cont.transform, new Vector2(0f, -0.45f), new Vector2(1f, 0.05f),
                    16, FontStyles.Italic, TextAlignmentOptions.Center, UITheme.TextSubtle).text = $"Saved: {when}";
        }

        Col(mainButtons.transform, "New Game", UITheme.ButtonNormal, OnNewGame);
        Col(mainButtons.transform, "Settings", UITheme.ButtonNormal, OnSettings);
        Col(mainButtons.transform, "Quit", new Color(0.45f, 0.20f, 0.22f, 1f), OnQuit);

        // Confirm-overwrite panel (hidden by default)
        confirmPanel = UIKit.Panel(canvas.transform, "Confirm",
            new Vector2(0.3f, 0.32f), new Vector2(0.7f, 0.6f), UITheme.PanelBg).gameObject;
        UIKit.Label(confirmPanel.transform, new Vector2(0.06f, 0.55f), new Vector2(0.94f, 0.92f),
            28, FontStyles.Bold, TextAlignmentOptions.Center, UITheme.TextLight)
            .text = "Start a new game?\nThis overwrites your existing save.";

        Button yes = UIKit.Button(confirmPanel.transform, "Yes, new game",
            new Color(0.45f, 0.20f, 0.22f, 1f), StartNewGame, 24);
        UIKit.Rect(yes.gameObject, new Vector2(0.08f, 0.12f), new Vector2(0.49f, 0.4f));
        Button no = UIKit.Button(confirmPanel.transform, "Cancel", UITheme.ButtonAction, () => ShowConfirm(false), 24);
        UIKit.Rect(no.gameObject, new Vector2(0.51f, 0.12f), new Vector2(0.92f, 0.4f));

        confirmPanel.SetActive(false);

        UIKit.Label(canvas.transform, new Vector2(0.02f, 0.01f), new Vector2(0.98f, 0.05f),
            18, FontStyles.Italic, TextAlignmentOptions.Center, UITheme.TextSubtle)
            .text = "Made with Unity · Romanian culture, language, geography, history & music";
    }

    private Button Col(Transform parent, string label, Color color, System.Action onClick)
    {
        Button b = UIKit.Button(parent, label, color, onClick, 30);
        b.gameObject.AddComponent<LayoutElement>().preferredHeight = 66;
        return b;
    }
}
