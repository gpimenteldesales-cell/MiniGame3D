using UnityEngine;

public class RandomColorNoBlack : MonoBehaviour
{
    [Tooltip("Brilho mínimo para evitar cores muito escuras/pretas (0 a 1)")]
    [Range(0f, 1f)]
    public float minBrightness = 0.3f;

    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    void Start()
    {
        AplicarCorAleatoria();
    }

    public void AplicarCorAleatoria()
    {
        Color corAleatoria;

        do
        {
            corAleatoria = new Color(
                Random.value,
                Random.value,
                Random.value
            );
        }
        while (corAleatoria.grayscale < minBrightness); // evita cores muito escuras (perto do preto)

        if (rend != null)
        {
            // Usa .material para não afetar outros objetos que compartilhem o mesmo material
            rend.material.color = corAleatoria;
        }
    }
}