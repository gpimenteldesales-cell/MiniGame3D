using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class GridBlock : MonoBehaviour
{
    public Color BlockColor { get; private set; }

    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    /// <summary>
    /// Define a cor visual e lógica do bloco.
    /// </summary>
    public void SetColor(Color color)
    {
        BlockColor = color;
        rend.material.color = color;
    }

    // IMPORTANTE: este collider precisa estar marcado como "Is Trigger".
    // Se o bloco também precisa ser sólido (o player andar em cima dele),
    // adicione um SEGUNDO BoxCollider no mesmo objeto SEM marcar Is Trigger.
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.OnPlayerEnteredBlock(this);
        }
    }
}