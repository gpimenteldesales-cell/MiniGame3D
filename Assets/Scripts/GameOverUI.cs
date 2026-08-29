using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Tooltip("Painel (GameObject) que contém os botões MENU e TRY AGAIN")]
    public GameObject painelGameOver;

    [Tooltip("Nome EXATO da cena do menu principal (precisa estar no Build Settings)")]
    public string nomeCenaMenu = "Menu";

    [Tooltip("Referência ao ScreenBlurController da cena")]
    public ScreenBlurController blur;

    void Awake()
    {
        if (painelGameOver != null)
            painelGameOver.SetActive(false);
    }

    /// <summary>
    /// Ligue isso no evento "On Round Lost" (ou "On Game Over") do GameManager, no Inspector.
    /// </summary>
    public void MostrarGameOver()
    {
        if (painelGameOver != null)
            painelGameOver.SetActive(true);

        if (blur != null)
            blur.AtivarBlur();
    }

    /// <summary>
    /// Ligue isso no OnClick do botão "TRY AGAIN".
    /// </summary>
    public void TryAgain()
    {
        if (painelGameOver != null)
            painelGameOver.SetActive(false);

        if (blur != null)
            blur.DesativarBlur();

        GameManager.Instance.ReiniciarJogo();
    }

    /// <summary>
    /// Ligue isso no OnClick do botão "MENU".
    /// </summary>
    public void IrParaMenu()
    {
        SceneManager.LoadScene(nomeCenaMenu);
    }
}