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
    /// Define a cor visual e lógica do bloco.
    public void SetColor(Color color)
    {
        BlockColor = color;
        rend.material.color = color;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.OnPlayerEnteredBlock(this);
        }
    }
}