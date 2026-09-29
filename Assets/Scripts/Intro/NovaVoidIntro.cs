using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Intro da NovaVoid Studio (~4s). Monta toda a UI por código: basta colocar
/// este script em um GameObject vazio de uma cena "Intro".
/// Sequência: scanlines VHS -> ponto roxo pulsando -> explosão de voxels
/// -> logo com glitch RGB + flash -> subtitulo digitado -> fade -> próxima cena.
/// </summary>
public class NovaVoidIntro : MonoBehaviour
{
    [Header("Fluxo")]
    public string nextSceneName = "MainMenu";
    public bool skippable = true;

    [Header("Textos")]
    public string studioName = "NOVAVOID";
    public string subtitle = "S T U D I O";

    [Header("Visual")]
    public TMP_FontAsset font;                       // opcional: fonte pixel
    public Color green = new Color(0.20f, 1.00f, 0.45f);
    public Color purple = new Color(0.62f, 0.20f, 1.00f);
    public int voxelCount = 56;
    [Range(0f, 0.6f)] public float scanlineAlpha = 0.35f;

    [Header("Audio (opcional)")]
    public AudioClip sting;                          // som de impacto/sintetizador

    // --- runtime ---
    CanvasGroup content;
    RectTransform contentRt;
    RectTransform dotRoot;
    CanvasGroup dotGroup;
    RawImage scanlines;
    Image flash;
    TextMeshProUGUI txtMain, txtPurple, txtGreen, txtSub;
    AudioSource audioSrc;
    readonly List<Voxel> voxels = new List<Voxel>();
    bool leaving;
    float elapsed;

    struct Voxel
    {
        public RectTransform rt;
        public Image img;
        public Vector2 dir;
        public float dist;
        public float spin;
        public float baseAlpha;
    }

    void Awake()
    {
        audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
        BuildUI();
    }

    void Start()
    {
        StartCoroutine(Run());
    }

    void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        if (skippable && !leaving && elapsed > 0.3f && SkipPressed())
        {
            StopAllCoroutines();
            StartCoroutine(Exit(0.2f));
        }
    }

    bool SkipPressed()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        var m = Mouse.current;
        var t = Touchscreen.current;
        return (k != null && k.anyKey.wasPressedThisFrame)
            || (m != null && m.leftButton.wasPressedThisFrame)
            || (t != null && t.primaryTouch.press.wasPressedThisFrame);
#else
        return Input.anyKeyDown || Input.GetMouseButtonDown(0);
