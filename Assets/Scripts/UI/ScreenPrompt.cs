using UnityEngine;
using TMPro;

/// <summary>
/// The lightweight HUD shared by every scene. Builds its own canvas on demand and
/// provides three things:
///   • a crosshair (auto-hidden while a blocking UI is open),
///   • a "[E] &lt;verb&gt;" interaction hint (requested each frame by
///     <see cref="PlayerInteraction"/>),
///   • timed toast messages (e.g. "Mission complete!").
/// </summary>
public class ScreenPrompt : MonoBehaviour
{
    private static ScreenPrompt instance;

    private GameObject crosshair;
    private GameObject promptBox;
    private TextMeshProUGUI promptLabel;
    private GameObject toastBox;
    private TextMeshProUGUI toastLabel;

    private int lastRequestFrame = -1;
    private float toastUntil = -1f;

    /// <summary>Make sure the HUD (crosshair) exists in the current scene.</summary>
    public static void Ensure() => EnsureExists();

    /// <summary>Show the interaction hint this frame (call every frame while aimed).</summary>
    public static void Request(string text)
    {
        EnsureExists();
        instance.promptLabel.text = text;
        instance.promptBox.SetActive(true);
        instance.lastRequestFrame = Time.frameCount;
    }

    /// <summary>Flash a centred message for a few seconds.</summary>
    public static void Toast(string text, float seconds = 3f)
    {
        EnsureExists();
        instance.toastLabel.text = text;
        instance.toastBox.SetActive(true);
        instance.toastUntil = Time.time + seconds;
    }

    private static void EnsureExists()
    {
        if (instance != null) return;
        instance = new GameObject("ScreenPrompt").AddComponent<ScreenPrompt>();
        instance.Build();
    }

    private void Build()
    {
        Canvas canvas = UIKit.Canvas("HudCanvas", 15, transform);

        // Crosshair (centre dot).
        crosshair = UIKit.Panel(canvas.transform, "Crosshair",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(1f, 1f, 1f, 0.85f)).gameObject;
        RectTransform crt = crosshair.GetComponent<RectTransform>();
        crt.sizeDelta = new Vector2(10, 10);

        // Interaction prompt (bottom-centre) with a soft background plate.
        promptBox = UIKit.Panel(canvas.transform, "PromptBox",
            new Vector2(0.5f, 0.14f), new Vector2(0.5f, 0.14f), new Color(0f, 0f, 0f, 0.5f)).gameObject;
        promptBox.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 64);
        promptLabel = Pad(UIKit.Label(promptBox.transform, Vector2.zero, Vector2.one,
            30, FontStyles.Bold, TextAlignmentOptions.Center, UITheme.TextLight));

        // Toast (upper-centre).
        toastBox = UIKit.Panel(canvas.transform, "ToastBox",
            new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f), new Color(0.12f, 0.35f, 0.2f, 0.85f)).gameObject;
        toastBox.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 80);
        toastLabel = Pad(UIKit.Label(toastBox.transform, Vector2.zero, Vector2.one,
            36, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.8f, 1f, 0.85f)));

        promptBox.SetActive(false);
        toastBox.SetActive(false);
    }

    // Horizontal breathing room inside a fixed-size plate.
    private static TextMeshProUGUI Pad(TextMeshProUGUI label)
    {
        RectTransform rt = label.rectTransform;
        rt.offsetMin = new Vector2(16, 0);
        rt.offsetMax = new Vector2(-16, 0);
        return label;
    }

    private void LateUpdate()
    {
        // Crosshair off while a dialogue / mini-game / popup is up.
        if (crosshair != null) crosshair.SetActive(!UIBlocker.IsBlocked);

        // Hide the interaction prompt if nobody requested it this frame.
        if (promptBox != null && promptBox.activeSelf && Time.frameCount > lastRequestFrame)
            promptBox.SetActive(false);

        // Expire the toast.
        if (toastBox != null && toastBox.activeSelf && Time.time > toastUntil)
            toastBox.SetActive(false);
    }
}
