using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPause : MonoBehaviour
{
    [SerializeField] private GameObject _container;

    private void Start()
    {
        _container.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _container.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void ResumeButton()
    {
        _container.SetActive(false);
        Time.timeScale = 1;
    }

    public void MainMenuButton()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
