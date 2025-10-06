using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TWagonInitializer : MonoBehaviour
{
    public Image[] wagonObjects;
    public Sprite[] wagonSprites;

    [SerializeField] public TrunkSides[] _trunkSides;
    protected int[] spriteIndices = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

    [SerializeField] protected Image[] trunkBackImages;
    [SerializeField] protected Image[] trunkFrontImages;
    protected int[] trunkSideSpriteIndices = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };

    [SerializeField] protected Material greenMaterial;
    [SerializeField] protected Material redMaterial;

    void Start()
    {
        AssignWagons();
        AssignTrunks();
    }

    protected void AssignWagons()
    {
        for (int i = 0; i < wagonObjects.Length; i++)
        {
            wagonObjects[i].sprite = wagonSprites[spriteIndices[i]];

            //AssignImageProperties(i, wagonObjects[i].sprite.name);
        }

        //_wagons[0].Last = true;
        //_wagons[16].AssignedNumber = 5;
        //_wagons[16].Number = 16;
    }

    protected void AssignTrunks()
    {
        for (int i = 0; i < trunkBackImages.Length; i++)
        {
            trunkBackImages[i].sprite = _trunkSides[trunkSideSpriteIndices[i]].Back;
            trunkFrontImages[i].sprite = _trunkSides[trunkSideSpriteIndices[i]].Front;

            var nameBack = _trunkSides[trunkSideSpriteIndices[i]].Back.name;
            trunkBackImages[i].name = nameBack.Substring(nameBack.IndexOf("_") + 1);

            var nameFront = _trunkSides[trunkSideSpriteIndices[i]].Front.name;
            trunkFrontImages[i].name = nameFront;
        }
    }

    public void SetImageMaterial(int index, MaterialColor materialColor)
    {
        if (materialColor == MaterialColor.GREEN)
            wagonObjects[index].material = greenMaterial;
        else if (materialColor == MaterialColor.RED)
            wagonObjects[index].material = redMaterial;
        else
            wagonObjects[index].material = null;
    }
}
