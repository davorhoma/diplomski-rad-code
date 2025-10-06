using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewTrunkValues : MonoBehaviour
{
    [SerializeField] public Image Back;
    [SerializeField] public Image Front;
    [SerializeField] private Rotate _rotateScript;

    private void Start()
    {
        _rotateScript.FindScripts();
        StartCoroutine(_rotateScript.RotateTrunk());
    }

    public void AssignImages(string trunkName, string backSpriteName, string frontSpriteName)
    {
        name = trunkName;

        Debug.Log("BackSpriteName: " + backSpriteName);
        Debug.Log("FrontSpriteName: " + frontSpriteName);
        Sprite backSprite = Resources.Load<Sprite>("Sprites/Back_" + backSpriteName);
        Sprite frontSprite = Resources.Load<Sprite>("Sprites/" + frontSpriteName);

        if (backSprite != null && frontSprite != null)
        {
            Back.sprite = backSprite;
            Front.sprite = frontSprite;
        }
        else
        {
            Debug.LogError("Sprite not found: " + name);
        }
    }
}
