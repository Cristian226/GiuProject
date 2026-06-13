using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Self-contained UI engine that runs a list of <see cref="MissionStep"/>s and
/// reports success when the player clears them all. Builds its entire interface
/// in code (same approach as <see cref="DialogueManager"/>) so a mission scene
/// only needs a station object — no canvas/prefab setup required.
///
/// Call <see cref="Get"/> then <see cref="Run"/>.
/// </summary>
public class MissionMiniGame : MonoBehaviour
{
    public static bool IsOpen { get; private set; }
    private static MissionMiniGame instance;

    // ── UI refs ───────────────────────────────────────────────
    private GameObject root;   // the whole canvas (dim + panel); toggled on open/close
    private GameObject panel;
    private TextMeshProUGUI titleText, counterText, promptText, feedbackText, actionLabel;
    private RectTransform contentArea;
    private GameObject actionButtonGO;
    private Button actionButton;

    // ── run state ─────────────────────────────────────────────
    private List<MissionStep> steps;
    private int index;
    private Action onSuccess, onAbort;
    private bool finished;
    private bool stepSolved;

    private readonly HashSet<int> multiSelected = new HashSet<int>();
    private readonly List<Image> optionImages = new List<Image>();
    private List<string> orderWorking;
    private List<MissionOption> multiOptions;   // shuffled options for the current MultiSelect step

    /// <summary>Find the existing mini-game in the scene or create one on demand.</summary>
    public static MissionMiniGame Get()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<MissionMiniGame>();
            if (instance == null)
                instance = new GameObject("MissionMiniGame").AddComponent<MissionMiniGame>();
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

    public void Run(string missionTitle, List<MissionStep> steps, Action onSuccess, Action onAbort)
    {
        if (steps == null || steps.Count == 0) { onSuccess?.Invoke(); return; }

        UIKit.EnsureEventSystem();
        this.steps = steps;
        this.onSuccess = onSuccess;
        this.onAbort = onAbort;
        index = 0;
        finished = false;

        root.SetActive(true);
        IsOpen = true;
        UIBlocker.Push();   // also shows the cursor

        titleHeader.text = missionTitle;
        RenderStep();
    }

    void Update()
    {
        if (!IsOpen || finished) return;
        if (Input.GetKeyDown(KeyCode.Escape)) Finish(false);
    }

    // ==========================================================
    //  Step rendering
    // ==========================================================
    private void RenderStep()
    {
        stepSolved = false;
        multiSelected.Clear();
        optionImages.Clear();
        ClearContent();
        feedbackText.text = "";

        MissionStep step = steps[index];
        titleText.text = step.title;
        counterText.text = $"Step {index + 1} / {steps.Count}";
        promptText.text = step.prompt;

        switch (step.kind)
        {
            case MissionStepKind.Info:
                SetAction("Continue", Advance);
                break;

            case MissionStepKind.SingleChoice:
                RenderSingle(step);
                HideAction();
                break;

            case MissionStepKind.MultiSelect:
                RenderMulti(step);
                SetAction("Check answer", () => OnMultiSubmit(step));
                break;

            case MissionStepKind.Ordering:
                RenderOrdering(step);
                SetAction("Check order", () => OnOrderSubmit(step));
                break;
        }
    }

    private void RenderSingle(MissionStep step)
    {
        // Shuffle so the correct answer isn't always in the same slot.
        foreach (MissionOption opt in ShuffledOptions(step.options))
        {
            MissionOption captured = opt;
            GameObject btn = MakeButton(contentArea, opt.label, UITheme.ButtonNormal);
            btn.GetComponent<Button>().onClick.AddListener(() => OnSinglePick(step, captured));
        }
    }

    private void OnSinglePick(MissionStep step, MissionOption opt)
    {
        if (stepSolved) return;
        if (opt.correct)
        {
            Solve(step);
        }
        else
        {
            string msg = string.IsNullOrEmpty(step.wrongFeedback)
                ? "Not quite — try another answer."
                : step.wrongFeedback;
            feedbackText.SetText($"<color=#FF8073>{msg}</color>");
        }
    }

    private void RenderMulti(MissionStep step)
    {
        // Shuffle once; both rendering and checking use this same order.
        multiOptions = ShuffledOptions(step.options);
        for (int i = 0; i < multiOptions.Count; i++)
        {
            int captured = i;
            GameObject btn = MakeButton(contentArea, multiOptions[i].label, UITheme.ButtonNormal);
            optionImages.Add(btn.GetComponent<Image>());
            btn.GetComponent<Button>().onClick.AddListener(() => ToggleMulti(captured));
        }
    }

