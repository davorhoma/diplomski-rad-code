using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Rotate : NetworkBehaviour
{
    private float _degreesPerSecond = 100f;
    [SerializeField] private Transform _trunkTransform;
    private Vector3 _desiredPosition = new Vector3(90, 180, 180);
    private Vector3 _neutralPosition = new Vector3(0, 10f, 0);
    [SerializeField] private GameObject _trunkTakenPosition;
    private Vector3 _hoverPosition = new Vector3(0, 0.455f, -0.51f);
    private Vector3 _downPosition = new Vector3(0, 0.474f, -0.51f);
    private Vector3 _upPos = new Vector3(0, 0.019f, 0);

    private float targetRotationAngle = 90.0f; // Set your target angle here
    private float _rotationSpeed = 0.5f; // Set the speed of rotation
    public bool _isRotating;
    private float _rotationStep = 90f;

    private float speed = 10f;
    private float _translateSpeed = 50f;
    public bool _positioned = false;
    RaycastHit _raycastHit;
    LayerMask _layerMask;
    GameObject _hitTrunk;
    private float _step;
    private bool _uplifted;

    [SerializeField] TrunkPlacer _trunkPlacerScript;
    [SerializeField] ImageAssigner _imageAssignerScript;
    [SerializeField] ClickTileEvent _clickTileEventScript;
    [SerializeField] WagonInitializer _wagonInitializerScript;
    private PlayerCommandController _playerCommandController;

    [SerializeField] private Image Back;
    [SerializeField] private Image Front;
    [SerializeField] private GameObject _newTrunkPrefab;
    [SerializeField] private GameObject _trunksParent;

    void Start()
    {
        _neutralPosition = _trunkTakenPosition.transform.position;
        _step = speed * Time.deltaTime;
        //_rotationStep = _rotationSpeed;
        _layerMask = LayerMask.GetMask("TrunkLayer");
        _trunkTransform = GetComponent<Transform>();
        _isRotating = true;

        _playerCommandController = NetworkClient.localPlayer.GetComponent<PlayerCommandController>();
    }

    private void Update()
    {
        if (_isRotating)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && _hitTrunk != null && _hitTrunk.name == name)
        {
            int index = int.Parse(Regex.Match(name, @"\d+$").Value) - 1;
            if (_wagonInitializerScript.IsTrunkAmbush(index))
            {
                if (_imageAssignerScript.AtLeastTwoTilesWithDice() == false)
                {
                    Debug.Log("FALSE");
                    return;
                }
                transform.position = _neutralPosition;

                _trunkPlacerScript.TrunkToDisable = this.gameObject;
                _playerCommandController.CmdUseAmbushTrunk(name);

                Debug.Log("asdadsadadsada");
            }
            else if (_wagonInitializerScript.IsTrunkRollAgain(index))
            {
                transform.position = _neutralPosition;

                _playerCommandController.CmdUseRollAgainTrunk(name, GetComponent<NetworkIdentity>().netId);
            }
        }
    }

    void FixedUpdate()
    {
        if (_isRotating)
        {
            return;
        }

        _hitTrunk = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out _raycastHit, Mathf.Infinity, _layerMask))
        {
            _hitTrunk = _raycastHit.collider.gameObject;
            if (_hitTrunk != null)
            {
                if (_hitTrunk.name == gameObject.name && !_uplifted)
                {
                    _uplifted = true;
                    transform.Translate(Vector3.left * _translateSpeed * Time.deltaTime, Space.World);
                }
                else if (_hitTrunk.name != gameObject.name && _uplifted)
                {
                    _uplifted = false;
                    transform.Translate(Vector3.right * _translateSpeed * Time.deltaTime, Space.World);
                }
            }
        }
        else if (_hitTrunk == null && _uplifted)
        {
            _uplifted = false;
            transform.Translate(Vector3.right * _translateSpeed * Time.deltaTime, Space.World);
        }
    }

    [Server]
    public void TakeTrunk(NetworkConnectionToClient client)
    {
        Debug.Log("Take trunk");
        //transform.localPosition = _neutralPosition;
        Debug.Log("client.connectionId: " + client.connectionId);
        //gameObject.SetActive(false);
        //SetObjectVisibility(false);
        GetComponent<NetworkIdentity>().AssignClientAuthority(client);

        RpcSetObjectVisibility(false, client.connectionId);
        TargetSetActive(client);

        //RpcRotateTrunk(client.connectionId);
        //TargetRotateTrunk(client);
        //StartCoroutine(RotateTrunk());
        //Destroy(gameObject);
    }

    [ClientRpc]
    private void RpcSetObjectVisibility(bool visible, int connId)
    {
        Debug.Log("RpcSetObjectVisibility");
        SetObjectVisibility(visible);
    }

    private void SetObjectVisibility(bool visible)
    {
        gameObject.SetActive(visible);
        return;
        Debug.Log("Set object visibility");
        // Disable/Enable renderers for this object and all children
        GetComponent<Renderer>().enabled = visible;
        foreach (var image in GetComponentsInChildren<Image>())
        {
            image.enabled = visible;
        }
    }

    [TargetRpc]
    private void TargetSetActive(NetworkConnectionToClient client)
    {
        SetObjectVisibility(true);
        StartCoroutine(RotateTrunk());
    }

    [ClientRpc]
    private void RpcRotateTrunk(int connectionId)
    {
        if (connectionId != NetworkClient.connection.connectionId)
        {
            gameObject.SetActive(false); // Hide for other players
        }
    }

    [TargetRpc]
    private void TargetRotateTrunk(NetworkConnectionToClient client)
    {
        Debug.Log("NetworkClient.connection.connectionId == clientId");
        Debug.Log("NetworkClient.connection.connectionId: " + NetworkClient.connection.connectionId);
        GameObject newTrunk = Instantiate(_newTrunkPrefab);

        newTrunk.transform.position = new Vector3(0,0,-200);
        newTrunk.transform.SetParent(_trunksParent.transform, false);
        var newTrunkValues = newTrunk.GetComponent<NewTrunkValues>();
        if (newTrunkValues == null) Debug.Log("newTrunkValues == null");
        if (Back.name == null) Debug.Log("Back.name");
        if (Front.name == null) Debug.Log("Front.name");
        if (newTrunkValues != null && Back.name != null && Front.name != null)
        {
            newTrunkValues.AssignImages(name, Back.name, Front.name);
            //newTrunk.transform.position = _neutralPosition;
        }
    }

    public IEnumerator RotateTrunk()
    {
        //_imageAssignerScript.IncreaseTrunks();
        Debug.Log("RotateTrunk");
        transform.position = _neutralPosition;
        while (!(Quaternion.Angle(transform.rotation, Quaternion.Euler(0,0,180)) < 0.1f))
        {
            Debug.Log("ROTATING");
            //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, 180), _rotationStep);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, 
                Quaternion.Euler(0, 0, 180), 
                _rotationStep * Time.deltaTime // Ensure consistent rotation speed
            );
            yield return null;
        }

        _isRotating = false;

        int index = int.Parse(Regex.Match(name, @"\d+$").Value) - 1;
        Debug.Log("index: " + index);
        if (_wagonInitializerScript.IsTrunkBomb(index))
        {
            Debug.Log("DESTROY TRUNK");

            _playerCommandController.CmdUseBombTrunk(index, name, GetComponent<NetworkIdentity>().netId);
            //_trunkPlacerScript.UseBombTrunk(index);
            //_wagonInitializerScript.AddPointsForTrunk(name);
            gameObject.SetActive(false);
        }
        else
        {
            _trunkPlacerScript.PositionTrunkDown(index);
            _playerCommandController.CmdIncreaseTrunks();
            _positioned = true;
        }
    }

    public void FindScripts()
    {
        var scriptObject = GameObject.Find("Script Object");
        _trunkPlacerScript = scriptObject.GetComponent<TrunkPlacer>();
        _imageAssignerScript = scriptObject.GetComponent<ImageAssigner>();
        _clickTileEventScript = scriptObject.GetComponent<ClickTileEvent>();
        _wagonInitializerScript = scriptObject.GetComponent<WagonInitializer>();
    }
}
