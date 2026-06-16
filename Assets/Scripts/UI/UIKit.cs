using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public static class UITheme
{
    public static readonly Color PanelBg       = new Color(0.07f, 0.08f, 0.13f, 0.97f);
    public static readonly Color Dim           = new Color(0f, 0f, 0f, 0.55f);
    public static readonly Color Accent        = new Color(0.40f, 0.75f, 1.00f, 0.90f);
    public static readonly Color Title         = new Color(0.50f, 0.85f, 1.00f);
    public static readonly Color TextLight     = Color.white;
    public static readonly Color TextSubtle    = new Color(0.65f, 0.65f, 0.78f);
    public static readonly Color ButtonNormal  = new Color(0.16f, 0.20f, 0.34f, 1f);
    public static readonly Color ButtonSelected = new Color(0.20f, 0.50f, 0.33f, 1f);
    public static readonly Color ButtonAction  = new Color(0.18f, 0.45f, 0.30f, 1f);
}

public static class UIKit
{
    public static Canvas Canvas(string name, int sortingOrder, Transform parent = null)
    {
        GameObject go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent);

        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    public static RectTransform Rect(GameObject go, Vector2 anchorMin, Vector2 anchorMax)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }

    public static Image Panel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Rect(go, anchorMin, anchorMax);
        Image img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    public static TextMeshProUGUI Label(Transform parent, Vector2 anchorMin, Vector2 anchorMax,
        float size, FontStyles style, TextAlignmentOptions align, Color color)
    {
        GameObject go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        Rect(go, anchorMin, anchorMax);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = align;
        t.color = color;
        t.raycastTarget = false;
        return t;
    }

    public static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    public static UnityEngine.UI.Button Button(Transform parent, string text, Color color,
        System.Action onClick, float fontSize = 26)
    {
        Image img = Panel(parent, "Button", Vector2.zero, Vector2.one, color);
        UnityEngine.UI.Button b = img.gameObject.AddComponent<UnityEngine.UI.Button>();
        b.targetGraphic = img;

        ColorBlock cb = b.colors;
        cb.highlightedColor = new Color(1.15f * color.r, 1.15f * color.g, 1.15f * color.b, 1f);
        cb.pressedColor = new Color(0.8f * color.r, 0.8f * color.g, 0.8f * color.b, 1f);
        cb.fadeDuration = 0.08f;
        b.colors = cb;

        TextMeshProUGUI t = Label(img.transform, Vector2.zero, Vector2.one,
            fontSize, FontStyles.Bold, TextAlignmentOptions.Center, UITheme.TextLight);
        t.text = text;
        t.margin = new Vector4(12, 0, 12, 0);
        t.enableWordWrapping = true;

        if (onClick != null) b.onClick.AddListener(() => onClick());
        return b;
    }

    public static UnityEngine.UI.Slider Slider(Transform parent, float min, float max, float value,
        System.Action<float> onChange)
    {
        GameObject go = new GameObject("Slider");
        go.transform.SetParent(parent, false);
        Rect(go, Vector2.zero, Vector2.one);
        UnityEngine.UI.Slider s = go.AddComponent<UnityEngine.UI.Slider>();

        Image bg = Panel(go.transform, "Background",
            new Vector2(0f, 0.35f), new Vector2(1f, 0.65f), new Color(0.10f, 0.12f, 0.20f, 1f));

        // Fill Area -> Fill
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        RectTransform far = Rect(fillArea, new Vector2(0f, 0.35f), new Vector2(1f, 0.65f));
        far.offsetMin = new Vector2(10, 0); far.offsetMax = new Vector2(-10, 0);
        Image fill = Panel(fillArea.transform, "Fill", Vector2.zero, Vector2.one, UITheme.Accent);
        RectTransform fillRt = fill.rectTransform;
        fillRt.anchorMin = new Vector2(0f, 0f); fillRt.anchorMax = new Vector2(0f, 1f);
        fillRt.sizeDelta = new Vector2(10, 0);

        // Handle Slide Area -> Handle
        GameObject hsa = new GameObject("Handle Slide Area");
        hsa.transform.SetParent(go.transform, false);
        RectTransform hsaRt = Rect(hsa, Vector2.zero, Vector2.one);
        hsaRt.offsetMin = new Vector2(10, 0); hsaRt.offsetMax = new Vector2(-10, 0);
        Image handle = Panel(hsa.transform, "Handle", Vector2.zero, Vector2.one, Color.white);
        RectTransform handleRt = handle.rectTransform;
        handleRt.anchorMin = new Vector2(0f, 0f); handleRt.anchorMax = new Vector2(0f, 1f);
        handleRt.sizeDelta = new Vector2(22, 0);

        s.fillRect = fillRt;
        s.handleRect = handleRt;
        s.targetGraphic = handle;
        s.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
        s.minValue = min;
        s.maxValue = max;
        s.value = value;
        s.wholeNumbers = false;

        if (onChange != null) s.onValueChanged.AddListener(v => onChange(v));
        return s;
    }
}
