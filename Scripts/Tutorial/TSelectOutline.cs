using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TSelectOutline : MonoBehaviour
{
    private GameObject highlight;
    [SerializeField] private GameObject selection;
    private RaycastHit[] raycastHits;
    private RaycastHit _raycastHit;

    // Store the initial outline color
    private Color _outlineColor = Color.cyan;
    private Color _lowOpacityColor = new Color(0, 1, 1, 0.3f);

    private LayerMask _layerMask;
    private List<Rigidbody> highlightedDice = new List<Rigidbody>();
    private List<Rigidbody> selectedDice = new List<Rigidbody>();
    private List<Rigidbody> highlightedJokers = new List<Rigidbody>();

    public GameObject SelectedDiceGroup => selection;
    public bool WasSelected;
    private string jokerName = "JokerGroup";

    // selectionTag can be "Selectable" or "Positioned"
    private string selectionTag;
    private string currentColor;
    public string CurrentPossibleSelectionGroup = "BlueMaskGroup";

    [SerializeField] ImageAssigner _imageAssignerScript;
    [SerializeField] ClickTileEvent _clickTileScript;

    //[SerializeField] private Rigidbody jokerRb;
    [SerializeField] private int jokerNum;
    [SerializeField] private int previousJokerNum = -1;
    public int JokerToPosition = 0;
    //[SerializeField] private Rigidbody previousJokerRb;
    private GameObject previousSelection;
    private Rigidbody previousSelectedRb;
    private bool areFewerJokersHighlighted;
    public int jokerTiles;
    private PlayerCommandController _playerCommandController;

    [SerializeField] private GraphicRaycaster raycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;

    public void SetSelectionTag(string name)
    {
        selectionTag = name;
    }

    public void SetCurrentColor(string color)
    {
        currentColor = color;
    }

    private void Awake()
    {
        selectionTag = "BlueSelectable";
    }

    private void Start()
    {
        eventSystem = EventSystem.current;

        _layerMask = LayerMask.GetMask("DiceLayer");

        var controllers = FindObjectsOfType<PlayerCommandController>();
        if (controllers != null)
        {
            if (controllers.Length > 1)
            {
                foreach (var controller in controllers)
                {
                    if (controller.name == "LocalGamePlayer")
                        _playerCommandController = controller;
                }
            }
            else if (controllers.Length == 1)
            {
                _playerCommandController = FindObjectOfType<PlayerCommandController>();
            }
        }
    }

    void Update()
    {
        //if (selection != null && selection != previousSelection)
        //{
        //    if (selection.name.Contains(jokerName))
        //    {
        //        StartCoroutine(Wait());
        //        //_playerCommandController.CmdOutlineTilesWithDice();
        //        _imageAssignerScript.OutlineTilesWithDice();
        //    }
        //    else
        //    {
        //        StartCoroutine(Wait());
        //        Debug.Log("selection.name: " + selection.name);
        //        //_playerCommandController.CmdOutlineAvailableTile(selection.name);
        //        _imageAssignerScript.OutlineAvailableTile(selection.name);
        //    }
        //}
        //if (selection == null && selection != previousSelection && !IsPointerOverUIImage())
        //{
        //    StartCoroutine(Wait());
        //    _imageAssignerScript.RemoveGreenOutlines();
        //    Debug.Log("WasSelected: " + WasSelected);
        //}

        previousSelection = selection;
        if (highlight != null)
        {
            // Disable outline for the previous highlights
            foreach (var highlightedDie in highlightedDice)
            {
                DisableOutline(highlightedDie.gameObject);
            }
            highlightedDice.Clear();
            highlight = null;
        }

        foreach (var die in highlightedJokers)
        {
            DisableOutline(die.gameObject);
        }
        highlightedJokers.Clear();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // !EventSystem.current.IsPointerOverGameObject() && 
        if (Physics.Raycast(ray, out _raycastHit, Mathf.Infinity, _layerMask))
        {
            Rigidbody hitDie = _raycastHit.rigidbody;

            if (hitDie != null) /*&& hitDie != selection*/
            {
                if (hitDie.gameObject.transform.parent == null) return;
                highlight = hitDie.gameObject.transform.parent.gameObject;
                if (!highlight.name.Contains(CurrentPossibleSelectionGroup)) return;
                //highlight = hitDie.GetComponentInParent<Rigidbody>();
                // You can loop through siblings if the dice are siblings.
                if (highlight != selection)
                {
                    if (hitDie.CompareTag(selectionTag))
                    {
                        highlightedDice.Clear();

                        //jokerRb = null;
                        jokerNum = 0;

                        if (hitDie.transform.parent.name.Contains(jokerName))
                        {
                            foreach (Transform sibling in hitDie.transform.parent)
                            {
                                Rigidbody siblingDie = sibling.GetComponent<Rigidbody>();
                                highlightedDice.Add(siblingDie);
                                EnableOutline(siblingDie.gameObject);
                                if (sibling == hitDie.transform)
                                {
                                    //jokerRb = hitDie.GetComponent<Rigidbody>();
                                    break;
                                }
                                jokerNum++;
                                //JokerToPosition++;
                            }
                        }
                        else
                        {
                            foreach (Transform sibling in hitDie.transform.parent)
                            {
                                Rigidbody siblingDie = sibling.GetComponent<Rigidbody>();
                                highlightedDice.Add(siblingDie);
                                EnableOutline(siblingDie.gameObject);
                            }
                        }
                    }
                    else
                    {
                        //jokerRb = null;
                        jokerNum = 0;

                        highlightedDice.Clear();
                        highlight = null; // Maybe I can comment this. I'm not sure does it do anything.
                    }
                }
                else if (hitDie.transform.parent.name.Contains(jokerName))
                {
                    highlightedDice.Clear();

                    //jokerNum = 0;
                    jokerNum = 0;
                    var hitDieParent = hitDie.transform.parent;
                    for (int i = 0; i < hitDieParent.childCount; i++)
                    {
                        var sibling = hitDieParent.GetChild(i);
                        if (!IsOutlineEnabled(sibling.gameObject))
                        {
                            highlightedJokers.Add(sibling.GetComponent<Rigidbody>());
                            EnableOutline(sibling.gameObject);
                            //EnableLowOpacityOutline(sibling.gameObject);
                        }
                        jokerNum++;
                        //JokerToPosition++;
                        if (sibling == hitDie.transform)
                        {
                            areFewerJokersHighlighted = (i + 1) < selectedDice.Count;
                            if (areFewerJokersHighlighted)
                            {
                                for (int j = i + 1; j < hitDieParent.childCount; j++)
                                {
                                    var siblingToDisableOutline = hitDieParent.GetChild(j);
                                    if (!IsOutlineEnabled(siblingToDisableOutline.gameObject)) break;

                                    DisableOutline(siblingToDisableOutline.gameObject);
                                }
                            }
                            break;
                        }
                    }

                    //foreach (Transform sibling in hitDie.transform.parent)
                    //{
                    //    Rigidbody siblingDie = sibling.GetComponent<Rigidbody>();
                    //    //highlightedDice.Add(siblingDie);
                    //    highlightedJokers.Add(siblingDie);
                    //    EnableOutline(siblingDie.gameObject);
                    //    if (sibling == hitDie.transform)
                    //    {
                    //        //jokerRb = hitDie.GetComponent<Rigidbody>();
                    //        break;
                    //    }
                    //    jokerNum++;
                    //}
                }

                previousSelectedRb = hitDie;
            }
        }
        else if (selection != null && selection.name.Contains(jokerName))
        {
            jokerNum = selectedDice.Count;
            foreach (var sibling in selectedDice)
            {
                EnableOutline(sibling.gameObject);
            }
        }

        // Selection
        if (Input.GetMouseButtonDown(0))
        {
            var image = IsPointerOverUIImage();
            if (image != null && WasSelected)
            {
                WasSelected = true;
                image.GetComponent<TClickTile>()?.PlaceDice();
            }
            else
            {
                WasSelected = false;
            }

            if (highlight)
            {
                Debug.Log("Clicked on " + highlight.name);
                // Deselect the previously selected dice
                if (highlight.name.Contains(jokerName))
                {
                    if (selection != highlight)
                    {
                        Debug.Log("Here");
                        foreach (var dice in selectedDice)
                        {
                            DisableOutline(dice.gameObject);
                        }
                        previousJokerNum = jokerNum;
                        selectedDice.Clear();
                        selectedDice.AddRange(highlightedDice);
                        selection = highlight;
                        highlight = null;
                        highlightedDice.Clear();

                        //_clickTileScript.GroupName = selection.name.Substring(0, selection.name.Length - 1);
                    }
                    else if (highlightedJokers.Count > 0)
                    {
                        Debug.Log("highlightedJokers.Count > 0");
                        previousJokerNum = jokerNum;
                        selectedDice.AddRange(highlightedJokers);
                        selection = highlight; // Not necessary since selection is already Jokers
                        highlight = null;
                        highlightedJokers.Clear();
                        highlightedDice.Clear(); // Probably not necessary since it is already empty
                    }
                    else if (areFewerJokersHighlighted)
                    {
                        Debug.Log("areFewerJokersHighlighted: " + areFewerJokersHighlighted);
                        for (int i = jokerNum; i < selectedDice.Count; i++)
                        {
                            DisableOutline(selectedDice[i].gameObject);
                        }
                        //for (int i = selectedDice.Count - 1; i >= jokerNum; i--)
                        //{
                        //    selectedDice.Remove(selectedDice[i]);
                        //}

                        // Commented for above is the same as the line below
                        selectedDice.RemoveRange(jokerNum, selectedDice.Count - jokerNum);


                        jokerNum = selectedDice.Count;
                        previousJokerNum = jokerNum;
                        selection = highlight; // Not necessary since selection is already Jokers
                        highlight = null;
                        highlightedJokers.Clear();
                        highlightedDice.Clear();
                    }

                    Debug.Log("selectedDice.Count: " + selectedDice.Count + ", JokerToPosition: " + JokerToPosition);
                }
                else if (selection != highlight)
                {
                    Debug.Log("Here");
                    foreach (var dice in selectedDice)
                    {
                        DisableOutline(dice.gameObject);
                    }
                    selectedDice.Clear();
                    selectedDice.AddRange(highlightedDice);
                    selection = highlight;
                    highlight = null;
                    highlightedDice.Clear();

                    //_clickTileScript.GroupName = selection.name.Substring(0, selection.name.Length - 1);

                    previousJokerNum = -1;
                    jokerNum = 0;
                }
                // Select the new highlighted dice
                WasSelected = true;
            }
            else
            {
                // Deselect all dice if no new highlight
                if (selection)
                {
                    Debug.Log("Here");
                    foreach (var dice in selectedDice)
                    {
                        DisableOutline(dice.gameObject);
                    }
                    selectedDice.Clear();
                    selection = null;

                    previousJokerNum = -1;
                    jokerNum = 0;
                }
            }
        }
    }

    private IEnumerator Wait(float time = 0.5f)
    {
        yield return new WaitForSeconds(time);
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

    public bool IsOutlineEnabled(GameObject obj)
    {
        var outline = obj.GetComponent<Outline>();
        return outline.OutlineColor != Color.clear;
    }

    public void EnableLowOpacityOutline(GameObject obj)
    {
        foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
        {
            Outline outline = renderer.gameObject.GetComponent<Outline>();

            if (outline != null)
            {
                outline.OutlineColor = _lowOpacityColor;
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
                Debug.Log("result.gameobject.name: " + go.name);
                Debug.Log("Return true");
                return go;
            }
        }

        //Debug.Log("IsPointerOverUIImage == False");
        return null;
    }
}
