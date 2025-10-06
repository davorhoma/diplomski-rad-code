using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class STTransparencyManager : MonoBehaviour
{
    [SerializeField] private Image _transparentImage;
    [SerializeField] private GameObject[] _blueDice;
    [SerializeField] private GameObject[] _redDice;
    [SerializeField] private Camera _camera;

    [SerializeField] private Transform _fullyOpaqueParent;
    [SerializeField] private Transform _transparentParent;
    [SerializeField] private Transform _wagonsParent;
    [SerializeField] private Transform _trunksParent;
    private Transform _stealWagon;
    [SerializeField] private Image _stealWagonPointImage;
    private Transform _stealTrunk;
    [SerializeField] private GameObject[] _tooltips;
    [SerializeField] private GameObject _stealButton;
    private STWagonInitializer _stWagonInitializer;
    private STDicePositioner _stDicePositioner;

    private int _currentStep = 0;
    private Color _outlineColor = Color.white;

    [SerializeField] private GraphicRaycaster raycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;

    [SerializeField] private GameObject _finalText;
    [SerializeField] private GameObject _finishButton;
    [SerializeField] private TMP_Text _bluePoints;

    private void Start()
    {
        _stWagonInitializer = GetComponent<STWagonInitializer>();
        _stDicePositioner = GetComponent<STDicePositioner>();
        _stealWagon = _wagonsParent.GetChild(1);
        _stealWagon.SetParent(_fullyOpaqueParent, true);
        _stealTrunk = _trunksParent.GetChild(1);
        _stealTrunk.SetParent(_fullyOpaqueParent, true);

        _camera.gameObject.SetActive(true);

        for (int i = 4; i < 7; i++)
        {
            EnableOutline(_blueDice[i]);
        }

        for (int i = 5; i < 7; i++)
        {
            EnableOutline(_redDice[i]);
        }
    }

    private void Update()
    {
        if ((_currentStep == 0 || _currentStep == 1) 
            && Input.GetMouseButtonDown(0))
        {
            NextStep();
        }

        if (_currentStep == 3 && Input.GetMouseButtonDown(0))
        {
            var image = IsPointerOverUIImage();
            if (image != null && image.name.Contains("Wagon2"))
            {
                NextStep();
            }
        }
    }

    public void NextStep()
    {
        switch (_currentStep)
        {
            case 0:
                _tooltips[_currentStep].SetActive(false);
                _tooltips[_currentStep + 1].SetActive(true);
                RemoveAllOutlines();
                _camera.gameObject.SetActive(false);
                _stealWagon.SetParent(_wagonsParent, true);
                _stealTrunk.SetParent(_trunksParent, true);
                _stealWagonPointImage.enabled = true;
                break;
            case 1:
                _tooltips[_currentStep].SetActive(false);
                _tooltips[_currentStep + 1].SetActive(true);
                _stealButton.transform.SetParent(_fullyOpaqueParent, true);
                _stealWagonPointImage.enabled = false;
                break;
            case 2:
                _tooltips[_currentStep].SetActive(false);
                _stealWagon.SetParent(_fullyOpaqueParent, true);
                _stealTrunk.SetParent(_fullyOpaqueParent, true);
                _stWagonInitializer.SetImageMaterial(1, MaterialColor.RED);
                _camera.gameObject.SetActive(true);
                _stealButton.transform.SetParent(_wagonsParent, true);
                break;
            case 3:
                Destroy(_stealWagon.gameObject);
                Destroy(_stealTrunk.gameObject);
                _camera.gameObject.SetActive(false);
                _stDicePositioner.MoveDiceToCorner(_blueDice[0].transform.parent, 4);
                _stDicePositioner.MoveDiceToCorner(_redDice[0].transform.parent, 5);
                _stWagonInitializer.ShiftWagons();
                _bluePoints.transform.SetParent(_fullyOpaqueParent, true);
                _bluePoints.text = "2";
                _bluePoints.fontSize = 32;
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

        foreach (var item in _redDice)
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

    private GameObject IsPointerOverUIImage()
    {
        pointerEventData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        int tilesLayer = LayerMask.NameToLayer("TilesLayer");
        foreach (var result in results)
        {
            var go = result.gameObject;
            if (go != null && go.layer == tilesLayer)
            {
                //Debug.Log("result.gameobject.name: " + go.name);
                //Debug.Log("Return true");
                return go;
            }
        }

        //Debug.Log("IsPointerOverUIImage == False");
        return null;
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
        _bluePoints.transform.SetParent(_transparentParent, true);
        _bluePoints.fontSize = 24;
    }
}
