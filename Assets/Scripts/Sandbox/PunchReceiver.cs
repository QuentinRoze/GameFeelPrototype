using UnityEngine;

public class PunchReceiver : MonoBehaviour
{
    [HideInInspector] public Rigidbody myRb;
    [SerializeField] private GameObject punchReceivedPS;
    [SerializeField] private Animator punchReceivedAnim;
    [SerializeField] private bool shouldSpawnPS = false;
    [SerializeField] private bool shouldAnim = false;

    private void Start()
    {
        myRb = GetComponent<Rigidbody>();
    }

    public void OnPunchReceived()
    {
        if(shouldSpawnPS)
        {
            Instantiate(punchReceivedPS, transform.position + Vector3.up*1.5f, Quaternion.identity);
        }
        if (shouldAnim)
        {
            punchReceivedAnim.SetTrigger("PunchReceivedTrigger");
        }
    }
}
