using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// A reusable "read about this object" popup. Self-building (no prefab needed),
/// works in any scene, and is shown by <see cref="InfoObject"/>. Close with
/// E / Esc / Enter or the Close button.
/// </summary>
public class InfoPopup : MonoBehaviour
{
    public static bool IsOpen { get; private set; }
    private static InfoPopup instance;

    private GameObject root;    // the whole canvas (dim + panel); toggled on open/close
    private GameObject panel;
    private TextMeshProUGUI titleText, bodyText;
    private int openedFrame = -1;

    public static InfoPopup Get()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<InfoPopup>();
            if (instance == null)
                instance = new GameObject("InfoPopup").AddComponent<InfoPopup>();
        }
        return instance;
    }

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        IsOpen = false;   // defensive: clear any stale static from a previous scene
        BuildUI();
    }

    public void Show(string title, string body)
    {
        UIKit.EnsureEventSystem();
        titleText.text = title;
        bodyText.text = body;
        root.SetActive(true);
        IsOpen = true;
        openedFrame = Time.frameCount;
        UIBlocker.Push();   // also shows the cursor
    }

    void Update()
    {
        if (!IsOpen) return;
        if (Time.frameCount == openedFrame) return;   // ignore the key that opened us

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape) ||
            Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            Close();
    }

    public void Close()
    {
        IsOpen = false;
        root.SetActive(false);
        UIBlocker.Pop();   // also hides the cursor
    }

    private void BuildUI()
    {
        Canvas canvas = UIKit.Canvas("InfoCanvas", 28, transform);
        root = canvas.gameObject;
        UIKit.Panel(canvas.transform, "Dim", Vector2.zero, Vector2.one, UITheme.Dim);

        panel = UIKit.Panel(canvas.transform, "Panel",
            new Vector2(0.22f, 0.24f), new Vector2(0.78f, 0.76f), UITheme.PanelBg).gameObject;

        UIKit.Panel(panel.transform, "Accent",
            new Vector2(0f, 0.86f), new Vector2(1f, 0.865f), UITheme.Accent);

        titleText = UIKit.Label(panel.transform, new Vector2(0.05f, 0.86f), new Vector2(0.95f, 0.97f),
            38, FontStyles.Bold, TextAlignmentOptions.Left, UITheme.Title);

        bodyText = UIKit.Label(panel.transform, new Vector2(0.05f, 0.18f), new Vector2(0.95f, 0.84f),
            26, FontStyles.Normal, TextAlignmentOptions.TopLeft, UITheme.TextLight);
        bodyText.enableWordWrapping = true;
        bodyText.enableAutoSizing = true;
        bodyText.fontSizeMin = 18;
        bodyText.fontSizeMax = 28;
        bodyText.lineSpacing = 8f;

        UIKit.Label(panel.transform, new Vector2(0.05f, 0.04f), new Vector2(0.6f, 0.14f),
            20, FontStyles.Italic, TextAlignmentOptions.Left, UITheme.TextSubtle)
            .text = "[E] / [Esc] to close";

        // Close button.
        Image btn = UIKit.Panel(panel.transform, "CloseButton",
            new Vector2(0.78f, 0.03f), new Vector2(0.95f, 0.15f), UITheme.ButtonAction);
        btn.gameObject.AddComponent<Button>().onClick.AddListener(Close);
        UIKit.Label(btn.transform, Vector2.zero, Vector2.one,
            24, FontStyles.Bold, TextAlignmentOptions.Center, UITheme.TextLight).text = "Close";

        root.SetActive(false);
    }
}
