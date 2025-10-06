using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TGameDie : MonoBehaviour
{
    private int _value;
    public int Value => _value;
    [SerializeField, FormerlySerializedAs("_diceSides")] private DieSide[] _dieSides;
    [SerializeField] private bool _hasLanded;
    [SerializeField] private bool _isThrown;
    [SerializeField] private Vector3 _initPos;
    [SerializeField] private Rigidbody _rb;
    private bool sleeping;
    [SerializeField] private int torqueX;
    [SerializeField] private int torqueY;
    [SerializeField] private int torqueZ;

    private SoundManager _soundManager;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        _initPos = transform.position;
        _soundManager = FindObjectOfType<SoundManager>();
    }

    public void Roll()
    {
        if (_rb == null) return;
        _isThrown = false;
        _hasLanded = false;
        _value = 0;
        if (!_isThrown && !_hasLanded)
        {
            _isThrown = true;
            _rb.useGravity = true;
            _hasLanded = false;
            var minValue = 100f;
            var maxValue = 500f;

            float x = Random.Range(minValue, maxValue);
            float y = Random.Range(minValue, maxValue);
            float z = Random.Range(minValue, maxValue);

            Debug.Log("ADDING TORQUE");
            //_rb.AddTorque(x, y, z, ForceMode.Acceleration);
            _rb.AddTorque(torqueX, torqueY, torqueZ, ForceMode.Acceleration);

            StartCoroutine(CheckIfSleeping(_rb, sleeping));
        }
        else if (_isThrown && _hasLanded)
        {
            ResetDie();
        }
    }

    private void ResetDie()
    {
        _rb.useGravity = false;
        transform.position = _initPos;
        _isThrown = false;
        _hasLanded = false;
        _rb.useGravity = true;
    }

    private IEnumerator CheckIfSleeping(Rigidbody rigidbody, bool sleeping)
    {
        while (!_rb.IsSleeping())
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        if (_rb.IsSleeping() && !_hasLanded && _isThrown)
        {
            _hasLanded = true;
            GetDieValue(_rb);
        }
        if (_rb.IsSleeping() && _hasLanded && _value == 0)
        {
            RollAgain();
        }
    }

    private void RollAgain()
    {
        Debug.Log("ROLLING AGAIN");
        ResetDie();
        Roll();
    }

    public void GetDieValue(Rigidbody dice)
    {
        _value = 0;
        int i = 0;
        foreach (DieSide side in _dieSides)
        {
            if (side.OnGround)
            {
                i++;
                _value = side.sideValue;
            }
        }
    }

    public bool IsDieSleeping()
    {
        return _rb.IsSleeping();
    }

    public bool HasDieValue()
    {
        return (Value != 0) ? true : false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //float collisionPower = collision.relativeVelocity.magnitude;
        //float min = 0.00001f;
        //float max = 0.5f;

        //if (collision.collider.CompareTag("RollingPad") || collision.collider.CompareTag("GameBoard"))
        //{
        //    _soundManager.PlaySoundHitBoard(Mathf.Clamp(collisionPower, min, max));
        //}

        //if (collision.collider.gameObject.layer == LayerMask.NameToLayer("DiceLayer"))
        //{
        //    _soundManager.PlaySoundHitDice(Mathf.Clamp(collisionPower, min, max));
        //}
    }
}
