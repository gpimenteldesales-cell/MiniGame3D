using UnityEngine;

public static class ColorUtils
{
    /// <summary>
    /// Gera uma cor RGB aleatória, evitando tons muito próximos do preto.
    /// </summary>
    public static Color RandomColorNoBlack(float minBrightness = 0.3f)
    {
        Color cor;
        do
        {
            cor = new Color(Random.value, Random.value, Random.value);
        }
        while (cor.grayscale < minBrightness);

        return cor;
    }

    /// <summary>
    /// Sorteia uma cor aleatória (sem preto) que fique a pelo menos "distanciaMinima"
    /// de distância da cor de referência. Usado pra garantir que os blocos errados
    /// não fiquem parecidos demais com a cor certa por puro acaso (e pra controlar
    /// a dificuldade: quanto menor a distância mínima, mais parecidas as cores ficam).
    /// </summary>
    public static Color RandomColorDiferente(Color referencia, float distanciaMinima, float minBrightness = 0.3f)
    {
        Color cor;
        int tentativas = 0;

        do
        {
            cor = RandomColorNoBlack(minBrightness);
            tentativas++;
        }
        while (Distancia(cor, referencia) < distanciaMinima && tentativas < 200);

        return cor;
    }

    /// <summary>
    /// Distância entre duas cores (maior diferença entre os canais R, G ou B).
    /// Quanto maior o valor, mais fácil de distinguir a olho nu.
    /// </summary>
    public static float Distancia(Color a, Color b)
    {
        return Mathf.Max(Mathf.Abs(a.r - b.r), Mathf.Abs(a.g - b.g), Mathf.Abs(a.b - b.b));
    }

    /// <summary>
    /// Compara duas cores com uma pequena tolerância (evita erro de ponto flutuante).
    /// </summary>
    public static bool CoresIguais(Color a, Color b, float tolerancia = 0.01f)
    {
        return Distancia(a, b) < tolerancia;
    }
}