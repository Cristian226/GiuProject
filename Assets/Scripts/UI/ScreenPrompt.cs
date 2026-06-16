using UnityEngine;
using TMPro;

public class ScreenPrompt : MonoBehaviour
{
    private static ScreenPrompt instance;

    private GameObject crosshair;
    private GameObject promptBox;
    private TextMeshProUGUI promptLabel;
    private GameObject toastBox;
    private TextMeshProUGUI toastLabel;
    private TextMeshProUGUI fpsLabel;

    private int lastRequestFrame = -1;
    private float toastUntil = -1f;

    private float fpsAccum;
    private int fpsFrames;
    private float fpsNextUpdate;

    public static void Ensure() => EnsureExists();

    public static void Request(string text)
    {
        EnsureExists();
        instance.promptLabel.text = text;
        instance.promptBox.SetActive(true);
        instance.lastRequestFrame = Time.frameCount;
    }

    public static void Toast(string text, float seconds = 3f)
    {
        EnsureExists();
        instance.toastLabel.text = text;
        instance.toastBox.SetActive(true);
        instance.toastUntil = Time.unscaledTime + seconds;
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

        // Interaction prompt (bottom-centre).
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

        // FPS counter (top-left), toggled by the graphics setting.
        fpsLabel = UIKit.Label(canvas.transform, new Vector2(0.005f, 0.94f), new Vector2(0.2f, 0.99f),
            26, FontStyles.Bold, TextAlignmentOptions.TopLeft, new Color(0.7f, 1f, 0.7f));
        fpsLabel.gameObject.SetActive(false);

        promptBox.SetActive(false);
        toastBox.SetActive(false);
    }

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

        // Expire the toast (unscaled so it still clears while the game is paused).
        if (toastBox != null && toastBox.activeSelf && Time.unscaledTime > toastUntil)
            toastBox.SetActive(false);

        UpdateFps();
    }

    private void UpdateFps()
    {
        if (fpsLabel == null) return;

        bool show = GameSettings.ShowFps;
        if (fpsLabel.gameObject.activeSelf != show) fpsLabel.gameObject.SetActive(show);
        if (!show) return;

        fpsAccum += Time.unscaledDeltaTime;
        fpsFrames++;
        if (Time.unscaledTime >= fpsNextUpdate)
        {
            float fps = fpsFrames / Mathf.Max(0.0001f, fpsAccum);
            fpsLabel.text = $"{Mathf.RoundToInt(fps)} FPS";
            fpsAccum = 0f; fpsFrames = 0;
            fpsNextUpdate = Time.unscaledTime + 0.5f;
        }
    }
}
