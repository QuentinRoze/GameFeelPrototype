using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using static Unity.VisualScripting.Member;

public class PunchEmitter : MonoBehaviour
{
    [Header("Hard References")]
    [SerializeField] InputActionReference punchInputRef;
    [SerializeField] TargetsGathering targetsClose;
    [SerializeField] TargetsGathering targetsFar;
    [SerializeField] CinemachineImpulseSource strongPunchCameraShake;
    [SerializeField] CinemachineCamera cinemachineCam;
    [SerializeField] Renderer rightPunchRend;

    [Header("Soft References")]
    [SerializeField] Volume juicyPostProcessVolume;
    [SerializeField] float juicyPostProcessVolumeInLerpSpeed;
    [SerializeField] float juicyPostProcessVolumeOutLerpSpeed;

    [Header("Charging State Tweakables")]
    [SerializeField] float punchChargeTime;
    [SerializeField] float punchChargeMaxFOV;
    [SerializeField] float punchChargeFOVLerpSpeed;
    [SerializeField] float punchBackToIdleFOVLerpSpeed;

    [Header("Strong Punch State Tweakables")]
    [SerializeField] float strongPunchTimeBeforeIdle;
    [SerializeField] float strongPunchTimeBeforeHit;
    [SerializeField] Vector3 strongPunchDirection;
    [SerializeField] float strongPunchForce;
    [SerializeField] Vector3 strongPunchOffset;
    [Space]
    [SerializeField] float farTargetsForce;
    [SerializeField] AnimationCurve punchTimeScaleCurve;
    [SerializeField] AnimationCurve punchFOVAnimCurve;
    [SerializeField] float screenShakeDelay;

    [Header("Cancel State Tweakables")]
    [SerializeField] float cancelTimeBeforeIdle;

    [Space]
    [SerializeField] UnityEvent punchEvent;

    private float punchChargeTimer = 0;
    private float punchTimer = 0;
    private float cancelTimerBeforeIdle = 0;
    private bool punchPerformed = false;
    private float defaultFOV = 70;
    private Coroutine punchFeedbackCoroutine;


    public enum PunchState
    {
        Idle,
        Charge,
        Punch,
        Cancel
    }
    private PunchState myState = PunchState.Idle;

    private void Start()
    {
        defaultFOV = cinemachineCam.Lens.FieldOfView;
    }

    private void Update()
    {
        UpdateState();
    }

    //UPDATE STATES-----------------
    private void IdleUpdate()
    {
        //post process lerp to null
        if (juicyPostProcessVolume != null)
            juicyPostProcessVolume.weight = Mathf.Lerp(juicyPostProcessVolume.weight, 0.001f, juicyPostProcessVolumeOutLerpSpeed * Time.deltaTime);

        cinemachineCam.Lens.FieldOfView = Mathf.Lerp(cinemachineCam.Lens.FieldOfView, defaultFOV, punchBackToIdleFOVLerpSpeed);

        if (punchInputRef.action.IsPressed())
        {
            SwitchState(PunchState.Charge);
        }
    }
    private void ChargeUpdate()
    {
        //CHARGING
        if (punchInputRef.action.ReadValue<float>() >0f)
        {
            //post process lerp to 1
            if (juicyPostProcessVolume != null)
                juicyPostProcessVolume.weight = Mathf.Lerp(juicyPostProcessVolume.weight, 1f, juicyPostProcessVolumeInLerpSpeed * Time.deltaTime);

            cinemachineCam.Lens.FieldOfView = Mathf.Lerp(cinemachineCam.Lens.FieldOfView, punchChargeMaxFOV, punchChargeFOVLerpSpeed);

            punchChargeTimer += Time.deltaTime;
            rightPunchRend.material.SetFloat("_ChargeAmount", punchChargeTimer / punchChargeTime);
        }
        //BUTTON RELEASE WITH ENOUGH CHARGE -> PUNCH
        else if (punchChargeTimer > punchChargeTime)
        {
            SwitchState(PunchState.Punch);
        }
        //BUTTON RELEASE WITHOUT ENOUGH CHARGE -> BACK TO IDLE
        else
        {
            SwitchState(PunchState.Idle);
        }
    }
    private void PunchUpdate()
    {
        //post process lerp to null
        if(juicyPostProcessVolume != null)
            juicyPostProcessVolume.weight = Mathf.Lerp(juicyPostProcessVolume.weight, 0.001f, juicyPostProcessVolumeOutLerpSpeed * Time.deltaTime);

        punchTimer += Time.deltaTime;

        if (punchTimer > strongPunchTimeBeforeIdle)
        {
            SwitchState(PunchState.Idle);
        }
        else if(punchTimer> strongPunchTimeBeforeHit && !punchPerformed)
        {
            Punch();
        }
    }
    private void CancelUpdate()
    {
        cancelTimerBeforeIdle += Time.deltaTime;
        if(cancelTimerBeforeIdle > cancelTimeBeforeIdle)
        {
            SwitchState(PunchState.Idle);
        }
    }

