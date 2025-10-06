using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class TRotate : MonoBehaviour
{
    private Vector3 _neutralPosition = new Vector3(0, 10f, 0);
    private float _rotationStep = 90f;
    [SerializeField] private GameObject _front;
    [SerializeField] private GameObject _back;
    private Animator _trunkAnimator;

    private void Start()
    {
        _trunkAnimator = GetComponent<Animator>();
    }

    public void Rotate()
    {
        //StartCoroutine(RotateTrunk());
        transform.position = _neutralPosition;
        _trunkAnimator.SetTrigger("Rotate");
    }

    private IEnumerator RotateTrunk()
    {
        Debug.Log("RotateTrunk");
        transform.position = _neutralPosition;

        while (!(Quaternion.Angle(transform.rotation, Quaternion.Euler(0,0,180)) < 0.1f))
        {
            Debug.Log("ROTATING");
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, 
                Quaternion.Euler(0, 0, 180), 
                _rotationStep * Time.deltaTime // Ensure consistent rotation speed
            );

            yield return null;
        }

        //_isRotating = false;

        int index = int.Parse(Regex.Match(name, @"\d+$").Value) - 1;
        Debug.Log("index: " + index);
    }

    public void HalfRotation()
    {
        _back.SetActive(false);
        _front.SetActive(true);
    }
}
