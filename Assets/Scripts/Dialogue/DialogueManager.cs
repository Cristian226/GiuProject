using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// A small, persistent dialogue box. Shows one or more pages of text under a
/// speaker name; the player presses Enter to advance and Esc to dismiss. The final
/// page can carry an "accept" action — used by <see cref="MissionGiver"/> to start
/// a mission. Builds its own UI in code and freezes the player while open.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Tooltip("Optional TMP Font Asset to override the default dialogue font.")]
    public TMP_FontAsset dialogueFont;

    private GameObject panel;
    private TextMeshProUGUI speakerText, bodyText, instrText;

    private string[] pages;
    private int pageIndex;
    private Action onAccept, onCancel;
    private bool isOpen;
    private bool blockerPushed;

    public bool IsOpen => isOpen;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
    }

    /// <summary>Show a single message (optionally with an Accept/Cancel choice).</summary>
    public void Show(string speaker, string message, Action onAccept = null, Action onCancel = null)
        => Show(speaker, new[] { message }, onAccept, onCancel);

    /// <summary>Show a sequence of pages; Enter advances, the last page may accept.</summary>
    public void Show(string speaker, string[] pages, Action onAccept = null, Action onCancel = null)
    {
        if (pages == null || pages.Length == 0) return;

        this.pages = pages;
        this.onAccept = onAccept;
        this.onCancel = onCancel;
        pageIndex = 0;

        speakerText.text = speaker;
        panel.SetActive(true);
        isOpen = true;
        PushBlocker();   // also shows the cursor
        ShowPage();
    }

    void Update()
    {
        if (!isOpen) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (pageIndex < pages.Length - 1) { pageIndex++; ShowPage(); }
            else { Action accept = onAccept; Close(); accept?.Invoke(); }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Action cancel = onCancel; Close(); cancel?.Invoke();
        }
    }

    private void ShowPage()
    {
        bodyText.text = pages[pageIndex];
        bool last = pageIndex >= pages.Length - 1;

        if (last)
            instrText.text = onAccept != null
                ? "<color=#44FF88>[Enter]</color> Accept     <color=#FF6666>[Esc]</color> Cancel"
                : "<color=#44FF88>[Enter]</color> Close";
        else
            instrText.text = "<color=#44FF88>[Enter]</color> Continue     <color=#FF6666>[Esc]</color> Skip";
    }

    public void Close()
    {
        panel.SetActive(false);
        isOpen = false;
        PopBlocker();   // also hides the cursor
    }

    // Freeze player movement while the box is up. Guarded so it can't stack.
    private void PushBlocker()
    {
        if (blockerPushed) return;
        blockerPushed = true;
        UIBlocker.Push();
    }

    private void PopBlocker()
    {
        if (!blockerPushed) return;
        blockerPushed = false;
        UIBlocker.Pop();
    }

    private void BuildUI()
    {
        Canvas canvas = UIKit.Canvas("DialogueCanvas", 20, transform);

        // Panel across the bottom of the screen.
        panel = UIKit.Panel(canvas.transform, "DialoguePanel",
            new Vector2(0.1f, 0.04f), new Vector2(0.9f, 0.46f),
            new Color(0.05f, 0.05f, 0.1f, 0.92f)).gameObject;

        // Subtle 2px border behind the panel.
        Image border = UIKit.Panel(panel.transform, "Border", Vector2.zero, Vector2.one,
            new Color(0.4f, 0.4f, 0.6f, 0.6f));
        border.rectTransform.offsetMin = new Vector2(-2, -2);
        border.rectTransform.offsetMax = new Vector2(2, 2);
        border.transform.SetAsFirstSibling();

        speakerText = UIKit.Label(panel.transform, new Vector2(0.04f, 0.82f), new Vector2(0.6f, 0.97f),
            30, FontStyles.Bold, TextAlignmentOptions.Left, new Color(0.4f, 1f, 0.7f));

        bodyText = UIKit.Label(panel.transform, new Vector2(0.04f, 0.30f), new Vector2(0.96f, 0.82f),
            30, FontStyles.Normal, TextAlignmentOptions.TopLeft, Color.white);
        bodyText.enableWordWrapping = true;
        bodyText.enableAutoSizing = true;
        bodyText.fontSizeMin = 22;
        bodyText.fontSizeMax = 36;
        bodyText.lineSpacing = 10f;

        instrText = UIKit.Label(panel.transform, new Vector2(0.04f, 0.03f), new Vector2(0.96f, 0.28f),
            24, FontStyles.Normal, TextAlignmentOptions.BottomLeft, Color.white);

        if (dialogueFont != null) { bodyText.font = dialogueFont; speakerText.font = dialogueFont; }

        panel.SetActive(false);
    }
}
