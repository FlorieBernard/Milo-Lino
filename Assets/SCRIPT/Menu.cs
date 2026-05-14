using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void PlayGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ResetAndStart();
        else
            SceneManager.LoadScene("Debut");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
