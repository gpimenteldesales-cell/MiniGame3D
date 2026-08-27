using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 6f;

    [Tooltip("Se marcado, o player gira suavemente pra encarar a direção que está andando")]
    public bool virarNaDirecao = true;
    public float velocidadeGiro = 720f; // graus por segundo

    private Rigidbody rb;
    private Vector3 direcaoMovimento;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Trava a rotação física pra não tombar (o giro visual é feito manualmente abaixo)
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Lê o input em Update (mais responsivo), aplica o movimento em FixedUpdate
        float h = Input.GetAxisRaw("Horizontal"); // A/D ou setas
        float v = Input.GetAxisRaw("Vertical");   // W/S ou setas

        // Movimento no plano X/Z (chão), ignorando a câmera (world-space)
        direcaoMovimento = new Vector3(h, 0f, v);
        if (direcaoMovimento.sqrMagnitude > 1f)
            direcaoMovimento.Normalize();
    }

    void FixedUpdate()
    {
        // Move o Rigidbody de forma segura com a física (colisores e triggers continuam funcionando)
        Vector3 novaPosicao = rb.position + direcaoMovimento * velocidade * Time.fixedDeltaTime;
        rb.MovePosition(novaPosicao);

        // Gira o player suavemente na direção do movimento
        if (virarNaDirecao && direcaoMovimento.sqrMagnitude > 0.01f)
        {
            Quaternion rotacaoAlvo = Quaternion.LookRotation(direcaoMovimento, Vector3.up);
            Quaternion novaRotacao = Quaternion.RotateTowards(rb.rotation, rotacaoAlvo, velocidadeGiro * Time.fixedDeltaTime);
            rb.MoveRotation(novaRotacao);
        }
    }
}