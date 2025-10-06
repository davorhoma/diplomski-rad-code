using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialMenu : MonoBehaviour
{
    public void ToMainMenu()
    {
        gameObject.SetActive(false);
    }
    
    public void StartTutorial()
    {
        SceneManager.LoadScene(Scenes.TutorialAttack);
    }

    public void OnAttackClicked()
    {
        Debug.Log("CLICKED ON IMAGE ATTACK");
        SceneManager.LoadScene(Scenes.TutorialAttack);
    }

    public void OnStealClicked()
    {
        Debug.Log("CLICKED ON STEAL");
        SceneManager.LoadScene(Scenes.TutorialTrunks);
    }
}
