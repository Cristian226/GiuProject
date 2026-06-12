using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Shared colours so every runtime-built UI (dialogue, mini-game, info popup, HUD)
/// looks consistent. Tweak here to restyle the whole game at once.
/// </summary>
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

/// <summary>
/// Tiny factory helpers for building Screen-Space-Overlay UI in code, used by the
/// self-building UI classes. Removes the copy-pasted canvas/label/EventSystem
/// boilerplate that each of them used to carry.
/// </summary>
public static class UIKit
{
    /// <summary>Create a standard overlay canvas (1920x1080 reference, even scaling).</summary>
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

    /// <summary>Add/reuse a RectTransform and anchor it between the given corners.</summary>
    public static RectTransform Rect(GameObject go, Vector2 anchorMin, Vector2 anchorMax)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }

    /// <summary>A coloured Image panel anchored between two corners.</summary>
    public static Image Panel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Rect(go, anchorMin, anchorMax);
        Image img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    /// <summary>A non-interactive TextMeshPro label anchored between two corners.</summary>
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

    /// <summary>Guarantee an EventSystem exists so UI buttons receive clicks.</summary>
    public static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }
}