#endif
    }

    // ------------------------------------------------------------------
    // SEQUENCIA
    // ------------------------------------------------------------------
    IEnumerator Run()
    {
        // 1) scanlines aparecem
        yield return Tween(0.4f, t => scanlines.color = new Color(1, 1, 1, Mathf.Lerp(0f, scanlineAlpha, t)));

        // 2) ponto roxo: batimento
        if (sting) audioSrc.PlayOneShot(sting);
        dotRoot.gameObject.SetActive(true);
        yield return Tween(0.8f, t =>
        {
            float beat = 1f + 0.45f * Mathf.Abs(Mathf.Sin(t * Mathf.PI * 2f));
            dotRoot.localScale = Vector3.one * beat * Mathf.Lerp(0.2f, 1f, EaseOut(t));
        });

        // 3) explosão de voxels + logo entra em paralelo
        Coroutine logo = StartCoroutine(LogoIn());
        yield return Tween(0.9f, t =>
        {
            float e = EaseOut(t);
            dotRoot.localScale = Vector3.one * Mathf.Lerp(1f, 7f, e);
            dotGroup.alpha = 1f - t;
            foreach (var v in voxels)
            {
                v.rt.gameObject.SetActive(true);
                v.rt.anchoredPosition = v.dir * v.dist * e;
                v.rt.localRotation = Quaternion.Euler(0, 0, v.spin * t);
                float fade = t < 0.5f ? 1f : 1f - (t - 0.5f) * 2f;
                var c = v.img.color; c.a = v.baseAlpha * fade; v.img.color = c;
            }
        });
        dotRoot.gameObject.SetActive(false);
        yield return logo;

        // 4) segura com micro-glitch
        float hold = 0.6f, next = 0f;
        yield return Tween(hold, t =>
        {
            next -= Time.unscaledDeltaTime;
            if (next <= 0f)
            {
                next = Random.Range(0.08f, 0.2f);
                SetSplit(Random.Range(3f, 9f), Random.Range(-2f, 2f));
            }
        });

        // 5) saída
        yield return Exit(0.4f);
    }

    IEnumerator LogoIn()
    {
        yield return Wait(0.45f);

        // impacto: flash branco + glitch forte
        txtMain.gameObject.SetActive(true);
        txtPurple.gameObject.SetActive(true);
        txtGreen.gameObject.SetActive(true);
        StartCoroutine(Flash(0.12f, 0.85f));

        yield return Tween(0.5f, t =>
        {
            float amp = Mathf.Lerp(50f, 4f, t);
            SetSplit(Random.Range(-amp, amp), Random.Range(-amp * 0.25f, amp * 0.25f));
            float a = Random.value > t ? Random.Range(0.3f, 1f) : 1f;
            txtMain.alpha = a;
            txtPurple.alpha = a * 0.85f;
            txtGreen.alpha = a * 0.85f;
            contentRt.anchoredPosition = new Vector2(Random.Range(-1f, 1f) * (1f - t) * 12f, 0f);
        });
        contentRt.anchoredPosition = Vector2.zero;
        txtMain.alpha = 1f;
        SetSplit(5f, 0f);

        // subtítulo digitado
        txtSub.gameObject.SetActive(true);
        txtSub.maxVisibleCharacters = 0;
        int total = subtitle.Length;
        yield return Tween(0.6f, t => txtSub.maxVisibleCharacters = Mathf.CeilToInt(total * t));
    }

    IEnumerator Exit(float fadeTime)
    {
        leaving = true;
        float start = content.alpha;
        yield return Tween(fadeTime, t => content.alpha = Mathf.Lerp(start, 0f, t));

        if (Application.CanStreamedLevelBeLoaded(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("[NovaVoidIntro] Cena '" + nextSceneName + "' não está no Build Settings.");
    }

    IEnumerator Flash(float duration, float peak)
    {
        yield return Tween(duration, t => flash.color = new Color(1, 1, 1, Mathf.Lerp(peak, 0f, t)));
    }

    // ------------------------------------------------------------------
    // HELPERS
    // ------------------------------------------------------------------
    void SetSplit(float x, float y)
    {
        txtPurple.rectTransform.anchoredPosition = new Vector2(-x, y);
        txtGreen.rectTransform.anchoredPosition = new Vector2(x, -y);
    }

    static IEnumerator Tween(float duration, System.Action<float> step)
    {
        float e = 0f;
        while (e < duration)
        {
            e += Time.unscaledDeltaTime;
            step(Mathf.Clamp01(e / duration));
            yield return null;
        }
        step(1f);
    }

    static IEnumerator Wait(float s)
    {
        float e = 0f;
        while (e < s) { e += Time.unscaledDeltaTime; yield return null; }
    }

    static float EaseOut(float t) { return 1f - Mathf.Pow(1f - t, 3f); }

    // ------------------------------------------------------------------
    // CONSTRUÇÃO DA UI (tudo por código)
    // ------------------------------------------------------------------
    void BuildUI()
    {
        // Canvas
        var canvasGo = new GameObject("IntroCanvas", typeof(RectTransform));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        var root = (RectTransform)canvasGo.transform;

        // Fundo preto fixo
        MakeImage(root, "Background", Color.black, Vector2.zero, true);

        // Conteúdo (fade geral)
        var contentGo = new GameObject("Content", typeof(RectTransform));
        contentRt = (RectTransform)contentGo.transform;
        contentRt.SetParent(root, false);
        Stretch(contentRt);
        content = contentGo.AddComponent<CanvasGroup>();
        content.interactable = false;
        content.blocksRaycasts = false;

        // Voxels
        for (int i = 0; i < voxelCount; i++)
        {
            float ang = Random.Range(0f, Mathf.PI * 2f);
            float size = Random.Range(10f, 36f);
            Color col = Random.value < 0.1f ? Color.white : (Random.value < 0.5f ? green : purple);
            var img = MakeImage(contentRt, "Voxel" + i, col, new Vector2(size, size), false);
            img.gameObject.SetActive(false);
            voxels.Add(new Voxel
            {
                rt = img.rectTransform,
                img = img,
                dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang) * 0.6f), // achatado: lembra a vista isométrica
                dist = Random.Range(250f, 900f),
                spin = Random.Range(-360f, 360f),
                baseAlpha = col.a
            });
        }

        // Ponto roxo (com brilho)
        var dotGo = new GameObject("Dot", typeof(RectTransform));
        dotRoot = (RectTransform)dotGo.transform;
        dotRoot.SetParent(contentRt, false);
        dotRoot.sizeDelta = Vector2.zero;
        dotGroup = dotGo.AddComponent<CanvasGroup>();
        var glow = MakeImage(dotRoot, "Glow", new Color(purple.r, purple.g, purple.b, 0.25f), new Vector2(80, 80), false);
        glow.rectTransform.anchoredPosition = Vector2.zero;
        var core = MakeImage(dotRoot, "Core", purple, new Vector2(28, 28), false);
        core.rectTransform.anchoredPosition = Vector2.zero;
        dotGo.SetActive(false);

        // Logo (3 camadas: roxo, verde, branco)
        txtPurple = MakeText(contentRt, "LogoPurple", studioName, 190f, purple, new Vector2(0, 40));
        txtGreen = MakeText(contentRt, "LogoGreen", studioName, 190f, green, new Vector2(0, 40));
        txtMain = MakeText(contentRt, "LogoMain", studioName, 190f, Color.white, new Vector2(0, 40));
        txtSub = MakeText(contentRt, "Subtitle", subtitle, 56f, green, new Vector2(0, -120));
        txtPurple.gameObject.SetActive(false);
        txtGreen.gameObject.SetActive(false);
        txtMain.gameObject.SetActive(false);
        txtSub.gameObject.SetActive(false);

        // Scanlines VHS (textura 1x4 repetida)
        var tex = new Texture2D(1, 4, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Point
        };
        tex.SetPixels(new[]
        {
            new Color(0, 0, 0, 0.9f), new Color(0, 0, 0, 0.9f),
            new Color(0, 0, 0, 0f),   new Color(0, 0, 0, 0f)
        });
        tex.Apply();
        var scanGo = new GameObject("Scanlines", typeof(RectTransform));
        var scanRt = (RectTransform)scanGo.transform;
        scanRt.SetParent(root, false);
        Stretch(scanRt);
        scanlines = scanGo.AddComponent<RawImage>();
        scanlines.texture = tex;
        scanlines.uvRect = new Rect(0, 0, 1, 270); // 1080 / 4
        scanlines.color = new Color(1, 1, 1, 0);
        scanlines.raycastTarget = false;

        // Flash branco (por cima de tudo)
        flash = MakeImage(root, "Flash", new Color(1, 1, 1, 0), Vector2.zero, true);
    }

    Image MakeImage(RectTransform parent, string name, Color color, Vector2 size, bool stretch)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        if (stretch) Stretch(rt); else rt.sizeDelta = size;
        return img;
    }

    TextMeshProUGUI MakeText(RectTransform parent, string name, string text, float size, Color color, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.sizeDelta = new Vector2(1700, 300);
        rt.anchoredPosition = pos;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.fontStyle = FontStyles.Bold;
        t.overflowMode = TextOverflowModes.Overflow;
        t.raycastTarget = false;
        if (font) t.font = font;
        return t;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
