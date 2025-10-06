using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class STWagonInitializer : TWagonInitializer
{
    [SerializeField] private Rigidbody _blueDieToMove;
    [SerializeField] private Rigidbody _redDieToMove;

    public void ShiftWagons()
    {
        Image firstImage = wagonObjects[0];
        Image secondImage = wagonObjects[1];
        RectTransform firstImageRect = firstImage.GetComponent<RectTransform>();
        RectTransform secondImageRect = secondImage.GetComponent<RectTransform>();

        firstImageRect.position = secondImageRect.position;
        Vector3 pos = firstImageRect.position;
        MoveDiceToNextTile(pos);
    }

    private void MoveDiceToNextTile(Vector3 pos)
    {
        pos.x -= 2.5f;
        pos.y = 1f;
        var startZ = pos.z;

        // BLUE
        pos.z = startZ - 1.5f;
        _blueDieToMove.MovePosition(pos);

        // RED
        pos.z = startZ - 0.5f;
        _redDieToMove.MovePosition(pos);
    }
}
