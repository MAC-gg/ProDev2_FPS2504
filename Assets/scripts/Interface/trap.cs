using UnityEngine;
using System.Collections;

public class TrapSet : MonoBehaviour
{
    private enum trapType { noreset, auto, manual }
    private enum trapState { inactive, reset, active };

    [SerializeField] trapType type;
    [SerializeField] float speedMult;
    [SerializeField] int trapDurationSeconds = 5;
    [SerializeField] int trapResetSeconds = 2;

    trapState state = trapState.active;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        Debug.Log("Trap Ontriggerenter");

        ITrap itrap = other.GetComponent<ITrap>();

        if (itrap != null && state == trapState.active)
        {
            StartCoroutine(itrap.trap(speedMult, trapDurationSeconds));
            Debug.Log("return from interface");
        }

        if (type != trapType.noreset)
        {
            state++; // inactive
            if (type == trapType.auto)
            {
                // start reset cooroutine
                // StartCoroutine(resetTrap());
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger)
            return;

        ITrap itrap = other.GetComponent<ITrap>();

        if (itrap != null)
        {
            // StartCoroutine(itrap.trap(speedMult, trapDurationSeconds));
        }
    }
}