using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class GridBlock : MonoBehaviour
{
    public Color BlockColor { get; private set; }

    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    public void SetColor(Color color)
    {
        BlockColor = color;
        rend.material.color = color;
    }

    public void SetVisualColor(Color color)
    {
        rend.material.color = color;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogError(
                $"GridBlock '{name}': GameManager não encontrado."
            );

            return;
        }

        GameManager.Instance.OnPlayerEnteredBlock(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (GameManager.Instance == null)
            return;

        GameManager.Instance.OnPlayerExitedBlock(this);
    }
}