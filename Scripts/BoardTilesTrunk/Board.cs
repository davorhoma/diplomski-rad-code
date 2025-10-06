using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GroupPos {
    public bool Has;
    public int TileNumber;

    public GroupPos() { }

    public GroupPos(bool h, int num)
    {
        Has = h;
        TileNumber = num;
        Debug.Log("num: " + num);
        Debug.Log("has: " + h);
    }
};

public class Board : NetworkBehaviour
{
    // IMPORTANT: I am not sure if SyncVar is correctly used here because it might not work with class)
    [SyncVar(hook = nameof(BlueHatsChanged))] public GroupPos hasBlueHats;
    [SyncVar] public GroupPos hasBlueMasks;
    [SyncVar] public GroupPos hasBlueHeels;
    [SyncVar] public GroupPos hasBlueHorseshoes;
    [SyncVar] public GroupPos hasBlueGuns;
    [SyncVar] public GroupPos hasBlueJokers;
    [SyncVar] public int Blue;
    public int blueAvailableTile;

    [SyncVar(hook = nameof(RedHatsChanged))] public GroupPos hasRedHats;
    [SyncVar] public GroupPos hasRedMasks;
    [SyncVar] public GroupPos hasRedHeels;
    [SyncVar] public GroupPos hasRedHorseshoes;
    [SyncVar] public GroupPos hasRedGuns;
    [SyncVar] public GroupPos hasRedJokers;
    [SyncVar] public int Red;
    public int redAvailableTile;

    [SyncVar] public GroupPos hasGreenHats;
    [SyncVar] public GroupPos hasGreenMasks;
    [SyncVar] public GroupPos hasGreenHeels;
    [SyncVar] public GroupPos hasGreenHorseshoes;
    [SyncVar] public GroupPos hasGreenGuns;
    [SyncVar] public GroupPos hasGreenJokers;
    [SyncVar] public int Green;
    public int greenAvailableTile;

    [SyncVar] public GroupPos hasYellowHats;
    [SyncVar] public GroupPos hasYellowMasks;
    [SyncVar] public GroupPos hasYellowHeels;
    [SyncVar] public GroupPos hasYellowHorseshoes;
    [SyncVar] public GroupPos hasYellowGuns;
    [SyncVar] public GroupPos hasYellowJokers;
    [SyncVar] public int Yellow;
    public int yellowAvailableTile;

    private void BlueHatsChanged(GroupPos oldValue, GroupPos newValue)
    {
        Debug.Log("BLUEHATSCHANGED");
    }

    private void RedHatsChanged(GroupPos oldValue, GroupPos newValue)
    {
        Debug.Log("REDHATSCHANGED");
        Debug.Log("RedHats: " + newValue.Has + ", " + newValue.TileNumber);
    }

    public void SetBlueGroup(string name, bool has, int num)
    {
        if (name.Contains("Hat"))
        {
            if (hasBlueHats.Has)
            {
                hasBlueHats.TileNumber = num;
            }
            else
            {
                hasBlueHats = new GroupPos(has, num);
            }
            Debug.Log("Hats: " + hasBlueHats.TileNumber);
        }
        else if (name.Contains("Mask"))
        {
            if (hasBlueMasks.Has)
            {
                hasBlueMasks.TileNumber = num;
            }
            else
            {
                hasBlueMasks = new GroupPos(has, num);
            }
            Debug.Log("Masks: " + hasBlueMasks.TileNumber);
        }
        else if (name.Contains("Heel"))
        {
            if (hasBlueHeels.Has)
            {
                hasBlueHeels.TileNumber = num;
            }
            else
            {
                hasBlueHeels = new GroupPos(has, num);
            }
            Debug.Log("Heels: " + hasBlueHeels.TileNumber);
        }
        else if (name.Contains("Horseshoe"))
        {
            if (hasBlueHorseshoes.Has)
            {
                hasBlueHorseshoes.TileNumber = num;
            }
            else
            {
                hasBlueHorseshoes = new GroupPos(has, num);
            }
            Debug.Log("Horseshoes: " + hasBlueHorseshoes.TileNumber);
        }
        else if (name.Contains("Gun"))
        {
            if (hasBlueGuns.Has)
            {
                hasBlueGuns.TileNumber = num;
            }
            else
            {
                hasBlueGuns = new GroupPos(has, num);
            }
            Debug.Log("Guns: " + hasBlueGuns.TileNumber);
        }
        else if (name.Contains("Joker"))
        {
            if (hasBlueJokers.Has)
            {
                hasBlueJokers.TileNumber = num;
            }
            else
            {
                hasBlueJokers.Has = true;
                hasBlueJokers.TileNumber = num;
            }
            Debug.Log("Jokers: " + hasBlueJokers.TileNumber);
        }
        //Debug.Log("Hats: " + hasBlueHats.Number);
        //Debug.Log("Masks: " + hasBlueMasks.Number);
        //Debug.Log("Heels: " + hasBlueHeels.Number);
        //Debug.Log("Horseshoes: " + hasBlueHorseshoes.Number);
        //Debug.Log("Guns: " + hasBlueGuns.Number);
        //Debug.Log("Jokers: " + hasBlueJokers.Number);
    }

