using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using static Unity.VisualScripting.Member;

public class PunchEmitter : MonoBehaviour
{
    [Header("Hard References")]
    [SerializeField] InputActionReference punchInputRef;
    [SerializeField] TargetsGathering targetsClose;
    [SerializeField] TargetsGathering targetsFar;
    [SerializeField] CinemachineImpulseSource weakPunchCameraShake;
    [SerializeField] CinemachineImpulseSource strongPunchCameraShake;

    [Header("Soft References")]
    [SerializeField] Volume juicyPostProcessVolume;
    [SerializeField] float juicyPostProcessVolumeInLerpSpeed;
    [SerializeField] float juicyPostProcessVolumeOutLerpSpeed;

    [Header("Charging State Tweakables")]
    [SerializeField] float weakPunchChargeTime;
    [SerializeField] float strongPunchChargeTime;

    [Header("Weak Punch State Tweakables")]
    [SerializeField] float weakPunchTimeBeforeIdle;
    [SerializeField] float weakPunchTimeBeforeHit;
    [SerializeField] Vector3 weakPunchDirection;
    [SerializeField] float weakPunchForce;
    [SerializeField] Vector3 weakPunchOffset;

    [Header("Strong Punch State Tweakables")]
    [SerializeField] float strongPunchTimeBeforeIdle;
    [SerializeField] float strongPunchTimeBeforeHit;
    [SerializeField] Vector3 strongPunchDirection;
    [SerializeField] float strongPunchForce;
    [SerializeField] Vector3 strongPunchOffset;
    [Space]
    [SerializeField] float farTargetsForce;

    [Header("Cancel State Tweakables")]
    [SerializeField] float cancelTimeBeforeIdle;

    private float punchChargeTimer = 0;
    private float punchTimer = 0;
    private float cancelTimerBeforeIdle = 0;
    private bool punchPerformed = false;


    public enum PunchIntensity
    {
        None,
        Weak,
        Strong
    }
    public enum PunchState
    {
        Idle,
        Charge,
        Punch,
        Cancel
    }
    private PunchState myState = PunchState.Idle;
    private PunchIntensity currentPunchIntensity = PunchIntensity.None;

    private void Update()
    {
        UpdateState();
    }

    //UPDATE STATES-----------------
    private void IdleUpdate()
    {
        juicyPostProcessVolume.weight = Mathf.Lerp(juicyPostProcessVolume.weight, 0.001f, juicyPostProcessVolumeOutLerpSpeed * Time.deltaTime);
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
            juicyPostProcessVolume.weight = Mathf.Lerp(juicyPostProcessVolume.weight, 1f, juicyPostProcessVolumeInLerpSpeed * Time.deltaTime);
            punchChargeTimer += Time.deltaTime;
            if (punchChargeTimer > strongPunchChargeTime && currentPunchIntensity != PunchIntensity.Strong)
                currentPunchIntensity = PunchIntensity.Strong;
            else if(punchChargeTimer > weakPunchChargeTime && currentPunchIntensity==PunchIntensity.None)
                currentPunchIntensity=PunchIntensity.Weak;
        }
        //BUTTON RELEASE WITH ENOUGH CHARGE -> PUNCH
        else if (currentPunchIntensity != PunchIntensity.None)
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
        juicyPostProcessVolume.weight = Mathf.Lerp(juicyPostProcessVolume.weight, 0.001f, juicyPostProcessVolumeOutLerpSpeed * Time.deltaTime);
        punchTimer += Time.deltaTime;
        if(currentPunchIntensity == PunchIntensity.Weak)
        {
            if(punchTimer > weakPunchTimeBeforeIdle)
            {
                SwitchState(PunchState.Idle);
            }
            else if (punchTimer > weakPunchTimeBeforeHit && !punchPerformed)
            {
                Punch();
            }
        }
        else if(currentPunchIntensity == PunchIntensity.Strong)
        {
            if (punchTimer > strongPunchTimeBeforeIdle)
            {
                SwitchState(PunchState.Idle);
            }
            else if(punchTimer> strongPunchTimeBeforeHit && !punchPerformed)
            {
                Punch();
            }
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
                currentPunchIntensity = PunchIntensity.None;
                break;
            case PunchState.Charge:
                punchChargeTimer = 0;
                print("Enter State Charge");
                break;
            case PunchState.Punch:
                punchTimer = 0;
                punchPerformed = false;
                print("current punch intensity: " + currentPunchIntensity);
                break;
            case PunchState.Cancel:
                cancelTimeBeforeIdle = 0;
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
        punchPerformed = true;
        Vector3 _punchIntensityAndForce;
        Vector3 _punchPositionOffset;
        
        //Setting values to differ depending on intensity
        if(currentPunchIntensity == PunchIntensity.Weak)
        {
            weakPunchCameraShake.GenerateImpulse();
            Vector3 punchDirection = weakPunchDirection.x * Camera.main.transform.right + weakPunchDirection.y * Camera.main.transform.up + weakPunchDirection.z * Camera.main.transform.forward;
            _punchIntensityAndForce = punchDirection.normalized * weakPunchForce;
            _punchPositionOffset = weakPunchOffset.x * Camera.main.transform.right + weakPunchOffset.y * Camera.main.transform.up + weakPunchOffset.z * Camera.main.transform.forward;
        }
        else
        {
            strongPunchCameraShake.GenerateImpulse();
            Vector3 punchDirection = strongPunchDirection.x * Camera.main.transform.right + strongPunchDirection.y * Camera.main.transform.up + strongPunchDirection.z * Camera.main.transform.forward;
            _punchIntensityAndForce = punchDirection.normalized * strongPunchForce;
            _punchPositionOffset = strongPunchOffset.x * Camera.main.transform.right + strongPunchOffset.y * Camera.main.transform.up + strongPunchOffset.z * Camera.main.transform.forward;
        }

        //Apply Force + tell entities they have been punched
        for(int i=0; i<targetsClose.punchReceivers.Count; i++)
        {
            targetsClose.punchReceivers[i].myRb.AddForceAtPosition(_punchIntensityAndForce, targetsClose.punchReceivers[i].transform.position+targetsClose.punchReceivers[i].myRb.centerOfMass+ _punchPositionOffset);
            targetsClose.punchReceivers[i].OnPunchReceived(currentPunchIntensity);
        }


        //lighter push on far targets
        if (currentPunchIntensity == PunchIntensity.Strong)
        {
            for (int i = 0; i < targetsFar.punchReceivers.Count; i++)
            {
                if (!targetsClose.punchReceivers.Contains(targetsFar.punchReceivers[i]))
                {
                    targetsFar.punchReceivers[i].myRb.AddForce(farTargetsForce * (targetsFar.punchReceivers[i].transform.position - transform.position).normalized);
                }
            }
        }
    }

    private IEnumerator WeakPunchFeedback()
    {
        float weakPunchTimer = 0;
        weakPunchCameraShake.GenerateImpulse();

        while(weakPunchTimer < weakPunchTimeBeforeIdle)
        {
            weakPunchTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }
    private IEnumerator StrongPunchFeedback()
    {
        float strongPunchTimer = 0;
        strongPunchCameraShake.GenerateImpulse();

        while (strongPunchTimer < weakPunchTimeBeforeIdle)
        {
            strongPunchTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }
}