    private void ToggleMulti(int i)
    {
        if (stepSolved) return;
        if (multiSelected.Contains(i)) multiSelected.Remove(i);
        else multiSelected.Add(i);
        optionImages[i].color = multiSelected.Contains(i) ? UITheme.ButtonSelected : UITheme.ButtonNormal;
    }

    private void OnMultiSubmit(MissionStep step)
    {
        if (stepSolved) return;
        for (int i = 0; i < multiOptions.Count; i++)
        {
            bool picked = multiSelected.Contains(i);
            if (picked != multiOptions[i].correct)
            {
                feedbackText.SetText("<color=#FF8073>Not quite — review which items belong.</color>");
                return;
            }
        }
        Solve(step);
    }

    private void RenderOrdering(MissionStep step)
    {
        orderWorking = new List<string>(step.orderItems);
        Shuffle(orderWorking, step.orderItems);
        RenderOrderRows();
    }

    private void RenderOrderRows()
    {
        ClearContent();
        for (int i = 0; i < orderWorking.Count; i++)
        {
            int row = i;
            GameObject rowGO = MakeRow(contentArea);

            MakeRowLabel(rowGO.transform, $"{i + 1}.  {orderWorking[i]}");
            GameObject up = MakeMiniButton(rowGO.transform, "Up");
            GameObject down = MakeMiniButton(rowGO.transform, "Dn");
            up.GetComponent<Button>().onClick.AddListener(() => MoveRow(row, -1));
            down.GetComponent<Button>().onClick.AddListener(() => MoveRow(row, +1));
        }
    }

    private void MoveRow(int from, int dir)
    {
        if (stepSolved) return;
        int to = from + dir;
        if (to < 0 || to >= orderWorking.Count) return;
        (orderWorking[from], orderWorking[to]) = (orderWorking[to], orderWorking[from]);
        RenderOrderRows();
    }

    private void OnOrderSubmit(MissionStep step)
    {
        if (stepSolved) return;
        for (int i = 0; i < step.orderItems.Count; i++)
        {
            if (orderWorking[i] != step.orderItems[i])
            {
                feedbackText.SetText("<color=#FF8073>Not in the right order yet — keep going.</color>");
                return;
            }
        }
        Solve(step);
    }

    // Mark the current step solved: show the teaching note and a Continue button.
    private void Solve(MissionStep step)
    {
        stepSolved = true;
        string body = "<color=#73FF99><b>Correct!</b></color>";
        if (!string.IsNullOrEmpty(step.explanation))
            body += "\n" + step.explanation;
        feedbackText.text = body;
        SetAction(index == steps.Count - 1 ? "Finish" : "Continue", Advance);
    }

    private void Advance()
    {
        index++;
        if (index >= steps.Count) Finish(true);
        else RenderStep();
    }

    private void Finish(bool success)
    {
        if (finished) return;
        finished = true;
        IsOpen = false;
        root.SetActive(false);
        UIBlocker.Pop();   // also hides the cursor

        if (success) onSuccess?.Invoke();
        else onAbort?.Invoke();
    }

    // ==========================================================
    //  UI construction
    // ==========================================================
    private TextMeshProUGUI titleHeader; // big mission name at the very top

