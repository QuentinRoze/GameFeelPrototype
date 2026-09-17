using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] InputActionReference camMovementInputRef;
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform cameraHolderTransform;

    [Header("Tweakables")]
    [SerializeField] float rotateSpeed;
    [SerializeField] float rotationLerpSpeed;
    [SerializeField] float pitchClamp;

    private float rotX = 0;
    private Quaternion characterTargetRot;

    private void Start()
    {
        characterTargetRot = transform.rotation;
    }
    private void Update()
    {
        Vector2 rotateInput = camMovementInputRef.action.ReadValue<Vector2>();
        characterTargetRot *= Quaternion.Euler(0, rotateInput.x * rotateSpeed * Time.deltaTime, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, characterTargetRot, rotationLerpSpeed * Time.deltaTime);

        //Saving Rotation X like this otherwise clamp doesn't work
        rotX += -rotateInput.y * rotateSpeed * Time.deltaTime;
        rotX = Mathf.Clamp(rotX, -pitchClamp, pitchClamp);
        Vector3 aimedRotation = new Vector3(rotX, transform.eulerAngles.y, transform.eulerAngles.z);
        Quaternion newRot = Quaternion.Euler(aimedRotation);
        cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, newRot, rotationLerpSpeed*Time.deltaTime);
        cameraTransform.rotation *= cameraHolderTransform.localRotation;
    }
}
