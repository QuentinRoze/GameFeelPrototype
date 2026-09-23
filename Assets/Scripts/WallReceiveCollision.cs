using UnityEngine;

public class WallReceiveCollision : MonoBehaviour
{
    [SerializeField] private float minForceToParticle;
    [SerializeField] private GameObject particleToSpawn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.GetComponent<Rigidbody>() != null && collision.relativeVelocity.magnitude > minForceToParticle)
        {
            Instantiate(particleToSpawn, collision.transform.position, Quaternion.LookRotation(-transform.forward, Vector3.up));
        }
    }
}
