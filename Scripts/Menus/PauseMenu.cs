using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public static bool IsGamePaused = false;
    [SerializeField] private GameObject _pauseMenuUI;
    [SerializeField] private GameObject _settingsMenu;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsGamePaused && !_settingsMenu.activeInHierarchy)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        _pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        IsGamePaused = false;
    }

    public void Pause()
    {
        _pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        IsGamePaused = true;
    }

    public void Settings()
    {
        _settingsMenu.SetActive(true);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(Scenes.MainMenu);
    }
}
