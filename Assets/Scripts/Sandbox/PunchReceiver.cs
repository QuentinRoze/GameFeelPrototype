using UnityEngine;

public class PunchReceiver : MonoBehaviour
{
    [HideInInspector] public Rigidbody myRb;

    private void Start()
    {
        myRb = GetComponent<Rigidbody>();
    }

    public void OnPunchReceived()
    {

    }
}
