using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    private GameObject dialoguePanel;
    private TextMeshProUGUI dialogueText;
    private bool isOpen;
    private Action onAccept;
    private Action onCancel;
    private TextMeshProUGUI speakerNameText;
    private Transform choicesContainer;
    private GameObject choiceButtonPrefab;
    private GameObject continueButtonGO;
    private DialogueNode currentNode;
    private string currentNPCName;
    private Coroutine typewriterCoroutine;
    private bool isTyping = false;
    private float typewriterSpeed = 0.03f;

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

        //Speaker name label (sits above the dialogue text)
        GameObject speakerGO = new GameObject("SpeakerName");
        speakerGO.transform.SetParent(dialoguePanel.transform, false);
        RectTransform srt = speakerGO.AddComponent<RectTransform>();
        srt.anchorMin = new Vector2(0.04f, 0.82f);
        srt.anchorMax = new Vector2(0.6f, 0.97f);
        srt.offsetMin = srt.offsetMax = Vector2.zero;
        speakerNameText = speakerGO.AddComponent<TextMeshProUGUI>();
        speakerNameText.fontSize = 20;
        speakerNameText.fontStyle = FontStyles.Bold;
        speakerNameText.color = new Color(0.4f, 1f, 0.7f);


        // Dialogue text — CHANGED anchor to leave room for speaker name above
        GameObject textGO = new GameObject("DialogueText");
        textGO.transform.SetParent(dialoguePanel.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.04f, 0.30f);
        textRect.anchorMax = new Vector2(0.96f, 0.82f); // was 0.94f, lowered to fit speaker name
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

        //Choices container (vertical stack of buttons)
        GameObject choicesGO = new GameObject("ChoicesContainer");
        choicesGO.transform.SetParent(dialoguePanel.transform, false);
        RectTransform choicesRect = choicesGO.AddComponent<RectTransform>();
        choicesRect.anchorMin = new Vector2(0.04f, 0.03f);
        choicesRect.anchorMax = new Vector2(0.74f, 0.28f);
        choicesRect.offsetMin = choicesRect.offsetMax = Vector2.zero;
        VerticalLayoutGroup vlg = choicesGO.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.LowerLeft;
        vlg.spacing = 5;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        choicesContainer = choicesGO.transform;

         //Choice button prefab (built in memory, never added to scene directly)
        choiceButtonPrefab = new GameObject("ChoiceBtn");
        choiceButtonPrefab.AddComponent<RectTransform>().sizeDelta = new Vector2(0, 34);
        choiceButtonPrefab.AddComponent<Image>().color = new Color(0.15f, 0.2f, 0.35f, 0.9f);
        choiceButtonPrefab.AddComponent<Button>();
        GameObject btnTextGO = new GameObject("Text");
        btnTextGO.transform.SetParent(choiceButtonPrefab.transform, false);
        RectTransform btnTextRT = btnTextGO.AddComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.offsetMin = new Vector2(10, 0);
        btnTextRT.offsetMax = Vector2.zero;
        TextMeshProUGUI btnTMP = btnTextGO.AddComponent<TextMeshProUGUI>();
        btnTMP.fontSize = 15;
        btnTMP.color = Color.white;
        btnTMP.alignment = TextAlignmentOptions.Left;

        //Continue button (bottom right of panel)
        continueButtonGO = new GameObject("ContinueButton");
        continueButtonGO.transform.SetParent(dialoguePanel.transform, false);
        RectTransform contRT = continueButtonGO.AddComponent<RectTransform>();
        contRT.anchorMin = new Vector2(0.75f, 0.03f);
        contRT.anchorMax = new Vector2(0.96f, 0.26f);
        contRT.offsetMin = contRT.offsetMax = Vector2.zero;
        continueButtonGO.AddComponent<Image>().color = new Color(0.2f, 0.5f, 0.3f, 0.9f);
        continueButtonGO.AddComponent<Button>();
        GameObject contTextGO = new GameObject("Text");
        contTextGO.transform.SetParent(continueButtonGO.transform, false);
        RectTransform contTextRT = contTextGO.AddComponent<RectTransform>();
        contTextRT.anchorMin = Vector2.zero;
        contTextRT.anchorMax = Vector2.one;
        contTextRT.offsetMin = contTextRT.offsetMax = Vector2.zero;
        TextMeshProUGUI contTMP = contTextGO.AddComponent<TextMeshProUGUI>();
        contTMP.text = "Continue";
        contTMP.fontSize = 17;
        contTMP.color = Color.white;
        contTMP.alignment = TextAlignmentOptions.Center;
        continueButtonGO.SetActive(false);

        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (!isOpen) return;
        if (currentNode != null) return; // node-based dialogue, buttons handle everything

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

    //entry point for node-based dialogue
    public void StartDialogue(string npcName, DialogueNode rootNode)
    {
        if (rootNode == null) return;
        currentNPCName = npcName;
        dialoguePanel.SetActive(true);
        isOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ShowNode(rootNode);
    }

    private void ShowNode(DialogueNode node)
    {
        currentNode = node;
        speakerNameText.text = node.speaker == DialogueNode.Speaker.NPC ? currentNPCName : "You";

        if (node.questToGrant != null)
            Debug.Log($"[Dialogue] Quest granted: {node.questToGrant.questName}");
        if (node.itemToGrant != null)
            Debug.Log($"[Dialogue] Item granted: {node.itemDisplayName}");

        ClearChoices();
        continueButtonGO.SetActive(false);

        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(TypewriteText(node.text, () => RevealControls(node)));
    }

    private IEnumerator TypewriteText(string text, Action onComplete)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSecondsRealtime(typewriterSpeed);
        }
        isTyping = false;
        onComplete?.Invoke();
    }

    private void RevealControls(DialogueNode node)
    {
        if (node.choices == null || node.choices.Count == 0)
        {
            continueButtonGO.SetActive(true);
            continueButtonGO.GetComponentInChildren<TextMeshProUGUI>().text = "Close";
            continueButtonGO.GetComponent<Button>().onClick.RemoveAllListeners();
            continueButtonGO.GetComponent<Button>().onClick.AddListener(Close);
            return;
        }
        if (node.choices.Count == 1)
        {
            continueButtonGO.SetActive(true);
            var label = node.choices[0].choiceLabel;
            continueButtonGO.GetComponentInChildren<TextMeshProUGUI>().text =
                string.IsNullOrEmpty(label) ? "Continue" : label;
            continueButtonGO.GetComponent<Button>().onClick.RemoveAllListeners();
            DialogueNode next = node.choices[0].nextNode;
            continueButtonGO.GetComponent<Button>().onClick.AddListener(() => {
                if (next != null) ShowNode(next); else Close();
            });
            return;
        }
        foreach (var choice in node.choices)
        {
            GameObject btn = Instantiate(choiceButtonPrefab, choicesContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = choice.choiceLabel;
            DialogueNode next = choice.nextNode;
            btn.GetComponent<Button>().onClick.AddListener(() => {
                if (next != null) ShowNode(next); else Close();
            });
        }
    }

    private void ClearChoices()
    {
        foreach (Transform child in choicesContainer) Destroy(child.gameObject);
    }

    // NEW — call this from an EventTrigger on the dialogue text if you want click-to-skip
    public void OnDialogueBodyClicked()
    {
        if (!isTyping) return;
        StopCoroutine(typewriterCoroutine);
        isTyping = false;
        dialogueText.text = currentNode.text;
        RevealControls(currentNode);
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
