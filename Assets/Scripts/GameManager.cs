using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuração")]
    [Tooltip("Objeto pai que contém todos os blocos da fase")]
    public Transform blocksParent;

    [Tooltip("Brilho mínimo das cores sorteadas")]
    [Range(0f, 1f)]
    public float minBrightness = 0.3f;

    [Header("Dificuldade")]
    public float tempoInicial = 3f;
    public float tempoMinimo = 0.6f;

    [Range(0.5f, 0.99f)]
    public float reducaoPorRodada = 0.95f;

    [Header("Feedback visual")]
    public Color corPadraoBlocos = Color.white;

    public int quantidadePiscadas = 3;
    public float intervaloPiscada = 0.15f;

    [Tooltip("Pausa depois da vitória antes da próxima rodada")]
    public float pausaAposVitoria = 0.5f;

    [Header("Eventos")]
    public UnityEvent onRoundWon;
    public UnityEvent onRoundLost;
    public UnityEvent onGameOver;

    private readonly List<GridBlock> blocks = new List<GridBlock>();

    // Blocos atualmente ocupados pelo Player.
    private readonly List<GridBlock> currentBlocks = new List<GridBlock>();

    private GridBlock currentBlock;

    private Color targetColor;
    private float tempoAtual;

    private Coroutine timerCoroutine;
    private Coroutine loopDerrotaCoroutine;

    private bool roundActive;
    private int rodada;

    private void Awake()
    {
        // Evita dois GameManagers na mesma cena.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (blocksParent == null)
        {
            Debug.LogError(
                "GameManager: blocksParent não foi definido no Inspector."
            );

            enabled = false;
            return;
        }

        blocks.AddRange(
            blocksParent.GetComponentsInChildren<GridBlock>()
        );

        if (blocks.Count == 0)
        {
            Debug.LogError(
                "GameManager: nenhum GridBlock foi encontrado em blocksParent."
            );

            enabled = false;
            return;
        }

        tempoAtual = tempoInicial;
    }

    private void Start()
    {
        NovaRodada();
    }

    public void NovaRodada()
    {
        rodada++;
        roundActive = true;

        // IMPORTANTE:
        // Não limpamos currentBlocks aqui.
        // O Player pode continuar sobre o mesmo bloco entre rodadas.

        if (Camera.main == null)
        {
            Debug.LogError(
                "GameManager: nenhuma câmera com a tag MainCamera foi encontrada."
            );
        }
        else
        {
            targetColor =
                ColorUtils.RandomColorNoBlack(minBrightness);

            Camera.main.backgroundColor = targetColor;
        }

        // Sorteia as cores dos blocos.
        foreach (GridBlock block in blocks)
        {
            block.SetColor(
                ColorUtils.RandomColorNoBlack(minBrightness)
            );
        }

        // Garante que exista pelo menos um bloco correto.
        int indiceCorreto = Random.Range(0, blocks.Count);

        blocks[indiceCorreto].SetColor(targetColor);

        Debug.Log(
            $"Rodada {rodada} iniciada | " +
            $"Tempo: {tempoAtual:F2}s | " +
            $"Bloco correto: {blocks[indiceCorreto].name}"
        );

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }

        timerCoroutine = StartCoroutine(TimerRodada());
    }

    private IEnumerator TimerRodada()
    {
        yield return new WaitForSeconds(tempoAtual);

        if (!roundActive)
            yield break;

        bool acertou =
            currentBlock != null &&
            ColorUtils.CoresIguais(
                currentBlock.BlockColor,
                targetColor
            );

        if (acertou)
        {
            StartCoroutine(VitoriaRoutine());
        }
        else
        {
            StartCoroutine(DerrotaRoutine());
        }
    }

    // =========================================================
    // PLAYER ENTROU
    // =========================================================

    public void OnPlayerEnteredBlock(GridBlock block)
    {
        if (block == null)
            return;

        if (!currentBlocks.Contains(block))
        {
            currentBlocks.Add(block);
        }

        currentBlock = block;

        Debug.Log(
            $"Entrou em {block.name} | " +
            $"Cor: {block.BlockColor}"
        );
    }

    // =========================================================
    // PLAYER SAIU
    // =========================================================

    public void OnPlayerExitedBlock(GridBlock block)
    {
        if (block == null)
            return;

        currentBlocks.Remove(block);

        if (currentBlock == block)
        {
            if (currentBlocks.Count > 0)
            {
                currentBlock =
                    currentBlocks[currentBlocks.Count - 1];
            }
            else
            {
                currentBlock = null;
            }
        }
    }

    // =========================================================
    // VITÓRIA
    // =========================================================

    private IEnumerator VitoriaRoutine()
    {
        roundActive = false;

        yield return PiscarBlocos(Color.green);

        foreach (GridBlock block in blocks)
        {
            block.SetColor(corPadraoBlocos);
        }

        onRoundWon?.Invoke();

        tempoAtual = Mathf.Max(
            tempoMinimo,
            tempoAtual * reducaoPorRodada
        );

        yield return new WaitForSeconds(pausaAposVitoria);

        NovaRodada();
    }

    // =========================================================
    // DERROTA
    // =========================================================

    private IEnumerator DerrotaRoutine()
    {
        roundActive = false;

        onRoundLost?.Invoke();
        onGameOver?.Invoke();

        loopDerrotaCoroutine =
            StartCoroutine(
                PiscarBlocosInfinito(Color.red)
            );

        yield break;
    }

    private IEnumerator PiscarBlocosInfinito(Color corFeedback)
    {
        while (true)
        {
            foreach (GridBlock block in blocks)
            {
                block.SetVisualColor(corFeedback);
            }

            yield return new WaitForSeconds(intervaloPiscada);

            foreach (GridBlock block in blocks)
            {
                block.SetVisualColor(block.BlockColor);
            }

            yield return new WaitForSeconds(intervaloPiscada);
        }
    }

    private IEnumerator PiscarBlocos(Color corFeedback)
    {
        for (int i = 0; i < quantidadePiscadas; i++)
        {
            foreach (GridBlock block in blocks)
            {
                block.SetVisualColor(corFeedback);
            }

            yield return new WaitForSeconds(intervaloPiscada);

            foreach (GridBlock block in blocks)
            {
                block.SetVisualColor(block.BlockColor);
            }

            yield return new WaitForSeconds(intervaloPiscada);
        }
    }

    // =========================================================
    // REINICIAR
    // =========================================================

    public void ReiniciarJogo()
    {
        if (loopDerrotaCoroutine != null)
        {
            StopCoroutine(loopDerrotaCoroutine);
            loopDerrotaCoroutine = null;
        }

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        tempoAtual = tempoInicial;
        rodada = 0;
        roundActive = false;

        NovaRodada();
    }
}