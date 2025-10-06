using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrTTransparencyManager : MonoBehaviour
{
    [SerializeField] private Transform _fullyOpaqueParent;
    [SerializeField] private Transform _transparentParent;
    [SerializeField] private Transform _wagonsParent;
    [SerializeField] private Transform _trunksParent;
    [SerializeField] private Transform _blueDiceParent;
    private Transform _wagon;
    private Transform _trunk;
    [SerializeField] private GameObject _buttonRetreat;
    [SerializeField] private GameObject[] _tooltips;
    private int _currentStep = 0;
    public int CurrentStep => _currentStep;

    [SerializeField] private Transform _retreatDie;
    [SerializeField] private Camera _secondWagonCamera;
    [SerializeField] private Camera _firstWagonCamera;

    private TrTDicePositioner _trTDicePositioner;

    private Color _outlineColor = Color.cyan;
    private Color _whiteOutlineColor = Color.white;

    void Start()
    {
        _trTDicePositioner = GetComponent<TrTDicePositioner>();

        _secondWagonCamera.gameObject.SetActive(true);
        _wagon = _wagonsParent.GetChild(1).transform;
        _trunk = _trunksParent.GetChild(1).transform;
        _wagon.SetParent(_fullyOpaqueParent, true);
        _trunk.SetParent(_fullyOpaqueParent, true);
    }

    private void Update()
    {
        if ((_currentStep == 0 || _currentStep == 1) 
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
                _tooltips[_currentStep].SetActive(false);
                _tooltips[_currentStep + 1].SetActive(true);

                _retreatDie.SetParent(_fullyOpaqueParent, true);
                EnableOutline(_retreatDie.gameObject, _whiteOutlineColor);

                _secondWagonCamera.gameObject.SetActive(false);
                _firstWagonCamera.gameObject.SetActive(true);

                _wagon.SetParent(_wagonsParent, true);
                _trunk.SetParent(_trunksParent, true);
                break;
            case 1:
                _tooltips[_currentStep].SetActive(false);
                _tooltips[_currentStep + 1].SetActive(true);
                _buttonRetreat.transform.SetParent(_fullyOpaqueParent, true);

                _retreatDie.SetParent(_transparentParent, true);
                DisableOutline(_retreatDie.gameObject);

                _firstWagonCamera.gameObject.SetActive(false);  
                break;
            case 2:
                _tooltips[_currentStep].SetActive(false);
                _tooltips[_currentStep + 1].SetActive(true);

                _retreatDie.SetParent(_fullyOpaqueParent, true);
                EnableOutline(_retreatDie.gameObject, _outlineColor);
                
                _firstWagonCamera.gameObject.SetActive(true);  
                break;
            case 3:
                _tooltips[_currentStep].SetActive(false);
                _retreatDie.SetParent(_blueDiceParent, true);
                DisableOutline(_retreatDie.gameObject);
                _trTDicePositioner.MoveDiceToCorner();
                _firstWagonCamera.gameObject.SetActive(false);
                break;
        }

        _currentStep++;
    }

    public void Retreat()
    {
        NextStep();
    }

    public void EnableOutline(GameObject obj, Color outlineColor)
    {
        //Debug.Log("Enabling outline");
        foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
        {
            Outline outline = renderer.gameObject.GetComponent<Outline>();

            if (outline != null)
            {
                outline.OutlineColor = outlineColor;
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
}
