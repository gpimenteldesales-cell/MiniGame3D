using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneManagementMenu : MonoBehaviour
{
    void CarregarJogo()
    {
               SceneManager.LoadScene("Game");
    }
    void SairDoJogo()
    {
               Application.Quit();
    }
}
