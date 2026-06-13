using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Profiling;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// A performance-monitoring overlay (item 9). Shows current / average / lowest /
/// highest FPS, frame time and allocated memory, and can append a labelled benchmark
/// row to <c>benchmarks.csv</c> under <see cref="Application.persistentDataPath"/> so
/// "before" and "after" optimisation runs can be compared (item 11).
///
/// Persistent and auto-created. Hotkeys: <b>F3</b> toggle overlay · <b>F4</b> save a
/// benchmark · <b>F5</b> reset the running stats.
/// </summary>
public class FpsMonitor : MonoBehaviour
{
    public static FpsMonitor Instance { get; private set; }

    // Rolling / since-reset statistics.
    private float warmupUntil;        // ignore the post-load hitch
    private float accumTime;          // summed dt since reset (post-warmup)
    private int accumFrames;          // frames since reset (post-warmup)
    private float minFps = float.MaxValue;
    private float maxFps;
    private float curFps;

    // Smoothed "current" sample.
    private float sampleAccum;
    private int sampleFrames;
    private float nextSample;

    private bool overlayOn;
    private GameObject root;
    private TextMeshProUGUI text;
    private float nextUiRefresh;

    private const float Warmup = 0.5f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance == null)
            new GameObject("FpsMonitor (auto)").AddComponent<FpsMonitor>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ResetStats();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m) => ResetStats();

    public void ResetStats()
    {
        warmupUntil = Time.unscaledTime + Warmup;
        accumTime = 0f; accumFrames = 0;
        minFps = float.MaxValue; maxFps = 0f;
        sampleAccum = 0f; sampleFrames = 0; nextSample = 0f;
    }

    void Update()
    {
        Sample();

        if (Input.GetKeyDown(KeyCode.F3)) ToggleOverlay();
        if (Input.GetKeyDown(KeyCode.F4)) SaveBenchmark();
        if (Input.GetKeyDown(KeyCode.F5)) { ResetStats(); ScreenPrompt.Toast("Performance stats reset.", 1.5f); }

        if (overlayOn && Time.unscaledTime >= nextUiRefresh)
        {
            nextUiRefresh = Time.unscaledTime + 0.25f;
            RefreshOverlay();
        }
    }

    private void Sample()
    {
        float dt = Time.unscaledDeltaTime;
        if (dt <= 0f) return;

        // Smoothed current FPS every 0.2s.
        sampleAccum += dt; sampleFrames++;
        if (Time.unscaledTime >= nextSample)
        {
            curFps = sampleFrames / Mathf.Max(0.0001f, sampleAccum);
            sampleAccum = 0f; sampleFrames = 0;
            nextSample = Time.unscaledTime + 0.2f;
        }

        // Skip warm-up frames and one-off load hitches so min/avg stay meaningful.
        if (Time.unscaledTime < warmupUntil || dt > 0.5f) return;

        float fps = 1f / dt;
        accumTime += dt; accumFrames++;
        if (fps < minFps) minFps = fps;
        if (fps > maxFps) maxFps = fps;
    }

    public float AverageFps => accumFrames > 0 ? accumFrames / accumTime : 0f;
    public float MinFps => minFps == float.MaxValue ? 0f : minFps;
    public float MaxFps => maxFps;
    public float CurrentFps => curFps;
    public float FrameTimeMs => curFps > 0f ? 1000f / curFps : 0f;
    public float MemoryMb => Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);

    // =====================================================================
    //  Overlay
    // =====================================================================
    private void ToggleOverlay()
    {
        overlayOn = !overlayOn;
        if (overlayOn) EnsureOverlay();
        if (root != null) root.SetActive(overlayOn);
        if (overlayOn) RefreshOverlay();
    }

    private void EnsureOverlay()
    {
        if (root != null) return;
        Canvas canvas = UIKit.Canvas("FpsMonitorCanvas", 38, transform);
        root = canvas.gameObject;
        Image panel = UIKit.Panel(canvas.transform, "Panel",
            new Vector2(0.785f, 0.66f), new Vector2(0.995f, 0.99f), new Color(0f, 0f, 0f, 0.72f));
        text = UIKit.Label(panel.transform, new Vector2(0.06f, 0.04f), new Vector2(0.96f, 0.96f),
            24, FontStyles.Bold, TextAlignmentOptions.TopLeft, new Color(0.7f, 1f, 0.75f));
        text.enableWordWrapping = false;
    }

    private void RefreshOverlay()
    {
        if (text == null) return;
        text.text =
            "<b>PERFORMANCE</b>  <size=70%>[F3]</size>\n" +
            $"FPS    : {CurrentFps,5:0}\n" +
            $"Avg    : {AverageFps,5:0}\n" +
            $"Lowest : {MinFps,5:0}\n" +
            $"Highest: {MaxFps,5:0}\n" +
            $"Frame  : {FrameTimeMs,5:0.0} ms\n" +
            $"Memory : {MemoryMb,5:0} MB\n" +
            $"Scene  : {SceneManager.GetActiveScene().name}\n" +
            "<size=80%>[F4] save benchmark  [F5] reset</size>";
    }

    // =====================================================================
    //  Benchmark capture (item 11 data)
    // =====================================================================
    private string BenchmarkPath => Path.Combine(Application.persistentDataPath, "benchmarks.csv");

    public void SaveBenchmark()
    {
        try
        {
            bool isNew = !File.Exists(BenchmarkPath);
            using (StreamWriter w = new StreamWriter(BenchmarkPath, append: true))
            {
                if (isNew)
                    w.WriteLine("timestamp,scene,avg_fps,lowest_fps,highest_fps,frame_ms,memory_mb,quality");

                string row = string.Join(",", new[]
                {
                    DateTime.Now.ToString("o"),
                    SceneManager.GetActiveScene().name,
                    AverageFps.ToString("0.0", CultureInfo.InvariantCulture),
                    MinFps.ToString("0.0", CultureInfo.InvariantCulture),
                    MaxFps.ToString("0.0", CultureInfo.InvariantCulture),
                    (AverageFps > 0f ? 1000f / AverageFps : 0f).ToString("0.00", CultureInfo.InvariantCulture),
                    MemoryMb.ToString("0.0", CultureInfo.InvariantCulture),
                    QualitySettings.GetQualityLevel().ToString(CultureInfo.InvariantCulture),
                });
                w.WriteLine(row);
            }
            ScreenPrompt.Toast($"Benchmark saved → {BenchmarkPath}", 4f);
            Debug.Log($"[FpsMonitor] Benchmark appended to {BenchmarkPath}");
        }
        catch (Exception e)
        {
            ScreenPrompt.Toast("Benchmark save failed (see Console).", 3f);
            Debug.LogError($"[FpsMonitor] Benchmark save failed: {e.Message}");
        }
    }
}
