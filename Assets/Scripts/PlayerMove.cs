using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 6f;

    [Header("Câmera isométrica")]
    [Tooltip("Câmera usada como referência de direção. Se vazio, usa Camera.main")]
    public Transform referenciaCamera;

    private Rigidbody rb;
    private Vector3 direcaoMovimento;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Trava todas as rotações (X, Y e Z) — o player nunca gira, nem por física nem por script
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (referenciaCamera == null && Camera.main != null)
            referenciaCamera = Camera.main.transform;
    }

    void Update()
    {
        // Lê o input em Update (mais responsivo), aplica o movimento em FixedUpdate
        float h = Input.GetAxisRaw("Horizontal"); // A/D ou setas
        float v = Input.GetAxisRaw("Vertical");   // W/S ou setas

        Vector3 inputBruto = new Vector3(h, 0f, v);
        if (inputBruto.sqrMagnitude > 1f)
            inputBruto.Normalize();

        if (referenciaCamera != null)
        {
            // Pega a direção "pra frente" e "pra direita" da câmera, achatadas no chão (ignora inclinação)
            Vector3 camForward = referenciaCamera.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 camRight = referenciaCamera.right;
            camRight.y = 0f;
            camRight.Normalize();

            // W = "pra frente" na tela, D = "pra direita" na tela, mesmo com a câmera em diagonal
            direcaoMovimento = camForward * inputBruto.z + camRight * inputBruto.x;
        }
        else
        {
            // Fallback: eixos do mundo, caso não tenha câmera de referência
            direcaoMovimento = inputBruto;
        }
    }

    void FixedUpdate()
    {
        // Move o Rigidbody de forma segura com a física (colisores e triggers continuam funcionando)
        Vector3 novaPosicao = rb.position + direcaoMovimento * velocidade * Time.fixedDeltaTime;
        rb.MovePosition(novaPosicao);
    }
}