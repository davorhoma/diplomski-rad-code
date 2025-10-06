using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Score
{
    [SerializeField] public TMP_Text totalPoints;
    [SerializeField] public TMP_Text wagonPoints;
    [SerializeField] public TMP_Text trunkPoints;
    [SerializeField] public TMP_Text trunksInHand;
}

public class ScoreboardMenu : MonoBehaviour
{
    [SerializeField] private RawImage _playerImage;
    private PlayerObjectController _localPlayer;
    [SerializeField] private GameObject _myPanelBorder;
    [SerializeField] private RectTransform[] _borderPositions;

    [SerializeField] private TMP_Text[] _blueTexts;
    [SerializeField] private TMP_Text[] _redTexts;
    [SerializeField] private TMP_Text[] _greenTexts;
    [SerializeField] private TMP_Text[] _yellowTexts;

    [SerializeField] private Score blueScore;
    [SerializeField] private Score redScore;
    [SerializeField] private Score greenScore;
    [SerializeField] private Score yellowScore;

    private PointCounter _pointCounterScript;

    private CustomNetworkManager manager;

    public CustomNetworkManager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }
            else
            {
                return manager = CustomNetworkManager.singleton as CustomNetworkManager;
            }
        }
    }

    private void Awake()
    {
        if (_playerImage == null)
        {
            Debug.Log("_playerImage == null");
        }
        else if (_playerImage.texture == null)
        {
            Debug.Log("_playerImage.texture == null");
        }
        else if (Manager == null)
        {
            Debug.Log("Manager == null");
        }
        else 
        {
            _playerImage.texture = Manager.GamePlayers[0].PlayerIcon.texture;
            _pointCounterScript = GameObject.FindObjectOfType<PointCounter>();
        }
    }

    private void Start()
    {
        _localPlayer = GameObject.Find("LocalGamePlayer").GetComponent<PlayerObjectController>();
        _myPanelBorder.transform.position = _borderPositions[(int)_localPlayer.PlayerColor].position;
        Debug.Log("POSITIONING BORDER");
    }

    [Client]
    public void UpdateTotalPoints()
    {
        if (_pointCounterScript == null)
        {
            _pointCounterScript = FindObjectOfType<PointCounter>();
        }

        _blueTexts[0].text = _pointCounterScript.BluePoints.ToString();
        //_blueTexts[1].text = _pointCounterScript.BluePoints.ToString();
        //_blueTexts[2].text = _pointCounterScript.BluePoints.ToString();
        //_blueTexts[3].text = _pointCounterScript.BluePoints.ToString();

        _redTexts[0].text = _pointCounterScript.RedPoints.ToString();

        _greenTexts[0].text = _pointCounterScript.GreenPoints.ToString();

        _yellowTexts[0].text = _pointCounterScript.YellowPoints.ToString();

        blueScore.totalPoints.text = _pointCounterScript.BluePoints.ToString();
        redScore.totalPoints.text = _pointCounterScript.RedPoints.ToString();
        greenScore.totalPoints.text = _pointCounterScript.GreenPoints.ToString();
        yellowScore.totalPoints.text = _pointCounterScript.YellowPoints.ToString();
    }

    [Client]
    public void UpdateWagonPoints()
    {
        blueScore.wagonPoints.text = _pointCounterScript.BlueWagonPoints.ToString();
        redScore.wagonPoints.text = _pointCounterScript.RedWagonPoints.ToString();
        greenScore.wagonPoints.text = _pointCounterScript.GreenWagonPoints.ToString();
        yellowScore.wagonPoints.text = _pointCounterScript.YellowWagonPoints.ToString();
    }

    [Client]
    public void UpdateTrunkPoints()
    {
        blueScore.trunkPoints.text = _pointCounterScript.BlueTrunkPoints.ToString();
        redScore.trunkPoints.text = _pointCounterScript.RedTrunkPoints.ToString();
        greenScore.trunkPoints.text = _pointCounterScript.GreenTrunkPoints.ToString();
        yellowScore.trunkPoints.text = _pointCounterScript.YellowTrunkPoints.ToString();
    }

    [Client]
    public void UpdateTrunksInHand()
    {
        blueScore.trunksInHand.text = _pointCounterScript.BlueTrunksInHand.ToString();
        redScore.trunksInHand.text = _pointCounterScript.RedTrunksInHand.ToString();
        greenScore.trunksInHand.text = _pointCounterScript.GreenTrunksInHand.ToString();
        yellowScore.trunksInHand.text = _pointCounterScript.YellowTrunksInHand.ToString();
    }
    
}
