using UnityEngine;

public class MenuPause : MonoBehaviour
{
    public GameObject container;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            container.SetActive(true);
            Time.timeScale = 0;

        }
    }

    public void ResumeButton()
    {
        container.SetActive(false);
        Time.timeScale = 1;

    }

    public void MainMenuButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {

        Application.Quit();
    }
}
