using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// The collectibles inventory. A persistent, auto-created overlay opened with the
/// (rebindable) Inventory key. Shows every reward in the catalog as a tile —
/// collected ones in colour with their glyph (click to read about them), missing
/// ones locked — plus an overall completion percentage and a progress bar.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    public static bool IsOpen { get; private set; }
    private static InventoryUI instance;

    private GameObject root;
    private RectTransform grid;
    private TextMeshProUGUI percentText;
    private Image progressFill;
    private TextMeshProUGUI bannerText;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance == null)
            new GameObject("InventoryUI (auto)").AddComponent<InventoryUI>();
    }

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        IsOpen = false;
        DontDestroyOnLoad(gameObject);
        BuildUI();
    }

    void Update()
    {
        if (IsOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || GameSettings.Pressed(GameAction.Inventory))
                Close();
            return;
        }

        if (UIBlocker.IsBlocked) return;
        if (PlayerMovement.Current == null) return;
        if (GameSettings.Pressed(GameAction.Inventory)) Open();
    }

    public static void OpenInventory() { if (instance != null) instance.Open(); }

    public void Open()
    {
        UIKit.EnsureEventSystem();
        Render();
        root.SetActive(true);
        IsOpen = true;
        UIBlocker.Push();
    }

    public void Close()
    {
        IsOpen = false;
        root.SetActive(false);
        UIBlocker.Pop();
    }

    // ── Rendering ─────────────────────────────────────────────────────────────
    private void Render()
    {
        GameProgress p = GameProgress.Instance;
        int percent = p != null ? p.CompletionPercent() : 0;

        percentText.text = $"Progress: {percent}%";
        if (progressFill != null)
        {
            RectTransform rt = progressFill.rectTransform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(Mathf.Clamp01(percent / 100f), 1f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        bool all = p != null && p.AllCollected();
        bannerText.gameObject.SetActive(all);
        if (all) bannerText.text = "All items collected!  Talk to the little guide in the city to start the final quest.";

        for (int i = grid.childCount - 1; i >= 0; i--) Destroy(grid.GetChild(i).gameObject);

        foreach (Collectible c in Catalog.All)
        {
            bool owned = p != null && p.HasItem(c.id);
            bool required = Catalog.RequiredMissionIds.Contains(c.missionId);
            BuildTile(c, owned, required);
        }
    }

    private void BuildTile(Collectible c, bool owned, bool required)
    {
        // Owned tiles take the collectible's accent colour (darkened); missing ones stay grey.
        Color bg = owned
            ? new Color(c.color.r * 0.32f, c.color.g * 0.32f, c.color.b * 0.32f, 1f)
            : new Color(0.12f, 0.13f, 0.20f, 1f);
        Image tile = UIKit.Panel(grid, "Tile", Vector2.zero, Vector2.one, bg);
        Button b = tile.gameObject.AddComponent<Button>();
        b.targetGraphic = tile;

        // Glyph (short text token — the font has no emoji)
        TextMeshProUGUI glyph = UIKit.Label(tile.transform, new Vector2(0f, 0.42f), new Vector2(1f, 0.95f),
            58, FontStyles.Bold, TextAlignmentOptions.Center,
            owned ? c.color : new Color(0.4f, 0.42f, 0.5f));
        glyph.text = owned ? c.glyph : "?";

        // Name
        TextMeshProUGUI name = UIKit.Label(tile.transform, new Vector2(0.05f, 0.24f), new Vector2(0.95f, 0.42f),
            22, FontStyles.Bold, TextAlignmentOptions.Center,
            owned ? UITheme.Title : new Color(0.55f, 0.57f, 0.66f));
        name.text = owned ? c.displayName : "???";
        name.enableWordWrapping = true;

        // Status
        string status = owned ? "Collected" : (required ? "Locked" : "Coming soon");
        Color statusColor = owned ? new Color(0.55f, 1f, 0.7f)
                                  : (required ? new Color(0.85f, 0.6f, 0.5f) : UITheme.TextSubtle);
        UIKit.Label(tile.transform, new Vector2(0.05f, 0.06f), new Vector2(0.95f, 0.24f),
            20, FontStyles.Italic, TextAlignmentOptions.Center, statusColor).text = status;

        if (owned)
            b.onClick.AddListener(() => InfoPopup.Get().Show($"{c.glyph}  {c.displayName}", c.description));
        else
            b.interactable = false;
    }

    // ── UI construction ───────────────────────────────────────────────────────
    private void BuildUI()
    {
        Canvas canvas = UIKit.Canvas("InventoryCanvas", 32, transform);
        root = canvas.gameObject;
        UIKit.Panel(canvas.transform, "Dim", Vector2.zero, Vector2.one, UITheme.Dim);

        GameObject panel = UIKit.Panel(canvas.transform, "Panel",
            new Vector2(0.16f, 0.12f), new Vector2(0.84f, 0.90f), UITheme.PanelBg).gameObject;

        UIKit.Label(panel.transform, new Vector2(0.04f, 0.90f), new Vector2(0.7f, 0.99f),
            40, FontStyles.Bold, TextAlignmentOptions.Left, UITheme.Title).text = "Inventory";

        percentText = UIKit.Label(panel.transform, new Vector2(0.5f, 0.90f), new Vector2(0.96f, 0.99f),
            30, FontStyles.Bold, TextAlignmentOptions.Right, UITheme.Accent);

        // Progress bar
        Image track = UIKit.Panel(panel.transform, "ProgressTrack",
            new Vector2(0.04f, 0.855f), new Vector2(0.96f, 0.885f), new Color(0.10f, 0.12f, 0.20f, 1f));
        progressFill = UIKit.Panel(track.transform, "ProgressFill",
            new Vector2(0f, 0f), new Vector2(0f, 1f), UITheme.ButtonSelected);

        // Item grid
        GameObject gridGO = new GameObject("Grid");
        gridGO.transform.SetParent(panel.transform, false);
        grid = UIKit.Rect(gridGO, new Vector2(0.04f, 0.16f), new Vector2(0.96f, 0.84f));
        GridLayoutGroup glg = gridGO.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(250, 220);
        glg.spacing = new Vector2(18, 18);
        glg.padding = new RectOffset(6, 6, 6, 6);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 3;
        glg.childAlignment = TextAnchor.UpperCenter;

        bannerText = UIKit.Label(panel.transform, new Vector2(0.04f, 0.085f), new Vector2(0.78f, 0.15f),
            22, FontStyles.Bold, TextAlignmentOptions.Left, new Color(1f, 0.92f, 0.55f));
        bannerText.enableWordWrapping = true;
        bannerText.gameObject.SetActive(false);

        Button close = UIKit.Button(panel.transform, "Close", UITheme.ButtonAction, Close, 26);
        UIKit.Rect(close.gameObject, new Vector2(0.80f, 0.03f), new Vector2(0.96f, 0.13f));

        UIKit.Label(panel.transform, new Vector2(0.04f, 0.02f), new Vector2(0.6f, 0.08f),
            20, FontStyles.Italic, TextAlignmentOptions.Left, UITheme.TextSubtle)
            .text = "Click a collected item to read about it.  [Esc] / [I] to close.";

        root.SetActive(false);
    }
}