    //STATE TRANSITIONS---------------
    private void ExitState(PunchState _toState)
    {
        switch (myState)
        {
            case PunchState.Idle:
                break;
            case PunchState.Charge:
                break;
            case PunchState.Punch:
                StopCoroutine(punchFeedbackCoroutine);
                break;
            case PunchState.Cancel:
                break;
        }
    }
    private void EnterState(PunchState _fromState)
    {
        switch (myState)
        {
            case PunchState.Idle:
                print("Enter State Idle");
                rightPunchRend.material.SetFloat("_ChargeAmount", 0f);
                break;
            case PunchState.Charge:
                punchChargeTimer = 0;
                print("Enter State Charge");
                break;
            case PunchState.Punch:
                punchFeedbackCoroutine = StartCoroutine(StrongPunchFeedbackCoroutine());
                punchTimer = 0;
                punchPerformed = false;
                break;
            case PunchState.Cancel:
                cancelTimeBeforeIdle = 0;
                rightPunchRend.material.SetFloat("_ChargeAmount", 0f);
                break;
        }
    }
    private void UpdateState()
    {
        switch (myState)
        {
            case PunchState.Idle:
                IdleUpdate();
                break;
            case PunchState.Charge:
                ChargeUpdate();
                break;
            case PunchState.Punch:
                PunchUpdate();
                break;
            case PunchState.Cancel:
                CancelUpdate();
                break;
        }
    }
    private void SwitchState(PunchState _newState)
    {
        PunchState _oldState = myState;
        ExitState(_newState);
        myState = _newState;
        EnterState(_oldState);
    }

    //OTHER FUNCTIONS-----------------
    private void Punch()
    {
        punchEvent.Invoke();

        rightPunchRend.material.SetFloat("_ChargeAmount", 0);
        punchPerformed = true;
        Vector3 _punchIntensityAndForce;
        Vector3 _punchPositionOffset;
        Vector3 punchDirection = strongPunchDirection.x * Camera.main.transform.right + strongPunchDirection.y * Camera.main.transform.up + strongPunchDirection.z * Camera.main.transform.forward;
        _punchIntensityAndForce = punchDirection.normalized * strongPunchForce;
        _punchPositionOffset = strongPunchOffset.x * Camera.main.transform.right + strongPunchOffset.y * Camera.main.transform.up + strongPunchOffset.z * Camera.main.transform.forward;

        //Apply Force + tell entities they have been punched
        for(int i=0; i<targetsClose.punchReceivers.Count; i++)
        {
            targetsClose.punchReceivers[i].myRb.AddForceAtPosition(_punchIntensityAndForce, targetsClose.punchReceivers[i].transform.position+targetsClose.punchReceivers[i].myRb.centerOfMass+ _punchPositionOffset);
            targetsClose.punchReceivers[i].OnPunchReceived();
        }

        //lighter push on far targets
        for (int i = 0; i < targetsFar.punchReceivers.Count; i++)
        {
            if (!targetsClose.punchReceivers.Contains(targetsFar.punchReceivers[i]))
            {
                targetsFar.punchReceivers[i].myRb.AddForce(farTargetsForce * (targetsFar.punchReceivers[i].transform.position - transform.position).normalized);
            }
        }
    }

    private IEnumerator StrongPunchFeedbackCoroutine()
    {
        float strongPunchTimer = 0;
        bool screenShakeDone = false;

        while (strongPunchTimer < 1)
        {
            strongPunchTimer += Time.deltaTime/strongPunchTimeBeforeIdle;
            //Time.timeScale = punchTimeScaleCurve.Evaluate(strongPunchTimer);
            if (!screenShakeDone && strongPunchTimer * strongPunchTimeBeforeIdle >= screenShakeDelay)
            {
                strongPunchCameraShake.GenerateImpulse();
                screenShakeDone = true;
            }

            cinemachineCam.Lens.FieldOfView = Mathf.Lerp(defaultFOV, punchChargeMaxFOV, punchFOVAnimCurve.Evaluate(strongPunchTimer));
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }
}
