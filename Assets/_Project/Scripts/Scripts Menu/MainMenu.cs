using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public GameObject AjustesMenu;
    public GameObject mainMenu;

    public void OpenOptionsPanel()
    {
        mainMenu.SetActive(false);
        AjustesMenu.SetActive(true);
    }
    public void OpenmainMenuPanel()
    {
        mainMenu.SetActive(true);
        AjustesMenu.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("CuartoEvangelo");
    }


}