    private void BuildUI()
    {
        Canvas canvas = UIKit.Canvas("MissionCanvas", 30, transform);
        root = canvas.gameObject;
        UIKit.Panel(canvas.transform, "Dim", Vector2.zero, Vector2.one, UITheme.Dim);

        panel = UIKit.Panel(canvas.transform, "Panel",
            new Vector2(0.14f, 0.10f), new Vector2(0.86f, 0.92f), UITheme.PanelBg).gameObject;

        titleHeader = UIKit.Label(panel.transform, new Vector2(0.04f, 0.92f), new Vector2(0.96f, 0.99f),
            38, FontStyles.Bold, TextAlignmentOptions.Left, UITheme.Title);

        titleText = UIKit.Label(panel.transform, new Vector2(0.04f, 0.85f), new Vector2(0.6f, 0.915f),
            26, FontStyles.Bold, TextAlignmentOptions.Left, new Color(0.7f, 0.75f, 0.9f));

        counterText = UIKit.Label(panel.transform, new Vector2(0.6f, 0.85f), new Vector2(0.96f, 0.915f),
            24, FontStyles.Normal, TextAlignmentOptions.Right, new Color(0.6f, 0.65f, 0.8f));

        promptText = UIKit.Label(panel.transform, new Vector2(0.04f, 0.72f), new Vector2(0.96f, 0.85f),
            30, FontStyles.Normal, TextAlignmentOptions.TopLeft, UITheme.TextLight);
        promptText.enableWordWrapping = true;

        // Content area (vertical stack of options / rows).
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(panel.transform, false);
        contentArea = UIKit.Rect(contentGO, new Vector2(0.04f, 0.17f), new Vector2(0.96f, 0.71f));
        VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 7;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        feedbackText = UIKit.Label(panel.transform, new Vector2(0.04f, 0.03f), new Vector2(0.66f, 0.16f),
            24, FontStyles.Normal, TextAlignmentOptions.TopLeft, UITheme.TextLight);
        feedbackText.enableWordWrapping = true;

        // Action button (Continue / Check answer / Finish).
        Image actionImg = UIKit.Panel(panel.transform, "ActionButton",
            new Vector2(0.7f, 0.04f), new Vector2(0.96f, 0.15f), UITheme.ButtonAction);
        actionButtonGO = actionImg.gameObject;
        actionButton = actionButtonGO.AddComponent<Button>();
        actionLabel = UIKit.Label(actionButtonGO.transform, Vector2.zero, Vector2.one,
            26, FontStyles.Bold, TextAlignmentOptions.Center, UITheme.TextLight);

        UIKit.Label(panel.transform, new Vector2(0.04f, 0.0f), new Vector2(0.4f, 0.035f),
            20, FontStyles.Italic, TextAlignmentOptions.Left, UITheme.TextSubtle)
            .text = "[Esc] leave mini-game";

        root.SetActive(false);
    }

    private void SetAction(string label, Action onClick)
    {
        actionButtonGO.SetActive(true);
        actionLabel.text = label;
        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(() => onClick());
    }

    private void HideAction() => actionButtonGO.SetActive(false);

    private void ClearContent()
    {
        for (int i = contentArea.childCount - 1; i >= 0; i--)
            Destroy(contentArea.GetChild(i).gameObject);
    }

    // ── element factories ─────────────────────────────────────
    private GameObject MakeButton(Transform parent, string text, Color color)
    {
        Image img = UIKit.Panel(parent, "Option", Vector2.zero, Vector2.one, color);
        img.gameObject.AddComponent<Button>();
        LayoutElement le = img.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = 58;
        le.flexibleWidth = 1;

        TextMeshProUGUI t = UIKit.Label(img.transform, Vector2.zero, Vector2.one,
            24, FontStyles.Normal, TextAlignmentOptions.Center, UITheme.TextLight);
        t.margin = new Vector4(16, 0, 16, 0);
        t.text = text;
        t.enableWordWrapping = true;
        return img.gameObject;
    }

    private GameObject MakeRow(Transform parent)
    {
        GameObject row = new GameObject("Row");
        row.transform.SetParent(parent, false);
        HorizontalLayoutGroup h = row.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 6;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandHeight = true;
        row.AddComponent<LayoutElement>().preferredHeight = 58;
        return row;
    }

    private void MakeRowLabel(Transform parent, string text)
    {
        Image img = UIKit.Panel(parent, "RowLabel", Vector2.zero, Vector2.one, UITheme.ButtonNormal);
        img.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        TextMeshProUGUI t = UIKit.Label(img.transform, Vector2.zero, Vector2.one,
            24, FontStyles.Normal, TextAlignmentOptions.Left, UITheme.TextLight);
        t.margin = new Vector4(16, 0, 4, 0);
        t.text = text;
    }

    private GameObject MakeMiniButton(Transform parent, string text)
    {
        Image img = UIKit.Panel(parent, "MiniBtn", Vector2.zero, Vector2.one, UITheme.ButtonAction);
        img.gameObject.AddComponent<Button>();
        LayoutElement le = img.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = le.minWidth = 72;
        UIKit.Label(img.transform, Vector2.zero, Vector2.one,
            24, FontStyles.Bold, TextAlignmentOptions.Center, UITheme.TextLight).text = text;
        return img.gameObject;
    }

    // Fisher-Yates copy of the options, so the correct answer lands in a random slot.
    private static List<MissionOption> ShuffledOptions(List<MissionOption> src)
    {
        List<MissionOption> list = new List<MissionOption>(src);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }

    // Fisher-Yates; guarantee the shuffle isn't already the correct order.
    private static void Shuffle(List<string> list, List<string> correct)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        bool same = true;
        for (int i = 0; i < list.Count; i++)
            if (list[i] != correct[i]) { same = false; break; }
        if (same && list.Count > 1) (list[0], list[1]) = (list[1], list[0]);
    }
}
