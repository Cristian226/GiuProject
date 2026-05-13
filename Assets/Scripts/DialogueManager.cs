using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    private GameObject dialoguePanel;
    private TextMeshProUGUI dialogueText;
    private bool isOpen;
    private Action onAccept;
    private Action onCancel;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
    }

    void BuildUI()
    {
        GameObject canvasGO = new GameObject("DialogueCanvas");
        canvasGO.transform.SetParent(transform);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        // Crosshair dot
        GameObject crosshair = new GameObject("Crosshair");
        crosshair.transform.SetParent(canvasGO.transform, false);
        RectTransform crt = crosshair.AddComponent<RectTransform>();
        crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = new Vector2(8, 8);
        crt.anchoredPosition = Vector2.zero;
        Image cimg = crosshair.AddComponent<Image>();
        cimg.color = new Color(1f, 1f, 1f, 0.8f);

        // Dialogue panel — bottom third of screen
        dialoguePanel = new GameObject("DialoguePanel");
        dialoguePanel.transform.SetParent(canvasGO.transform, false);
        RectTransform panelRect = dialoguePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.15f, 0.05f);
        panelRect.anchorMax = new Vector2(0.85f, 0.38f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image bg = dialoguePanel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.1f, 0.92f);

        // Border outline effect via child image
        GameObject border = new GameObject("Border");
        border.transform.SetParent(dialoguePanel.transform, false);
        RectTransform brt = border.AddComponent<RectTransform>();
        brt.anchorMin = Vector2.zero;
        brt.anchorMax = Vector2.one;
        brt.offsetMin = new Vector2(-2, -2);
        brt.offsetMax = new Vector2(2, 2);
        border.AddComponent<Image>().color = new Color(0.4f, 0.4f, 0.6f, 0.6f);
        border.transform.SetAsFirstSibling();

        // Dialogue text
        GameObject textGO = new GameObject("DialogueText");
        textGO.transform.SetParent(dialoguePanel.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.04f, 0.30f);
        textRect.anchorMax = new Vector2(0.96f, 0.94f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        dialogueText = textGO.AddComponent<TextMeshProUGUI>();
        dialogueText.fontSize = 24;
        dialogueText.color = Color.white;
        dialogueText.alignment = TextAlignmentOptions.TopLeft;

        // Instruction bar
        GameObject instrGO = new GameObject("Instructions");
        instrGO.transform.SetParent(dialoguePanel.transform, false);
        RectTransform instrRect = instrGO.AddComponent<RectTransform>();
        instrRect.anchorMin = new Vector2(0.04f, 0.03f);
        instrRect.anchorMax = new Vector2(0.96f, 0.28f);
        instrRect.offsetMin = Vector2.zero;
        instrRect.offsetMax = Vector2.zero;
        TextMeshProUGUI instrText = instrGO.AddComponent<TextMeshProUGUI>();
        instrText.text = "<color=#44FF88>[Enter]</color> Accept        <color=#FF6666>[Esc]</color> Cancel";
        instrText.fontSize = 18;
        instrText.alignment = TextAlignmentOptions.BottomLeft;

        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (!isOpen) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Close();
            onAccept?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
            onCancel?.Invoke();
        }
    }

    public void Show(string text, Action accept, Action cancel = null)
    {
        dialogueText.text = text;
        onAccept = accept;
        onCancel = cancel;
        dialoguePanel.SetActive(true);
        isOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Close()
    {
        dialoguePanel.SetActive(false);
        isOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool IsOpen => isOpen;
}
