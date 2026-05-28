using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pause menu controller. Attach to a persistent GameObject in each game scene.
/// Requires: _container assigned in Inspector (the pause panel root).
///
/// Inspector wiring:
///   _container          → root GameObject of the pause panel
///   _optionsMenu        → OptionsMenu component (optional)
///   Resume button       → calls ResumeButton()
///   Options button      → calls OptionsButton()
///   Main Menu button    → calls MainMenuButton()
///   Quit button         → calls QuitGame()
/// </summary>
public class MenuPause : MonoBehaviour
{
    [SerializeField] private GameObject _container;
    [SerializeField] private OptionsMenu _optionsMenu;

    private bool _isPaused = false;

    private void Start()
    {
        _container.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused) Resume();
            else Pause();
        }
    }

    private void Pause()
    {
        _isPaused = true;
        _container.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Resume()
    {
        _isPaused = false;
        _container.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ResumeButton() => Resume();

    public void OptionsButton() => _optionsMenu?.Show();

    public void MainMenuButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame() => Application.Quit();
}
