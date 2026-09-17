using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneReset : MonoBehaviour
{
    [SerializeField] InputActionReference resetSceneInputRef;

    void Update()
    {
        if (resetSceneInputRef.action.WasPressedThisFrame())
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
