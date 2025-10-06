using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[System.Serializable]
public struct TrunkSides
{
    [SerializeField] public Sprite Front;
    [SerializeField] public Sprite Back;
}

public class WagonInitializer : NetworkBehaviour
{
    public Image[] wagonObjects;
    public Sprite[] wagonSprites;
    [SyncVar]
    private int[] spriteIndices = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

    [SyncVar(hook = nameof(OnSpriteIndicesChanged))]
    private string serializedSpriteIndices;

    [SerializeField] private Image[] trunkBackImages;
    [SerializeField] private Image[] trunkFrontImages;

    [FormerlySerializedAs("_imageProperties")]
    [SerializeField] Wagon[] _wagons;

    [SerializeField] Trunk[] _trunks;

    [SerializeField] public TrunkSides[] _trunkSides;
    private int[] trunkSideSpriteIndices = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };

    [SyncVar(hook = nameof(OnTrunkSideSpriteIndicesChanged))]
    private string serializedTrunkSideSpriteIndices;

    [SerializeField] private PointCounter _pointCounterScript;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ShuffleTrunks();
        }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject); // Keep this object across scenes
        if (isServer)
        {
            ShuffleTrunks();
        }
    }

    private void ShuffleTrunks()
    {
        Debug.Log("Shuffling arrays");
        ShuffleArray(spriteIndices);
        serializedSpriteIndices = string.Join(",", spriteIndices);
        //RpcAssignSpritesToWagons(spriteIndices);

        ShuffleArray(trunkSideSpriteIndices);
        serializedTrunkSideSpriteIndices = string.Join(", ", trunkSideSpriteIndices);
    }

    void ShuffleArray<T>(T[] array)
    {
        int n = array.Length;
        for (int i = n - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            // Swap array[i] and array[j]
            T temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    [ClientRpc]
    private void RpcAssignSpritesToWagons(int[] shuffledIndices)
    {
        Debug.Log("Assigning sprites to wagons.");
        for (int i = 0; i < wagonSprites.Length; i++)
        {
            wagonObjects[i].sprite = wagonSprites[shuffledIndices[i]];
        }
    }

    private void OnSpriteIndicesChanged(string oldValue, string newValue)
    {
        // Deserialize indices and update the UI when SyncVar changes
        spriteIndices = System.Array.ConvertAll(newValue.Split(','), int.Parse);
        AssignSpritesToWagons();
    }

    private void OnTrunkSideSpriteIndicesChanged(string oldValue, string newValue)
    {
        trunkSideSpriteIndices = System.Array.ConvertAll(newValue.Split(','), int.Parse);
        AssignSpritesToTrunks();
    }

    private void AssignSpritesToWagons()
    {
        Debug.Log("Assigning.");
        if (wagonObjects.Length == 0) return; // Ensure wagonObjects are properly assigned
        for (int i = 0; i < wagonObjects.Length; i++)
        {
            wagonObjects[i].sprite = wagonSprites[spriteIndices[i]];

            AssignImageProperties(i, wagonObjects[i].sprite.name);
        }

        _wagons[0].Last = true;
        _wagons[16].AssignedNumber = 5;
    }

    private void AssignSpritesToTrunks()
    {
        Debug.Log("Assigning back trunks.");
        if (trunkBackImages.Length == 0) return; // Ensure wagonObjects are properly assigned
        for (int i = 0; i < trunkBackImages.Length; i++)
        {
            trunkBackImages[i].sprite = _trunkSides[trunkSideSpriteIndices[i]].Back;
            trunkFrontImages[i].sprite = _trunkSides[trunkSideSpriteIndices[i]].Front;

            var nameBack = _trunkSides[trunkSideSpriteIndices[i]].Back.name;
            trunkBackImages[i].name = nameBack.Substring(nameBack.IndexOf("_") + 1);

            var nameFront = _trunkSides[trunkSideSpriteIndices[i]].Front.name;
            trunkFrontImages[i].name = nameFront;

            Enum.TryParse(nameFront[..^1], ignoreCase: true, out _trunks[i].Type);
            //Debug.Log("_trunks[" + i + "].Type = " + _trunks[i].Type + ", nameFront[..1]: " + nameFront[..^1]);
            _trunks[i].Points = int.Parse(nameFront[^1].ToString());
            //Debug.Log("_trunks[" + i + "].Points = " + _trunks[i].Points);
            Enum.TryParse(trunkBackImages[i].name, ignoreCase: true, out _trunks[i].Symbol);
            //Debug.Log("_trunks[" + i + "].Symbol = " + _trunks[i].Symbol + ", back: " + trunkBackImages[i].name);
            _wagons[i].AssignedTrunk = _trunks[i];
        }
    }

    private void AssignImageProperties(int i, string wagonName)
    {
        switch (wagonName)
        {
            case "Wagon1":
                _wagons[i].AssignedNumber = 1;
                break;
            case "Wagon2":
                _wagons[i].AssignedNumber = 2;
                break;
            case "Wagon3":
                _wagons[i].AssignedNumber = 3;
                break;
            case "Wagon4":
                _wagons[i].AssignedNumber = 4;
                break;
        }
    }

    public bool IsTrunkBomb(int index)
    {
        if (trunkFrontImages[index] == null)
        {
            Debug.Log("trunkFrontImages[index] == null");
            var trunk = GameObject.Find("Trunk" + index);
            if (trunk == null) return false;
            var trunkValues = trunk.GetComponent<NewTrunkValues>();
            trunkBackImages[index] = trunkValues.Back;
            trunkFrontImages[index] = trunkValues.Front;

            //if (trunkValues.Front.name.StartsWith("Bomb"))
            if (_trunks[index].Type == TrunkType.BOMB)
            {
                return true;
            }
        }

        //if (trunkFrontImages[index].name.StartsWith("Bomb"))
        if (_trunks[index].Type == TrunkType.BOMB)
        {
            return true;
        }
        else
            return false;
    }

    public bool IsTrunkAmbush(int index)
    {
        //print(_trunkImages[index]._trunkFront.name);
        //if (trunkFrontImages[index].name.StartsWith("Ambush"))
        if (_trunks[index].Type == TrunkType.AMBUSH)
        {
            return true;
        }
        else
            return false;
    }

    public bool IsTrunkRollAgain(int index)
    {
        //print(_trunkImages[index]._trunkFront.name);
        //if (trunkFrontImages[index].name.StartsWith("Roll"))
        if (_trunks[index].Type == TrunkType.ROLL_AGAIN)
        {
            return true;
        }
        else
            return false;
    }

    [Server]
    public void AddPointsForTrunk(string name)
    {
        int index = int.Parse(name[^1].ToString());

        var points = int.Parse(trunkFrontImages[index - 1].name[^1].ToString());
        _pointCounterScript.AddTrunkPoints(points);
    }
}
