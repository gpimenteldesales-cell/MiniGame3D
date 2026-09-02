using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class ScreenBlurController : MonoBehaviour
{
    [Tooltip("Tempo em segundos pra transição do blur entrar/sair suavemente")]
    public float duracaoTransicao = 0.4f;

    private Volume volume;
    private Coroutine transicaoCoroutine;

    void Awake()
    {
        volume = GetComponent<Volume>();
        volume.weight = 0f; // começa sem blur
    }

    public void AtivarBlur()
    {
        IniciarTransicao(1f);
    }

    public void DesativarBlur()
    {
        IniciarTransicao(0f);
    }

    private void IniciarTransicao(float pesoAlvo)
    {
        if (transicaoCoroutine != null) StopCoroutine(transicaoCoroutine);
        transicaoCoroutine = StartCoroutine(TransicaoPeso(pesoAlvo));
    }

    private IEnumerator TransicaoPeso(float pesoAlvo)
    {
        float pesoInicial = volume.weight;
        float t = 0f;

        while (t < duracaoTransicao)
        {
            t += Time.deltaTime;
            volume.weight = Mathf.Lerp(pesoInicial, pesoAlvo, t / duracaoTransicao);
            yield return null;
        }

        volume.weight = pesoAlvo;
    }
}