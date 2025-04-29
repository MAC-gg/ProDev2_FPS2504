using UnityEngine;
using System.Collections;

public class TrapSet : MonoBehaviour
{
    private enum trapType { noreset, auto, manual }
    public enum trapState { inactive, reset, active, tripped };

    [SerializeField] Renderer model;
    [SerializeField] trapType type;
    [SerializeField] float speedMult;
    [SerializeField] int trapDurationSeconds = 5;
    [SerializeField] int trapResetSeconds = 2;

    // start inactive and reset
    // gives player time to leave radius
    public trapState curState = trapState.inactive;

    private bool inTrigger;
    WaitForSeconds resetWait;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resetWait = new WaitForSeconds(trapResetSeconds);
        
        OnStateChange();
        
        // reset
        StartCoroutine(resetTrap());
    }

    // Update is called once per frame
    void Update()
    {
        if(inTrigger && curState == trapState.inactive)
        {
            // TODO: show interact prompt
            Debug.Log("Interact");

            // MANUAL RESET
            if (Input.GetButtonDown("Interact"))
            {
                Debug.Log("interacted");
                // reset
                StartCoroutine(resetTrap());
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        inTrigger = true;

        ITrap itrap = other.GetComponent<ITrap>();

        if (itrap != null && curState == trapState.active)
        {
            curState = trapState.tripped;
            OnStateChange();
            StartCoroutine(itrap.trap(speedMult, trapDurationSeconds));
        }

        if (type != trapType.noreset)
        {
            curState = trapState.inactive;
            OnStateChange();
            if (type == trapType.auto)
            {
                // start reset cooroutine
                StartCoroutine(resetTrap());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        inTrigger = false;
    }

    IEnumerator resetTrap()
    {
        curState = trapState.reset; // reset
        OnStateChange();
        yield return resetWait;
        curState = trapState.active; // active
        OnStateChange();
    }

    public void OnStateChange()
    {
        Debug.Log("state change");
        if (model != null)
        {
            if (curState is trapState.active)
            {
                model.material.color = Color.red;
            }
            else if (curState is trapState.inactive)
            {
                model.material.color = Color.black;
            }
            else if (curState is trapState.reset)
            {
                Debug.Log("gray");
                model.material.color = Color.gray;
            }
            else if (curState is trapState.tripped)
            {
                Debug.Log("yellow");
                model.material.color = Color.yellow;
            }
        }
    }
}