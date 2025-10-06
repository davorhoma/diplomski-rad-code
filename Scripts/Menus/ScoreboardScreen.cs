using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreboardScreen : MonoBehaviour
{
    [SerializeField] private GameObject _scoreboardMenu;

    void Update()
    {
        if (Input.GetKey(KeyCode.Tab))
        {
            ShowScoreboard();
        }
        else
        {
            HideScoreboard();
        }
    }

    private void ShowScoreboard()
    {
        _scoreboardMenu.SetActive(true);
    }

    private void HideScoreboard()
    {
        _scoreboardMenu.SetActive(false);
    }
}
