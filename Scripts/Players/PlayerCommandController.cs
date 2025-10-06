using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCommandController : NetworkBehaviour
{
    private ImageAssigner _imageAssignerScript;
    private TrunkPlacer _trunkPlacerScript;
    private ClickTileEvent _clickTileEventScript;
    private WagonInitializer _wagonInitializerScript;
    private Retreat _retreatScript;
    private PointCounter _pointCounterScript;
    private SelectOutline _selectOutlineScript;

    [Command]
    public void CmdRollDice()
    {
        if (_clickTileEventScript == null)
        {
            //Debug.Log("_clickTileEventScript == null");
            _imageAssignerScript = FindObjectOfType<ImageAssigner>();
            _trunkPlacerScript = FindObjectOfType<TrunkPlacer>();
            _clickTileEventScript = FindObjectOfType<ClickTileEvent>();
            _wagonInitializerScript = FindObjectOfType<WagonInitializer>();
            _retreatScript = FindObjectOfType<Retreat>();
            _pointCounterScript = FindObjectOfType<PointCounter>();
            _selectOutlineScript = FindObjectOfType<SelectOutline>();
        }

        StartCoroutine(ButtonRoll.Instance.ServerRollDice());
        //Debug.Log("ROLLING DICE on the server");

    }

    [Command]
    public void CmdNextPlayer()
    {
        StartCoroutine(TurnManager.Instance.NextPlayerCoroutine());
    }

    [Command]
    public void CmdRetreatDice(uint parentNetId)
    {
        _retreatScript.RetreatDice(parentNetId, connectionToClient);
    }

    [Command]
    public void CmdAttack(int index, uint selectedGroupNetId, string groupName)
    {
        Debug.Log("CmdAttack index:" + index);

        _clickTileEventScript.ServerAttackWagon(index, selectedGroupNetId, groupName, connectionToClient);
    }

    [Command]
    public void CmdSteal(int index)
    {
        _clickTileEventScript.ServerStealWagon(index, connectionToClient);
    }

    [Command]
    public void CmdOutlineTilesWithDice()
    {
        if (!_imageAssignerScript)
        {
            _imageAssignerScript = FindObjectOfType<ImageAssigner>();
        }
        OutlineTilesWithDice();
    }

    private void OutlineTilesWithDice()
    {
        _imageAssignerScript.TargetOutlineTilesWithDice(connectionToClient);
    }

    [Command]
    public void CmdOutlineAvailableTile(string name)
    {
        if (!_imageAssignerScript)
        {
            _imageAssignerScript = FindObjectOfType<ImageAssigner>();
        }
        _imageAssignerScript.ServerOutlineAvailableTile(name, connectionToClient);
    }

    [Command]
    public void CmdSwitchTileDice(int firstTile, int secondTile)
    {
        Debug.Log("CmdSwitchTileDice");
        _imageAssignerScript.SwitchTileDice(connectionToClient, firstTile, secondTile);
    }

    [Command]
    public void CmdUseAmbushTrunk(string name)
    {
        _trunkPlacerScript.AdjustTrunkPositions(name);
        _wagonInitializerScript.AddPointsForTrunk(name);

        _clickTileEventScript.SetAction(PlayerAction.AMBUSH);
        OutlineTilesWithDice();
        _pointCounterScript.UpdateTrunkAmount(-1);
    }

    [Command]
    public void CmdUseRollAgainTrunk(string name, uint objectToDeactivateId)
    {
        _trunkPlacerScript.AdjustTrunkPositions(name);
        _wagonInitializerScript.AddPointsForTrunk(name);

        Debug.Log("used");

        if (!NetworkClient.spawned.TryGetValue(objectToDeactivateId, out var deactivateIdentity))
        {
            Debug.Log("Cannot find selected group by netId");
            return;
        }

        TargetRollAgain(connectionToClient);
        //StartCoroutine(ButtonRoll.Instance.ServerRollDice());
        TargetDisableGameObjectAfterDelay(connectionToClient, objectToDeactivateId);
        //StartCoroutine(DisableGameObjectAfterDelay(deactivateIdentity.gameObject, 1f));
        _pointCounterScript.UpdateTrunkAmount(-1);
    }

    [TargetRpc]
    private void TargetRollAgain(NetworkConnectionToClient conn)
    {
        ButtonRoll.Instance.RollDice();
    }

    private IEnumerator DisableGameObjectAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }

    [TargetRpc]
    private void TargetDisableGameObjectAfterDelay(NetworkConnectionToClient conn, uint objectToDeactivateId)
    {
        if (!NetworkClient.spawned.TryGetValue(objectToDeactivateId, out var deactivateIdentity))
        {
            Debug.Log("Cannot find selected group by netId");
            return;
        }

        StartCoroutine(DisableGameObjectAfterDelay(deactivateIdentity.gameObject, 1f));
    }

    [ClientRpc]
    private void RpcDisableGameObjectAfterDelay(uint objectToDeactivateId)
    {
        if (!NetworkClient.spawned.TryGetValue(objectToDeactivateId, out var deactivateIdentity))
        {
            Debug.Log("Cannot find selected group by netId");
            return;
        }

        StartCoroutine(DisableGameObjectAfterDelay(deactivateIdentity.gameObject, 1f));
    }

    [Command]
    public void CmdUseBombTrunk(int index, string name, uint objectToDeactivateId)
    {
        if (!NetworkClient.spawned.TryGetValue(objectToDeactivateId, out var deactivateIdentity))
        {
            Debug.Log("Cannot find selected group by netId");
            return;
        }

        StartCoroutine(DisableGameObjectAfterDelay(deactivateIdentity.gameObject, 1f));
        _retreatScript.RetreatDiceWithBomb(index);
        _wagonInitializerScript.AddPointsForTrunk(name);
    }

    [Command]
    public void CmdIncreaseTrunks()
    {
        _pointCounterScript.UpdateTrunkAmount(+1);
    }

    [Command]
    public void CmdUpdateTakenTrunks(Transform transform, int i)
    {
        _trunkPlacerScript.UpdateTakenTrunks(transform, i);
    }

    [Command]
    public void CmdSetJokerToPosition(int newValue)
    {
        Debug.Log("Command cmdSetJokerTopoistion");
        if (_selectOutlineScript == null)
        {
            Debug.Log("_selectOutlineScirpt == null");
        }
        Debug.Log("Command _selectOutlineScript.JokerToPosition: " + _selectOutlineScript.JokerToPosition);
        Debug.Log("Command newValue: " + newValue);

        _selectOutlineScript.JokerToPosition = newValue;
    }

    public void CmdRemoveAllWagons(TestEndGame script)
    {
        script.ServerRemoveAllWagons();
    }
}
