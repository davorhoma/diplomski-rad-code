using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject _settingsMenu;
    [SerializeField] private GameObject _rulesCanvas;
    [SerializeField] private GameObject _tutorialOptionsCanvas;

    public void Settings()
    {
        _settingsMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Rules()
    {
        _rulesCanvas.SetActive(true);
    }

    public void StartTutorial()
    {
        _tutorialOptionsCanvas.SetActive(true);
        //SceneManager.LoadScene("Tutorial");
    }
}
