using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagementMenu : MonoBehaviour
{
    public void CarregarJogo()
    {
        SceneManager.LoadScene("Game");
    }
    public void SairDoJogo()
    {
               Application.Quit();
        Debug.Log("Sair do jogo");
    }
}