    public void SetRedGroup(string name, bool has, int num)
    {
        if (name.Contains("Hat"))
        {
            if (hasRedHats.Has)
            {
                hasRedHats = new GroupPos(hasRedHats.Has, num);
                //hasRedHats.TileNumber = num;
            }
            else
            {
                hasRedHats = new GroupPos(has, num);
            }
        }
        else if (name.Contains("Mask"))
        {
            if (hasRedMasks.Has)
            {
                //hasRedMasks.TileNumber = num;
                hasRedMasks = new GroupPos(hasRedMasks.Has, num);
            }
            else
            {
                hasRedMasks = new GroupPos(has, num);
            }
        }
        else if (name.Contains("Heel"))
        {
            if (hasRedHeels.Has)
            {
                //hasRedHeels.TileNumber = num;
                hasRedHeels = new GroupPos(hasRedHeels.Has, num);
            }
            else
            {
                hasRedHeels = new GroupPos(has, num);
            }
        }
        else if (name.Contains("Horseshoe"))
        {
            if (hasRedHorseshoes.Has)
            {
                //hasRedHorseshoes.TileNumber = num;
                hasRedHorseshoes = new GroupPos(hasRedHorseshoes.Has, num);
            }
            else
            {
                hasRedHorseshoes = new GroupPos(has, num);
            }
        }
        else if (name.Contains("Gun"))
        {
            if (hasRedGuns.Has)
            {
                //hasRedGuns.TileNumber = num;
                hasRedGuns = new GroupPos(hasRedGuns.Has, num);
            }
            else
            {
                hasRedGuns = new GroupPos(has, num);
            }
        }
        else if (name.Contains("Joker"))
        {
            if (hasRedJokers.Has)
            {
                //hasRedJokers.TileNumber = num;
                hasRedJokers = new GroupPos(hasRedJokers.Has, num);
            }
            else
            {
                hasRedJokers = new GroupPos(has, num);
            }
        }
    }

    public void SetGreenGroup(string name, bool has, int num)
    {
        if (name.Contains("Hat"))
        {
            if (hasGreenHats.Has)
            {
                hasGreenHats.TileNumber = num;
            }
            else
            {
                hasGreenHats = new GroupPos(has, num);
            }
        }
        else if (name.Contains("Mask"))
        {
            if (hasGreenMasks.Has)
            {
                hasGreenMasks.TileNumber = num;
            }
            else
            {
                hasGreenMasks = new GroupPos(has, num);
            }
        }
        else if (name.Contains("Heel"))
        {
            if (hasGreenHeels.Has)
            {
                hasGreenHeels.TileNumber = num;
            }
            else
            {
                hasGreenHeels = new GroupPos(has, num);
            } 
        }
        else if (name.Contains("Horseshoe"))
        {
            if (hasGreenHorseshoes.Has)
            {
                hasGreenHorseshoes.TileNumber = num;
            }
            else
            {
                hasGreenHorseshoes = new GroupPos(has, num);
            }
        }
        else if (name.Contains("Gun"))
        {
            if (hasGreenGuns.Has)
            {
                hasGreenGuns.TileNumber = num;
            }
            else
            {
                hasGreenGuns = new GroupPos(has, num);
            }
        }
        else if (name.Contains("Joker"))
        {
            if (hasGreenJokers.Has)
            {
                hasGreenJokers.TileNumber = num;
            }
            else
            {
                hasGreenJokers = new GroupPos(has, num);
            }
        }
    }

    public void SetYellowGroup(string name, bool has, int num)
    {
        if (name.Contains("Hat"))
        {
            if (hasYellowHats.Has)
            {
                hasYellowHats.TileNumber = num;
            }
            else
            {
                hasYellowHats = new GroupPos(has, num);
            }
        }
        else if (name.Contains("Mask"))
        {
            if (hasYellowMasks.Has)
            {
                hasYellowMasks.TileNumber = num;
            }
            else
            {
                hasYellowMasks = new GroupPos(has, num);
            }
        }
        else if (name.Contains("Heel"))
        {
            if (hasYellowHeels.Has)
            {
                hasYellowHeels.TileNumber = num;
            }
            else
            {
                hasYellowHeels = new GroupPos(has, num);
            }
        }
        else if (name.Contains("Horseshoe"))
        {
            if (hasYellowHorseshoes.Has)
            {
                hasYellowHorseshoes.TileNumber = num;
            }
            else
            {
                hasYellowHorseshoes = new GroupPos(has, num);
            }
        }
        else if (name.Contains("Gun"))
        {
            if (hasYellowGuns.Has)
            {
                hasYellowGuns.TileNumber = num;
            }
            else
            {
                hasYellowGuns = new GroupPos(has, num);
            }
        }
        else if (name.Contains("Joker"))
        {
            if (hasYellowJokers.Has)
            {
                hasYellowJokers.TileNumber = num;
            }
            else
            {
                hasYellowJokers = new GroupPos(has, num);
            }
        }
    }
}
