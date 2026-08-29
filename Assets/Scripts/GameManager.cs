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

    [Header("Feedback visual (piscar)")]
    public Color corPadraoBlocos = Color.white;
    public int quantidadePiscadas = 3;
    public float intervaloPiscada = 0.15f;
    [Tooltip("Pausa depois da vitória, antes de sortear a próxima rodada")]
    public float pausaAposVitoria = 0.5f;

    [Header("Eventos (opcional: ligue UI, sons, animações, etc.)")]
    public UnityEvent onRoundWon;
    public UnityEvent onRoundLost;
    public UnityEvent onGameOver;

    private List<GridBlock> blocks = new List<GridBlock>();
    private GridBlock currentBlock;
    private Color targetColor;
    private float tempoAtual;
    private Coroutine timerCoroutine;
    private Coroutine loopDerrotaCoroutine;
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
        currentBlock = null;

        // Sorteia a cor alvo e aplica no fundo da câmera (Clear Flags precisa estar em "Solid Color")
        targetColor = ColorUtils.RandomColorNoBlack(minBrightness);
        Camera.main.backgroundColor = targetColor;

        // Sorteia uma cor aleatória pra cada bloco
        foreach (var block in blocks)
        {
            block.SetColor(ColorUtils.RandomColorNoBlack(minBrightness));
        }

        // Garante que pelo menos um bloco tenha exatamente a cor certa
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

        // Tempo acabou: verifica o bloco em que o player está parado NESSE EXATO MOMENTO
        bool acertou = currentBlock != null && ColorUtils.CoresIguais(currentBlock.BlockColor, targetColor);

        if (acertou)
            StartCoroutine(VitoriaRoutine());
        else
            StartCoroutine(DerrotaRoutine());
    }

    /// <summary>
    /// Chamado pelo GridBlock quando o player entra em um bloco (só atualiza o "bloco atual", não decide nada sozinho).
    /// </summary>
    public void OnPlayerEnteredBlock(GridBlock block)
    {
        currentBlock = block;
    }

    /// <summary>
    /// Chamado pelo GridBlock quando o player sai de um bloco.
    /// </summary>
    public void OnPlayerExitedBlock(GridBlock block)
    {
        if (currentBlock == block)
            currentBlock = null;
    }

    private IEnumerator VitoriaRoutine()
    {
        roundActive = false;

        yield return PiscarBlocos(Color.green);

        // Volta todos os blocos pra cor padrão antes da próxima rodada
        foreach (var block in blocks)
            block.SetColor(corPadraoBlocos);

        onRoundWon?.Invoke();

        // Aumenta a dificuldade gradualmente (tempo mais curto a cada rodada)
        tempoAtual = Mathf.Max(tempoMinimo, tempoAtual * reducaoPorRodada);

        yield return new WaitForSeconds(pausaAposVitoria);

        NovaRodada();
    }

    private IEnumerator DerrotaRoutine()
    {
        roundActive = false;

        onRoundLost?.Invoke();
        onGameOver?.Invoke();

        // Pisca vermelho pra sempre, até o jogador chamar ReiniciarJogo()
        loopDerrotaCoroutine = StartCoroutine(PiscarBlocosInfinito(Color.red));

        yield break;
    }

    /// <summary>
    /// Igual ao PiscarBlocos, mas nunca para sozinho (usado no Game Over).
    /// </summary>
    private IEnumerator PiscarBlocosInfinito(Color corFeedback)
    {
        while (true)
        {
            foreach (var block in blocks)
                block.SetVisualColor(corFeedback);

            yield return new WaitForSeconds(intervaloPiscada);

            foreach (var block in blocks)
                block.SetVisualColor(block.BlockColor);

            yield return new WaitForSeconds(intervaloPiscada);
        }
    }

    /// <summary>
    /// Pisca todos os blocos alternando entre a cor de feedback e a cor original de cada um.
    /// </summary>
    private IEnumerator PiscarBlocos(Color corFeedback)
    {
        for (int i = 0; i < quantidadePiscadas; i++)
        {
            foreach (var block in blocks)
                block.SetVisualColor(corFeedback);

            yield return new WaitForSeconds(intervaloPiscada);

            foreach (var block in blocks)
                block.SetVisualColor(block.BlockColor);

            yield return new WaitForSeconds(intervaloPiscada);
        }
    }

    /// <summary>
    /// Chame isso num botão de "Reiniciar" pra recomeçar do zero.
    /// </summary>
    public void ReiniciarJogo()
    {
        if (loopDerrotaCoroutine != null)
        {
            StopCoroutine(loopDerrotaCoroutine);
            loopDerrotaCoroutine = null;
        }

        tempoAtual = tempoInicial;
        rodada = 0;
        NovaRodada();
    }
}