using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CelebrationManager : MonoBehaviour
{
    public static CelebrationManager Instance { get; private set; }
    private Material particleMat;
    private CanvasGroup bannerGroup;
    private TextMeshProUGUI bannerTitle, bannerReward, bannerXp, bannerLevel;
    private Image xpFill;
    private Coroutine bannerRoutine;
    private GameObject victoryRoot;

    private static readonly Color[] Festive =
    {
        new Color(0.95f, 0.25f, 0.25f), new Color(0.98f, 0.78f, 0.20f),
        new Color(0.30f, 0.70f, 1.00f), new Color(0.35f, 0.85f, 0.45f),
        new Color(0.85f, 0.45f, 0.95f), new Color(1.00f, 1.00f, 1.00f),
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance == null)
            new GameObject("CelebrationManager (auto)").AddComponent<CelebrationManager>();
    }

    public static CelebrationManager Get()
    {
        if (Instance == null) Bootstrap();
        return Instance;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Shader s = Shader.Find("Sprites/Default");
        particleMat = new Material(s != null ? s : Shader.Find("Standard"));
    }

    public void PlayMissionComplete(string rewardName, int xpGained, int newXpTotal)
    {
        AudioManager.Instance?.PlayFanfare();
        BurstAroundCamera(8, 1f);
        SpawnConfetti(3.0f);

        EnsureBanner();
        bannerTitle.text = "MISSION COMPLETE";
        bannerReward.text = string.IsNullOrEmpty(rewardName) ? "Well done!" : $"Reward unlocked:  <b>{rewardName}</b>";
        if (bannerRoutine != null) StopCoroutine(bannerRoutine);
        bannerRoutine = StartCoroutine(BannerRoutine(xpGained, newXpTotal, 4.5f));
    }

    public void PlayFinale()
    {
        AudioManager.Instance?.PlayFanfare(1f);
        StartCoroutine(FireworksShow(4.5f));
        ShowVictoryScreen();
    }

    private IEnumerator BannerRoutine(int xpGained, int newXpTotal, float holdSeconds)
    {
        bannerGroup.gameObject.SetActive(true);
        bannerGroup.alpha = 0f;

        int startXp = Mathf.Max(0, newXpTotal - xpGained);

        // Fade in.
        for (float t = 0f; t < 0.35f; t += Time.unscaledDeltaTime)
        {
            bannerGroup.alpha = Mathf.Clamp01(t / 0.35f);
            yield return null;
        }
        bannerGroup.alpha = 1f;

        // Count the XP up and fill the level bar (handles crossing a level boundary).
        const float countTime = 1.2f;
        for (float t = 0f; t < countTime; t += Time.unscaledDeltaTime)
        {
            int shown = Mathf.RoundToInt(Mathf.Lerp(startXp, newXpTotal, t / countTime));
            ApplyXpVisual(shown, xpGained);
            yield return null;
        }
        ApplyXpVisual(newXpTotal, xpGained);

        // Hold, then fade out.
        yield return new WaitForSecondsRealtime(holdSeconds);
        for (float t = 0f; t < 0.6f; t += Time.unscaledDeltaTime)
        {
            bannerGroup.alpha = 1f - Mathf.Clamp01(t / 0.6f);
            yield return null;
        }
        bannerGroup.alpha = 0f;
        bannerGroup.gameObject.SetActive(false);
        bannerRoutine = null;
    }

    private void ApplyXpVisual(int shownXp, int xpGained)
    {
        int level = shownXp / GameProgress.XpPerLevel + 1;
        float frac = (float)(shownXp % GameProgress.XpPerLevel) / GameProgress.XpPerLevel;
        bannerXp.text = $"+{xpGained} XP";
        bannerLevel.text = $"Level {level}";
        if (xpFill != null)
        {
            RectTransform rt = xpFill.rectTransform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(Mathf.Clamp01(frac), 1f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }
    }

    private void EnsureBanner()
    {
        if (bannerGroup != null) return;

        Canvas canvas = UIKit.Canvas("CelebrationCanvas", 40, transform);
        bannerGroup = canvas.gameObject.AddComponent<CanvasGroup>();
        bannerGroup.interactable = false;
        bannerGroup.blocksRaycasts = false;

        Image panel = UIKit.Panel(canvas.transform, "Banner",
            new Vector2(0.27f, 0.68f), new Vector2(0.73f, 0.90f), new Color(0.06f, 0.09f, 0.16f, 0.95f));
        Image border = UIKit.Panel(panel.transform, "Border", Vector2.zero, Vector2.one, new Color(1f, 0.85f, 0.4f, 0.9f));
        border.rectTransform.offsetMin = new Vector2(-3, -3);
        border.rectTransform.offsetMax = new Vector2(3, 3);
        border.transform.SetAsFirstSibling();

        bannerTitle = UIKit.Label(panel.transform, new Vector2(0.04f, 0.62f), new Vector2(0.96f, 0.96f),
            54, FontStyles.Bold, TextAlignmentOptions.Center, new Color(1f, 0.9f, 0.5f));
        bannerReward = UIKit.Label(panel.transform, new Vector2(0.04f, 0.40f), new Vector2(0.96f, 0.62f),
            30, FontStyles.Normal, TextAlignmentOptions.Center, UITheme.TextLight);

        Image track = UIKit.Panel(panel.transform, "XpTrack", new Vector2(0.08f, 0.16f), new Vector2(0.92f, 0.30f),
            new Color(0.10f, 0.12f, 0.20f, 1f));
        xpFill = UIKit.Panel(track.transform, "XpFill", new Vector2(0f, 0f), new Vector2(0f, 1f), new Color(0.3f, 0.8f, 0.5f, 1f));

        bannerXp = UIKit.Label(panel.transform, new Vector2(0.08f, 0.02f), new Vector2(0.5f, 0.15f),
            26, FontStyles.Bold, TextAlignmentOptions.Left, new Color(0.6f, 1f, 0.7f));
        bannerLevel = UIKit.Label(panel.transform, new Vector2(0.5f, 0.02f), new Vector2(0.92f, 0.15f),
            26, FontStyles.Bold, TextAlignmentOptions.Right, UITheme.Accent);

        bannerGroup.gameObject.SetActive(false);
    }

    private void ShowVictoryScreen()
    {
        if (victoryRoot != null) { victoryRoot.SetActive(true); return; }

        Canvas canvas = UIKit.Canvas("VictoryCanvas", 45, transform);
        victoryRoot = canvas.gameObject;
        UIKit.EnsureEventSystem();
        UIKit.Panel(canvas.transform, "Dim", Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 0.8f));

        Image cert = UIKit.Panel(canvas.transform, "Certificate",
            new Vector2(0.22f, 0.16f), new Vector2(0.78f, 0.86f), new Color(0.97f, 0.95f, 0.88f, 1f));
        Image frame = UIKit.Panel(cert.transform, "Frame", Vector2.zero, Vector2.one, new Color(0.78f, 0.62f, 0.20f, 1f));
        frame.rectTransform.offsetMin = new Vector2(-6, -6);
        frame.rectTransform.offsetMax = new Vector2(6, 6);
        frame.transform.SetAsFirstSibling();

        Color ink = new Color(0.16f, 0.12f, 0.06f);
        UIKit.Label(cert.transform, new Vector2(0.06f, 0.80f), new Vector2(0.94f, 0.93f),
            30, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.55f, 0.10f, 0.12f)).text = "★  VICTORY  ★";
        UIKit.Label(cert.transform, new Vector2(0.06f, 0.66f), new Vector2(0.94f, 0.80f),
            46, FontStyles.Bold, TextAlignmentOptions.Center, ink).text = "Certificate of Completion";
        UIKit.Label(cert.transform, new Vector2(0.08f, 0.30f), new Vector2(0.92f, 0.64f),
            26, FontStyles.Normal, TextAlignmentOptions.Center, ink).text =
            "This certifies that the Traveller has completed\n<b>Primii Pași în România</b>\n\n" +
            "mastering its cuisine, geography, history and music,\nand passing the Grand Final Challenge.";
        UIKit.Label(cert.transform, new Vector2(0.08f, 0.18f), new Vector2(0.92f, 0.28f),
            22, FontStyles.Italic, TextAlignmentOptions.Center, new Color(0.35f, 0.28f, 0.16f)).text =
            $"Awarded {DateTime.Now:dd MMMM yyyy}";

        Button close = UIKit.Button(cert.transform, "Continue", UITheme.ButtonAction, CloseVictory, 26);
        UIKit.Rect(close.gameObject, new Vector2(0.36f, 0.04f), new Vector2(0.64f, 0.14f));

        UIBlocker.Push();   // pause + show cursor while the certificate is up
    }

    private void CloseVictory()
    {
        if (victoryRoot != null) victoryRoot.SetActive(false);
        UIBlocker.Pop();
    }

    private void BurstAroundCamera(int count, float scale)
    {
        Camera cam = Camera.main;
        Vector3 origin = cam != null ? cam.transform.position : Vector3.zero;
        Vector3 fwd = cam != null ? cam.transform.forward : Vector3.forward;
        fwd.y = 0f; fwd = fwd.sqrMagnitude > 0.001f ? fwd.normalized : Vector3.forward;
        Vector3 right = Vector3.Cross(Vector3.up, fwd);

        for (int i = 0; i < count; i++)
        {
            float side = (i % 2 == 0 ? 1f : -1f) * UnityEngine.Random.Range(2.5f, 7f);
            float dist = UnityEngine.Random.Range(8f, 14f);
            float up = UnityEngine.Random.Range(3f, 7f);
            Vector3 pos = origin + fwd * dist + right * side + Vector3.up * up;
            SpawnFirework(pos, Festive[UnityEngine.Random.Range(0, Festive.Length)], scale);
        }
    }

    private IEnumerator FireworksShow(float seconds)
    {
        float end = Time.unscaledTime + seconds;
        while (Time.unscaledTime < end)
        {
            BurstAroundCamera(3, UnityEngine.Random.Range(1.0f, 1.8f));
            yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(0.25f, 0.55f));
        }
    }

    private void SpawnFirework(Vector3 pos, Color color, float scale)
    {
        GameObject go = new GameObject("Firework");
        go.transform.position = pos;
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(); ps.Clear();

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 1.0f;
        main.startLifetime = 1.2f * scale;
        main.startSpeed = 6.5f * scale;
        main.startSize = 0.16f * scale;
        main.startColor = color;
        main.gravityModifier = 0.55f;
        main.maxParticles = 500;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)(90 * scale)) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.55f), new GradientAlphaKey(0f, 1f) });
        col.color = g;

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.25f));

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = particleMat;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        ps.Play();
    }

    private void SpawnConfetti(float duration)
    {
        Camera cam = Camera.main;
        Vector3 fwd = cam != null ? cam.transform.forward : Vector3.forward;
        fwd.y = 0f; fwd = fwd.sqrMagnitude > 0.001f ? fwd.normalized : Vector3.forward;
        Vector3 center = (cam != null ? cam.transform.position : Vector3.zero) + fwd * 6f + Vector3.up * 6f;

        GameObject go = new GameObject("Confetti");
        go.transform.position = center;
        go.transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(); ps.Clear();

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = duration;
        main.startLifetime = 3.2f;
        main.startSpeed = 1.2f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, 2f * Mathf.PI);
        main.gravityModifier = 0.35f;
        main.maxParticles = 800;
        main.stopAction = ParticleSystemStopAction.Destroy;

        Gradient cg = new Gradient();
        cg.SetKeys(
            new[]
            {
                new GradientColorKey(Festive[0], 0.0f), new GradientColorKey(Festive[1], 0.25f),
                new GradientColorKey(Festive[2], 0.5f), new GradientColorKey(Festive[3], 0.75f),
                new GradientColorKey(Festive[4], 1.0f),
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        ParticleSystem.MinMaxGradient mmg = new ParticleSystem.MinMaxGradient(cg)
        {
            mode = ParticleSystemGradientMode.RandomColor
        };
        main.startColor = mmg;

        var emission = ps.emission;
        emission.rateOverTime = 90f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(9f, 0.2f, 4f);

        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = particleMat;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        ps.Play();
    }
}
