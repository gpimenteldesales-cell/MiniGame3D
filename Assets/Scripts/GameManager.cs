using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuração")]
    [Tooltip("Objeto pai que contém todos os blocos (GridBlock) da fase")]
    public Transform blocksParent;

    [Tooltip("Brilho mínimo das cores sorteadas (evita preto)")]
    [Range(0f, 1f)] public float minBrightness = 0.3f;

    [Header("Dificuldade")]
    public float tempoInicial = 3f;
    public float tempoMinimo = 0.6f;
    [Range(0.5f, 0.99f)] public float reducaoPorRodada = 0.95f;

    [Header("Eventos (opcional: ligue UI, sons, animações, etc.)")]
    public UnityEvent onRoundWon;
    public UnityEvent onRoundLost;
    public UnityEvent onGameOver;

    private List<GridBlock> blocks = new List<GridBlock>();
    private GridBlock currentBlock;
    private Color targetColor;
    private float tempoAtual;
    private Coroutine timerCoroutine;
    private bool roundActive;
    private int rodada = 0;

    void Awake()
    {
        Instance = this;
        blocks.AddRange(blocksParent.GetComponentsInChildren<GridBlock>());
        tempoAtual = tempoInicial;
    }

    void Start()
    {
        NovaRodada();
    }

    public void NovaRodada()
    {
        rodada++;
        roundActive = true;

        // Sorteia a cor alvo e aplica na câmera (Clear Flags precisa estar em "Solid Color")
        targetColor = ColorUtils.RandomColorNoBlack(minBrightness);
        Camera.main.backgroundColor = targetColor;

        // Sorteia uma cor aleatória pra cada bloco
        foreach (var block in blocks)
        {
            block.SetColor(ColorUtils.RandomColorNoBlack(minBrightness));
        }

        // Garante que pelo menos um bloco tenha exatamente a cor certa
        // (senão a rodada seria impossível de vencer)
        int indiceCorreto = Random.Range(0, blocks.Count);
        blocks[indiceCorreto].SetColor(targetColor);

        // Reinicia o timer da rodada
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(TimerRodada());
    }

    private IEnumerator TimerRodada()
    {
        yield return new WaitForSeconds(tempoAtual);

        if (!roundActive) yield break;

        // Tempo acabou: verifica o bloco em que o player está parado
        if (currentBlock != null && ColorUtils.CoresIguais(currentBlock.BlockColor, targetColor))
        {
            RodadaVencida();
        }
        else
        {
            RodadaPerdida();
        }
    }
    /// Chamado pelo GridBlock quando o player entra em um bloco.
    public void OnPlayerEnteredBlock(GridBlock block)
    {
        currentBlock = block;

        if (!roundActive) return;

        if (ColorUtils.CoresIguais(block.BlockColor, targetColor))
        {
            RodadaVencida();
        }
        else
        {
            RodadaPerdida();
        }
    }

    private void RodadaVencida()
    {
        roundActive = false;
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);

        onRoundWon?.Invoke();

        // Aumenta a dificuldade gradualmente (tempo mais curto a cada rodada)
        tempoAtual = Mathf.Max(tempoMinimo, tempoAtual * reducaoPorRodada);

        NovaRodada();
    }

    private void RodadaPerdida()
    {
        roundActive = false;
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);

        onRoundLost?.Invoke();
        onGameOver?.Invoke();
    }
    /// Chame isso num botão de "Reiniciar" pra recomeçar do zero.
    public void ReiniciarJogo()
    {
        tempoAtual = tempoInicial;
        rodada = 0;
        NovaRodada();
    }
}
