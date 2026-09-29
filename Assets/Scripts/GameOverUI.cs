using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Tooltip("Painel que contém MENU e TRY AGAIN")]
    public GameObject painelGameOver;

    [Tooltip("Nome exato da cena do menu")]
    public string nomeCenaMenu = "Menu";

    [Tooltip("ScreenBlurController da cena")]
    public ScreenBlurController blur;

    private void Awake()
    {
        if (painelGameOver != null)
            painelGameOver.SetActive(false);
    }

    public void MostrarGameOver()
    {
        if (painelGameOver != null)
            painelGameOver.SetActive(true);

        if (blur != null)
            blur.AtivarBlur();
    }

    public void TryAgain()
    {
        if (painelGameOver != null)
            painelGameOver.SetActive(false);

        if (blur != null)
            blur.DesativarBlur();

        if (GameManager.Instance != null)
            GameManager.Instance.ReiniciarJogo();
    }

    public void IrParaMenu()
    {
        SceneManager.LoadScene(nomeCenaMenu);
    }
}