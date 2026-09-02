using UnityEngine;

public static class ColorUtils
{
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
        
    public static bool CoresIguais(Color a, Color b, float tolerancia = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) < tolerancia &&
               Mathf.Abs(a.g - b.g) < tolerancia &&
               Mathf.Abs(a.b - b.b) < tolerancia;
    }
}
