using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class TargetsGathering : MonoBehaviour
{
    [HideInInspector] public List<PunchReceiver> punchReceivers = new List<PunchReceiver>();

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<PunchReceiver>(out PunchReceiver _punchReceiver))
        {
            punchReceivers.Add(_punchReceiver);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PunchReceiver>(out PunchReceiver _punchReceiver))
        {
            punchReceivers.Remove(_punchReceiver);
        }
    }
}
