using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Música")]
    public AudioClip musicaJogo;

    [Header("Efeito de derrota (estilo Geometry Dash)")]
    [Tooltip("Som extra de \"quebra\" tocado no momento da derrota (opcional, pode deixar vazio)")]
    public AudioClip somDeQuebra;
    [Tooltip("Pitch mínimo antes de parar de vez (1 = normal, quanto menor mais grave/lento)")]
    [Range(0f, 1f)] public float pitchMinimoAoQuebrar = 0.3f;
    [Tooltip("Duração da queda de pitch/volume, em segundos")]
    public float duracaoQuebra = 0.3f;

    private AudioSource fonteMusica;
    private AudioSource fonteEfeito;
    private Coroutine quebraCoroutine;

    void Awake()
    {
        Instance = this;

        fonteMusica = GetComponent<AudioSource>();
        fonteMusica.clip = musicaJogo;
        fonteMusica.loop = true;
        fonteMusica.playOnAwake = false;

        // Fonte separada só pro som de quebra, pra não cortar a música ao tocar o efeito
        fonteEfeito = gameObject.AddComponent<AudioSource>();
        fonteEfeito.playOnAwake = false;
    }

    void Start()
    {
        TocarMusica();
    }

    /// <summary>
    /// Toca a música do começo, com pitch e volume normais.
    /// Ligue isso no botão TRY AGAIN (via GameOverUI, já está pronto) ou chame manualmente.
    /// </summary>
    public void TocarMusica()
    {
        if (quebraCoroutine != null)
        {
            StopCoroutine(quebraCoroutine);
            quebraCoroutine = null;
        }

        fonteMusica.pitch = 1f;
        fonteMusica.volume = 1f;
        fonteMusica.time = 0f;
        fonteMusica.Play();
    }

    /// <summary>
    /// Ligue isso no evento "On Round Lost" do GameManager (Inspector).
    /// A música "quebra": pitch cai, volume some, e para — tipo Geometry Dash.
    /// </summary>
    public void QuebrarMusica()
    {
        if (quebraCoroutine != null) StopCoroutine(quebraCoroutine);
        quebraCoroutine = StartCoroutine(QuebrarMusicaRoutine());
    }

    private IEnumerator QuebrarMusicaRoutine()
    {
        if (somDeQuebra != null)
            fonteEfeito.PlayOneShot(somDeQuebra);

        float pitchInicial = fonteMusica.pitch;
        float volumeInicial = fonteMusica.volume;
        float t = 0f;

        while (t < duracaoQuebra)
        {
            t += Time.deltaTime;
            float progresso = t / duracaoQuebra;
            fonteMusica.pitch = Mathf.Lerp(pitchInicial, pitchMinimoAoQuebrar, progresso);
            fonteMusica.volume = Mathf.Lerp(volumeInicial, 0f, progresso);
            yield return null;
        }

        fonteMusica.Stop();
        fonteMusica.pitch = 1f; // já deixa pronto pro próximo TocarMusica()
    }
}
