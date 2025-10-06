using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TTransparencyManager : MonoBehaviour
{
    [SerializeField] private Image _transparentImage;
    [SerializeField] private GameObject[] _blueDice;

    [SerializeField] private Transform _fullyOpaqueParent;
    [SerializeField] private Transform _transparentParent;
    [SerializeField] private Transform _wagonsParent;
    [SerializeField] private GameObject _buttonRoll;
    [SerializeField] private GameObject[] _tooltips;
    private int _currentStep = 0;
    public int CurrentStep => _currentStep;

    [SerializeField] private Image[] _wagons;
    [SerializeField] private GameObject _trunk;
    private TWagonInitializer _tWagonInitializer;

    [SerializeField] private TMP_Text _bluePoints;
    private TDicePositioner _tDicePositioner;
    [SerializeField] private GameObject _finalText;
    [SerializeField] private GameObject _finishButton;

    private Color _outlineColor = Color.white;

    void Start()
    {
        _tWagonInitializer = GetComponent<TWagonInitializer>();
        _tDicePositioner = GetComponent<TDicePositioner>();

        foreach (var item in _blueDice)
        {
            EnableOutline(item);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            RemoveAllOutlines();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            foreach (var item in _blueDice)
            {
                EnableOutline(item);
            }
        }

        if ((_currentStep == 0 || _currentStep == 7 || _currentStep == 8) 
            && Input.GetMouseButtonDown(0))
        {
            NextStep();
        }
    }

    public void NextStep()
    {
        switch (_currentStep)
        {
            case 0:
                _buttonRoll.transform.SetParent(_fullyOpaqueParent, true);
                _tooltips[_currentStep].SetActive(false);
                _tooltips[_currentStep + 1].SetActive(true);
                RemoveAllOutlines();
                break;
            case 1:
                _tooltips[_currentStep].SetActive(false);
                _transparentImage.gameObject.SetActive(false);
                break;
            case 2:
                Debug.Log("HELLO");
                _transparentImage.gameObject.SetActive(true);
                _tooltips[_currentStep].SetActive(true);
                EnableOutline(_blueDice[4]);
                //_blueDice[5].transform.SetParent(_fullyOpaqueParent, true);
                break;
            case 3:
                _wagons[0].transform.SetParent(_fullyOpaqueParent, true);
                _tooltips[_currentStep - 1].SetActive(false);
                _tooltips[_currentStep].SetActive(true);
                _tWagonInitializer.SetImageMaterial(0, MaterialColor.GREEN);
                break;
            case 4:
                _wagons[0].transform.SetParent(_wagonsParent, true);
                _tooltips[_currentStep - 1].SetActive(false);
                _tWagonInitializer.SetImageMaterial(0, MaterialColor.NONE);
                EnableOutline(_blueDice[0]);
                EnableOutline(_blueDice[2]);
                EnableOutline(_blueDice[5]);
                _tooltips[_currentStep].SetActive(true);
                break;
            case 5:
                _wagons[1].transform.SetParent(_fullyOpaqueParent, true);
                _trunk.transform.SetParent(_fullyOpaqueParent, true);
                _tooltips[_currentStep - 1].SetActive(false);
                _tooltips[_currentStep].SetActive(true);
                _tWagonInitializer.SetImageMaterial(1, MaterialColor.GREEN);
                break;
            case 6:
                _wagons[1].transform.SetParent(_transparentParent, true);
                _tooltips[_currentStep - 1].SetActive(false);
                _tWagonInitializer.SetImageMaterial(1, MaterialColor.NONE);
                _trunk.GetComponent<TRotate>().Rotate();
                _tooltips[_currentStep].SetActive(true);
                break;
            case 7:
                _tooltips[_currentStep - 1].SetActive(false);
                _tooltips[_currentStep].SetActive(true);
                _bluePoints.transform.SetParent(_fullyOpaqueParent, true);
                _bluePoints.text = "2";
                _bluePoints.fontSize = 32;
                break;
            case 8:
                _bluePoints.transform.SetParent(_transparentParent, true);
                _bluePoints.fontSize = 24;
                _tooltips[_currentStep - 1].SetActive(false);
                _trunk.SetActive(false);
                _tDicePositioner.RetreatDice(new Rigidbody[] { _blueDice[2].GetComponent<Rigidbody>(), _blueDice[5].GetComponent<Rigidbody>()});
                ActivateFinalText();
                break;
        }

        _currentStep++;
    }

    public void RemoveAllOutlines()
    {
        foreach (var item in _blueDice)
        {
            DisableOutline(item);
        }
    }

    public void EnableOutline(GameObject obj)
    {
        //Debug.Log("Enabling outline");
        foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
        {
            Outline outline = renderer.gameObject.GetComponent<Outline>();

            if (outline != null)
            {
                outline.OutlineColor = _outlineColor;
                if (!outline.enabled)
                    outline.enabled = true;
            }
        }
    }

    public void DisableOutline(GameObject obj)
    {
        //Debug.Log("DISABLING OUTLINE");
        foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
        {
            Outline outline = renderer.gameObject.GetComponent<Outline>();

            if (outline != null)
            {
                outline.OutlineColor = Color.clear;
            }
        }
    }

    public void PlaceDice()
    {
        Debug.Log("PlaceDice");
        NextStep();
    }

    public void ActivateFinalText()
    {
        StartCoroutine(ActivateAfterDelay(1f));
    }

    private IEnumerator ActivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _finalText.SetActive(true);
        _finishButton.SetActive(true);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(Scenes.MainMenu);
    }
}
