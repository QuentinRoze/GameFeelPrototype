using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneReset : MonoBehaviour
{
    [SerializeField] InputActionReference resetSceneInputRef;
    [SerializeField] Transform[] objectsToReset;

    private List<Vector3> startPosObjectsToReset = new List<Vector3>();
    private List<Quaternion> startRotObjectsToReset = new List<Quaternion>();

    private void Start()
    {
        for (int i = 0; i < objectsToReset.Length; i++)
        {
            startPosObjectsToReset.Add(objectsToReset[i].position);
            startRotObjectsToReset.Add(objectsToReset[i].rotation);
        }
    }

    void Update()
    {
        if (resetSceneInputRef.action.WasPressedThisFrame())
        {
            if (objectsToReset.Length > 0)
            {
                for (int i = 0; i < objectsToReset.Length; i++)
                {
                    objectsToReset[i].position = startPosObjectsToReset[i];
                    objectsToReset[i].rotation = startRotObjectsToReset[i];
                    if (objectsToReset[i].TryGetComponent<Rigidbody>(out Rigidbody rb))
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }

                }
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}
