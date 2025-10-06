using Mirror;
using Mirror.BouncyCastle.Security;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class GameDie : NetworkBehaviour
{
    [SyncVar] private DieValue _value;
    public DieValue Value => _value;
    [SerializeField, FormerlySerializedAs("_diceSides")] private DieSide[] _dieSides;
    [SerializeField] private bool _hasLanded;
    [SerializeField] private bool _isThrown;
    [SerializeField] private Vector3 _initPos;
    [SerializeField] private Rigidbody _rb;
    private float _rollForce = 15f;
    private float _torqueAmount = 100f;
    private float stopThreshold = 0.00001f;
    private bool sleeping;

    //[SerializeField] private AudioSource _audioSource;
    //[SerializeField] private AudioClip _audioClip;
    //[SerializeField] private AudioClip _twoDiceAudio;

    private SoundManager _soundManager;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _soundManager = FindObjectOfType<SoundManager>();
    }
    void Start()
    {
        //_rb.useGravity = true;
        _initPos = transform.position;

        //_audioSource = GameObject.Find("Sound Effects").GetComponent<AudioSource>();
    }

    private IEnumerator CheckIfSleeping(Rigidbody rigidbody, bool sleeping)
    {
        //yield return new WaitForSeconds(1f);
        //var timeOut = Time.time + 10f;
        while (!_rb.IsSleeping())
        {
            //print("not sleeping");
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        //sleeping = rigidbody.velocity.sqrMagnitude < stopThreshold && rigidbody.angularVelocity.sqrMagnitude < stopThreshold;
        //if (_rb.velocity.sqrMagnitude < stopThreshold && _rb.angularVelocity.sqrMagnitude < stopThreshold && !_hasLanded && _isThrown)
        //{
        //    _hasLanded = true;
        //    GetDieValue(_rb);
        //}

        //else if (_rb.velocity.sqrMagnitude < stopThreshold && _rb.angularVelocity.sqrMagnitude < stopThreshold && _hasLanded && _value == 0)
        //{
        //    RollAgain();
        //}
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

    [Server]
    public void Roll()
    {
        ServerRollDice();
    }

    [Server]
    public void ServerRollDice()
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

            _rb.AddTorque(x, y, z, ForceMode.Acceleration);
            //_rb.AddTorque(300, 300, 200, ForceMode.Acceleration); // Roll to get 4 jokers

            StartCoroutine(CheckIfSleeping(_rb, sleeping));
        }
        else if (_isThrown && _hasLanded)
        {
            ResetDie();
        }
    }

    [Server]
    private void ResetDie()
    {
        _rb.useGravity = false;
        transform.position = _initPos;
        _isThrown = false;
        _hasLanded = false;
        _rb.useGravity = true;

        RpcSyncReset(_initPos);
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
                _value = (DieValue)side.sideValue;
                //Debug.Log(i + ": " + side.sideValue);

                //print(_value);
                //switch (_value)
                //{
                //    case 1:
                //        Debug.Log("Joker has been rolled");
                //        break;
                //    case 2:
                //        Debug.Log("Hat has been rolled");
                //        break;
                //    case 3:
                //        Debug.Log("Mask has been rolled");
                //        break;
                //    case 4:
                //        Debug.Log("Heel has been rolled");
                //        break;
                //    case 5:
                //        Debug.Log("Gun has been rolled");
                //        break;
                //    case 6:
                //        Debug.Log("Horseshoe has been rolled");
                //        break;
                //}
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

    [ClientRpc]
    private void RpcSyncReset(Vector3 position)
    {
        _rb.MovePosition(position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        float collisionPower = collision.relativeVelocity.magnitude;
        float min = 0.00001f;
        float max = 0.5f;

        //Debug.Log("Collision Power: " + collisionPower);

        if (collision.collider.CompareTag("RollingPad") || collision.collider.CompareTag("GameBoard"))
        {
            //Debug.Log("MaxLinearVelocity: " + _rb.maxLinearVelocity);
            _soundManager.PlaySoundHitBoard(Mathf.Clamp(collisionPower, min, max));
            //_audioSource.PlayOneShot(_audioClip, Mathf.Clamp(collisionPower, min, max));
        }

        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("DiceLayer"))
        {
            _soundManager.PlaySoundHitDice(Mathf.Clamp(collisionPower, min, max));
            //_audioSource.PlayOneShot(_twoDiceAudio, Mathf.Clamp(collisionPower, min, max));
        }
    }
}
