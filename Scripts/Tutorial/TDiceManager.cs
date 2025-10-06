using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class TDiceManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _blueDice;
    [SerializeField] private TGameDie[] _blueDiceRollers;
    private List<TGameDie> gameDice;
    private int _rollForce;
    private int _torqueAmount;
    [SerializeField] float x = -4.401576f;

    [SerializeField] private TDicePositioner _dicePositioner;
    [SerializeField] private SoundManager _soundManager;
    private TTransparencyManager _transparencyManager;
    [SerializeField] private Button _rollButton;

    private string _blueSelectable = "BlueSelectable";

    private void Start()
    {
        _transparencyManager = GetComponent<TTransparencyManager>();
    }

    public void RollDice()
    {
        _rollButton.interactable = false;
        _transparencyManager.NextStep();
        StartCoroutine(ApplyForceToAll());
    }

    public IEnumerator ApplyForceToAll()
    {
        yield return StartCoroutine(Wait());
        var allRigidbodies = GetRigidbodiesFromTransforms(_blueDice);

        _soundManager.PlaySoundGrabDice();
        _dicePositioner.PositionDiceForRoll(allRigidbodies);

        yield return StartCoroutine(AddForceAndTorque());
    }

    private List<Rigidbody> GetRigidbodiesFromTransforms(GameObject[] list)
    {
        var rigidbodies = new List<Rigidbody>();
        foreach (var child in list)
        {
            var childRb = child.GetComponent<Rigidbody>();
            rigidbodies.Add(childRb);
        }

        return rigidbodies;
    }

    IEnumerator AddForceAndTorque()
    {
        //Debug.Log("AddForceAndTorque");
        var timeOut = Time.time + 10f;

        // Wait for all dice to stop rolling
        while (Time.time < timeOut && !AreAllDiceSleeping())
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        Debug.Log("Passed");

        foreach (var gameDie in _blueDiceRollers)
        {
            gameDie.enabled = true;
            gameDie.Roll();
        }

        // Wait for dice to stop
        gameDice = new List<TGameDie>(_blueDiceRollers);
        while (gameDice.Any(r => r.Value == 0))
        {
            yield return null;
        }

        Debug.Log("Rolled dice");

        StartCoroutine(ReadAndProcessDiceValues());
    }

    private bool AreAllDiceSleeping()
    {
        foreach (var gameDie in _blueDiceRollers)
        {
            if (!gameDie.IsDieSleeping())
            {
                return false;
            }
        }

        return true;
    }

    IEnumerator ReadAndProcessDiceValues()
    {
        Debug.Log("all dice have value");
        // Ensure a slight delay after all dice have stopped to stabilize their positions
        yield return new WaitForSeconds(0.1f);

        List<int> dieValues = new List<int>();

        foreach (var gameDie in gameDice)
        {
            if (gameDie.Value > 0)
            {
                Debug.Log("Dice value:" + gameDie.Value);
                dieValues.Add(gameDie.Value);
                gameDie.enabled = false;
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        gameDice.Clear();

        ProcessDieValues(dieValues);
    }

    void ProcessDieValues(List<int> dieValues)
    {
        int jokers = 0, hats = 0, masks = 0, heels = 0, guns = 0, horseshoes = 0;

        List<Rigidbody> maskRigidbodies = new List<Rigidbody>();
        List<Rigidbody> hatRigidbodies = new List<Rigidbody>();
        List<Rigidbody> heelRigidbodies = new List<Rigidbody>();
        List<Rigidbody> gunRigidbodies = new List<Rigidbody>();
        List<Rigidbody> horseshoeRigidbodies = new List<Rigidbody>();
        List<Rigidbody> jokerRigidbodies = new List<Rigidbody>();
        for (int i = 0; i < dieValues.Count; i++)
        {
            switch (dieValues[i])
            {
                case 1:
                    jokerRigidbodies.Add(_blueDice[i].GetComponent<Rigidbody>());
                    jokers++;
                    break;
                case 2:
                    hatRigidbodies.Add(_blueDice[i].GetComponent<Rigidbody>());
                    hats++;
                    break;
                case 3:
                    maskRigidbodies.Add(_blueDice[i].GetComponent<Rigidbody>());
                    masks++;
                    break;
                case 4:
                    heelRigidbodies.Add(_blueDice[i].GetComponent<Rigidbody>());
                    heels++;
                    break;
                case 5:
                    gunRigidbodies.Add(_blueDice[i].GetComponent<Rigidbody>());
                    guns++;
                    break;
                case 6:
                    horseshoeRigidbodies.Add(_blueDice[i].GetComponent<Rigidbody>());
                    horseshoes++;
                    break;
            }

            _blueDice[i].transform.tag = _blueSelectable;
        }

        //_clickTileScript.AppendDice = false;

        string color = "Blue";
        ProcessDiceGroup($"{color}HatGroup0", hatRigidbodies, hats, 0);
        ProcessDiceGroup($"{color}MaskGroup0", maskRigidbodies, masks, 1);
        ProcessDiceGroup($"{color}HeelGroup0", heelRigidbodies, heels, 2);
        ProcessDiceGroup($"{color}HorseshoeGroup0", horseshoeRigidbodies, horseshoes, 3);
        ProcessDiceGroup($"{color}GunsGroup0", gunRigidbodies, guns, 4);
        ProcessDiceGroup($"{color}JokerGroup0", jokerRigidbodies, jokers, 5);
        Debug.Log("Processed all dice groups");

        x = -4.401576f;

        //_buttonEndTurn.interactable = true;
        _transparencyManager.PlaceDice();
    }

    private void ProcessDiceGroup(string groupName, List<Rigidbody> rigidbodies, int count, int groupIndex)
    {
        if (count > 0)
        {
            GameObject groupObject = CreateGroup(groupName, rigidbodies.Count, groupIndex);

            GroupObjects(groupObject, rigidbodies);

            ResetDice(rigidbodies, x, groupObject);
            x += count + 1f;
        }
    }

    private GameObject CreateGroup(string groupName, int count, int groupIndex)
    {
        GameObject groupObject = new GameObject(groupName);
        BoxCollider boxCollider = groupObject.AddComponent<BoxCollider>();
        Rigidbody rb = groupObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.mass = count * 1f;

        groupObject.tag = _blueSelectable;

        int layerIndex = LayerMask.NameToLayer("DiceGroupLayer");
        groupObject.layer = layerIndex;
        boxCollider.isTrigger = true;

        return groupObject;
    }

    void GroupObjects(GameObject groupObject, List<Rigidbody> rigidbodies)
    {
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.transform.SetParent(groupObject.transform, true);
        }
    }

    void ResetDice(List<Rigidbody> rigidbodies, float x, GameObject groupObject, float z = -10f)
    {
        float a = 0f;
        foreach (Rigidbody rb in rigidbodies)
        {
            Vector3 existingRotation = rb.transform.rotation.eulerAngles;
            existingRotation.y = 0f;
            rb.transform.rotation = Quaternion.Euler(existingRotation);
            rb.useGravity = true;

            rb.transform.position = new Vector3(x + a, 1.5f, z);
            a += 1f;
        }
    }

    private IEnumerator Wait(float time = 0.5f)
    {
        yield return new WaitForSeconds(time);
    }
}
