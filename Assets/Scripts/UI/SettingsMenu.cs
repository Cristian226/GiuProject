using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    public static bool IsOpen { get; private set; }
    private static SettingsMenu instance;

    private GameObject root;
    private RectTransform contentArea;
    private readonly List<Button> tabButtons = new List<Button>();
    private TextMeshProUGUI hintText;

    private enum Tab { Controls, Mouse, Audio, Graphics }
    private Tab currentTab = Tab.Controls;

    private GameAction? rebinding;          // the action currently waiting for a key
    private TextMeshProUGUI rebindLabel;    // its on-screen key label
    private Action onClose;

    private static KeyCode[] scanKeys;      // keys offered to the rebind listener

    public static SettingsMenu Get()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<SettingsMenu>();
            if (instance == null)
                instance = new GameObject("SettingsMenu").AddComponent<SettingsMenu>();
        }
        return instance;
    }

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        IsOpen = false;
        BuildScanKeys();
        BuildUI();
    }

    public void Open(Action closeCallback)
    {
        onClose = closeCallback;
        UIKit.EnsureEventSystem();
        root.SetActive(true);
        IsOpen = true;
        UIBlocker.Push();
        rebinding = null;
        RenderTab(currentTab);
    }

    public void Close()
    {
        rebinding = null;
        IsOpen = false;
        root.SetActive(false);
        UIBlocker.Pop();
        Action cb = onClose; onClose = null;
        cb?.Invoke();
    }

    void Update()
    {
        if (!IsOpen) return;

        if (rebinding.HasValue)
        {
            CaptureRebind();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    private void CaptureRebind()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { CancelRebind(); return; }

        foreach (KeyCode kc in scanKeys)
        {
            if (Input.GetKeyDown(kc))
            {
                GameSettings.SetKey(rebinding.Value, kc);
                rebinding = null;
                rebindLabel = null;
                RenderTab(currentTab);
                return;
            }
        }
    }

    private void BeginRebind(GameAction action, TextMeshProUGUI label)
    {
        rebinding = action;
        rebindLabel = label;
        if (label != null) { label.text = "Press a key…"; label.color = UITheme.Accent; }
        if (hintText != null) hintText.text = "Press any key to bind, or [Esc] to cancel.";
    }

    private void CancelRebind()
    {
        rebinding = null;
        rebindLabel = null;
        RenderTab(currentTab);
    }

    private static void BuildScanKeys()
    {
        if (scanKeys != null) return;
        var list = new List<KeyCode>();
        foreach (KeyCode kc in Enum.GetValues(typeof(KeyCode)))
        {
            if (kc == KeyCode.None || kc == KeyCode.Escape) continue;
            if (kc >= KeyCode.Mouse0 && kc <= KeyCode.Mouse6) continue;   // don't bind to mouse clicks
            if ((int)kc >= (int)KeyCode.JoystickButton0) continue;        // skip gamepad
            list.Add(kc);
        }
        scanKeys = list.ToArray();
    }

    private void RenderTab(Tab tab)
    {
        currentTab = tab;
        for (int i = 0; i < tabButtons.Count; i++)
        {
            var img = tabButtons[i].targetGraphic as Image;
            if (img != null) img.color = (int)tab == i ? UITheme.ButtonSelected : UITheme.ButtonNormal;
        }

        ClearContent();
        if (hintText != null)
            hintText.text = "Changes save automatically.  [Esc] closes this menu.";

        switch (tab)
        {
            case Tab.Controls: RenderControls(); break;
            case Tab.Mouse:    RenderMouse();    break;
            case Tab.Audio:    RenderAudio();    break;
            case Tab.Graphics: RenderGraphics(); break;
        }
    }

    private void RenderControls()
    {
        KeyRow("Move Forward", GameAction.MoveForward);
        KeyRow("Move Back", GameAction.MoveBack);
        KeyRow("Move Left", GameAction.MoveLeft);
        KeyRow("Move Right", GameAction.MoveRight);
        KeyRow("Sprint", GameAction.Sprint);
        KeyRow("Jump", GameAction.Jump);
        KeyRow("Interact", GameAction.Interact);
        KeyRow("Inventory", GameAction.Inventory);
        KeyRow("Pause", GameAction.Pause);
    }

    private void RenderMouse()
    {
        SliderRow("Mouse Sensitivity", 0.2f, 10f, GameSettings.MouseSensitivity,
            v => v.ToString("0.0"), GameSettings.SetMouseSensitivity);
        ToggleRow("Invert Y Axis", GameSettings.InvertY, GameSettings.SetInvertY);
    }

    private void RenderAudio()
    {
        SliderRow("Master Volume", 0f, 1f, GameSettings.MasterVolume,
            v => Mathf.RoundToInt(v * 100f) + "%", GameSettings.SetMasterVolume);
        SliderRow("Music Volume", 0f, 1f, GameSettings.MusicVolume,
            v => Mathf.RoundToInt(v * 100f) + "%", GameSettings.SetMusicVolume);
        SliderRow("Effects Volume", 0f, 1f, GameSettings.EffectsVolume,
            v => Mathf.RoundToInt(v * 100f) + "%", GameSettings.SetEffectsVolume);
    }

    private void RenderGraphics()
    {
        string[] names = QualitySettings.names;
        ChoiceRow("Quality Preset", names, GameSettings.QualityLevel, GameSettings.SetQualityLevel);
        ToggleRow("Show FPS", GameSettings.ShowFps, GameSettings.SetShowFps);
    }

    private GameObject Row()
    {
        GameObject row = new GameObject("Row");
        row.transform.SetParent(contentArea, false);
        HorizontalLayoutGroup h = row.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 14;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = true;
        h.childAlignment = TextAnchor.MiddleLeft;
        row.AddComponent<LayoutElement>().preferredHeight = 62;
        return row;
    }

    private TextMeshProUGUI RowLabel(Transform parent, string text, float width)
    {
        TextMeshProUGUI t = UIKit.Label(parent, Vector2.zero, Vector2.one,
            26, FontStyles.Normal, TextAlignmentOptions.Left, UITheme.TextLight);
        t.text = text;
        LayoutElement le = t.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = width; le.minWidth = width;
        return t;
    }

    private void KeyRow(string label, GameAction action)
    {
        GameObject row = Row();
        RowLabel(row.transform, label, 360);

        Button b = UIKit.Button(row.transform, GameSettings.KeyLabel(action), UITheme.ButtonNormal, null, 24);
        LayoutElement le = b.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = 220; le.minWidth = 220;
        TextMeshProUGUI lbl = b.GetComponentInChildren<TextMeshProUGUI>();
        b.onClick.AddListener(() => BeginRebind(action, lbl));
    }

    private void SliderRow(string label, float min, float max, float value, Func<float, string> format, Action<float> onChange)
    {
        GameObject row = Row();
        RowLabel(row.transform, label, 360);

        TextMeshProUGUI valueText = UIKit.Label(row.transform, Vector2.zero, Vector2.one,
            24, FontStyles.Bold, TextAlignmentOptions.Center, UITheme.Accent);
        valueText.text = format(value);
        LayoutElement vle = valueText.gameObject.AddComponent<LayoutElement>();
        vle.preferredWidth = 90; vle.minWidth = 90;

        GameObject sliderCell = new GameObject("SliderCell");
        sliderCell.transform.SetParent(row.transform, false);
        sliderCell.AddComponent<RectTransform>();
        LayoutElement sle = sliderCell.AddComponent<LayoutElement>();
        sle.preferredWidth = 320; sle.minWidth = 200; sle.flexibleWidth = 1;

        UIKit.Slider(sliderCell.transform, min, max, value, v =>
        {
            valueText.text = format(v);
            onChange(v);
        });
    }

    private void ToggleRow(string label, bool value, Action<bool> onChange)
    {
        GameObject row = Row();
        RowLabel(row.transform, label, 360);

        bool state = value;
        Button b = UIKit.Button(row.transform, state ? "On" : "Off",
            state ? UITheme.ButtonSelected : UITheme.ButtonNormal, null, 24);
        LayoutElement le = b.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = 140; le.minWidth = 140;
        TextMeshProUGUI lbl = b.GetComponentInChildren<TextMeshProUGUI>();
        var img = b.targetGraphic as Image;
        b.onClick.AddListener(() =>
        {
            state = !state;
            lbl.text = state ? "On" : "Off";
            if (img != null) img.color = state ? UITheme.ButtonSelected : UITheme.ButtonNormal;
            onChange(state);
        });
    }

    private void ChoiceRow(string label, string[] options, int index, Action<int> onChange)
    {
        GameObject row = Row();
        RowLabel(row.transform, label, 360);

        int current = Mathf.Clamp(index, 0, Mathf.Max(0, options.Length - 1));

        Button prev = UIKit.Button(row.transform, "<", UITheme.ButtonNormal, null, 28);
        AddWidth(prev, 64);

        TextMeshProUGUI valueText = UIKit.Label(row.transform, Vector2.zero, Vector2.one,
            24, FontStyles.Bold, TextAlignmentOptions.Center, UITheme.Accent);
        valueText.text = options.Length > 0 ? options[current] : "—";
        LayoutElement vle = valueText.gameObject.AddComponent<LayoutElement>();
        vle.preferredWidth = 220; vle.minWidth = 220;

        Button next = UIKit.Button(row.transform, ">", UITheme.ButtonNormal, null, 28);
        AddWidth(next, 64);

        prev.onClick.AddListener(() =>
        {
            if (options.Length == 0) return;
            current = (current - 1 + options.Length) % options.Length;
            valueText.text = options[current];
            onChange(current);
        });
        next.onClick.AddListener(() =>
        {
            if (options.Length == 0) return;
            current = (current + 1) % options.Length;
            valueText.text = options[current];
            onChange(current);
        });
    }

    private static void AddWidth(Button b, float w)
    {
        LayoutElement le = b.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = w; le.minWidth = w;
    }

    private void ClearContent()
    {
        for (int i = contentArea.childCount - 1; i >= 0; i--)
            Destroy(contentArea.GetChild(i).gameObject);
    }

    private void BuildUI()
    {
        Canvas canvas = UIKit.Canvas("SettingsCanvas", 40, transform);
        root = canvas.gameObject;
        UIKit.Panel(canvas.transform, "Dim", Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 0.75f));

        GameObject panel = UIKit.Panel(canvas.transform, "Panel",
            new Vector2(0.12f, 0.08f), new Vector2(0.88f, 0.93f), UITheme.PanelBg).gameObject;

        UIKit.Label(panel.transform, new Vector2(0.04f, 0.91f), new Vector2(0.96f, 0.99f),
            40, FontStyles.Bold, TextAlignmentOptions.Left, UITheme.Title).text = "Settings";

        // Tab bar
        GameObject tabBar = new GameObject("TabBar");
        tabBar.transform.SetParent(panel.transform, false);
        UIKit.Rect(tabBar, new Vector2(0.04f, 0.83f), new Vector2(0.96f, 0.90f));
        HorizontalLayoutGroup tlg = tabBar.AddComponent<HorizontalLayoutGroup>();
        tlg.spacing = 10; tlg.childControlWidth = true; tlg.childControlHeight = true;
        tlg.childForceExpandWidth = true; tlg.childForceExpandHeight = true;

        tabButtons.Clear();
        AddTab(tabBar.transform, "Controls", Tab.Controls);
        AddTab(tabBar.transform, "Mouse", Tab.Mouse);
        AddTab(tabBar.transform, "Audio", Tab.Audio);
        AddTab(tabBar.transform, "Graphics", Tab.Graphics);

        // Content area (vertical stack of rows)
        GameObject content = new GameObject("Content");
        content.transform.SetParent(panel.transform, false);
        contentArea = UIKit.Rect(content, new Vector2(0.04f, 0.16f), new Vector2(0.96f, 0.81f));
        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8; vlg.childControlWidth = true; vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperLeft;

        hintText = UIKit.Label(panel.transform, new Vector2(0.04f, 0.02f), new Vector2(0.6f, 0.14f),
            22, FontStyles.Italic, TextAlignmentOptions.Left, UITheme.TextSubtle);

        // Reset + Back
        Button reset = UIKit.Button(panel.transform, "Reset to Defaults",
            new Color(0.45f, 0.28f, 0.18f, 1f), () =>
            {
                GameSettings.ResetToDefaults();
                RenderTab(currentTab);
                ScreenPrompt.Toast("Settings reset to defaults.", 2.5f);
            }, 24);
        UIKit.Rect(reset.gameObject, new Vector2(0.62f, 0.03f), new Vector2(0.80f, 0.13f));

        Button back = UIKit.Button(panel.transform, "Back", UITheme.ButtonAction, Close, 26);
        UIKit.Rect(back.gameObject, new Vector2(0.82f, 0.03f), new Vector2(0.96f, 0.13f));

        root.SetActive(false);
    }

    private void AddTab(Transform parent, string label, Tab tab)
    {
        Button b = UIKit.Button(parent, label, UITheme.ButtonNormal, () => RenderTab(tab), 24);
        tabButtons.Add(b);
    }
}
