using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ClimbProvider : MonoBehaviour
{
    public static event Action ClimbActive;
    public static event Action ClimbInactive;

    public InputActionProperty velocityLeft;
    public InputActionProperty velocityRight;

    private bool _leftActive = false;
    private bool _rightActive = false;
    private bool _velocityStopped = false;

    public Rigidbody PlayerRigid;
    private GameObject PlayerParent;
    public GameObject LHandBone, LShoulderBone, RHandBone, RShoulderBone;

    public Vector3 leftGrabPos, rightGrabPos;
    float armDistance;
    public XRDirectClimbInteractor LClimbScript, RClimbScript;
    public CustomIK rightIKScript, leftIKScript;

    void Start()
    {
        XRDirectClimbInteractor.ClimbHandActivated += HandActivated;
        XRDirectClimbInteractor.ClimbHandDeactivated += HandDeactivated;
        PlayerRigid = GameObject.Find("XR Origin").GetComponent<Rigidbody>();
        PlayerParent = GameObject.Find("Complete XR Origin Set Up");
    }
    void OnDestroy()
    {
        XRDirectClimbInteractor.ClimbHandActivated -= HandActivated;
        XRDirectClimbInteractor.ClimbHandDeactivated -= HandDeactivated;
    }

    private void HandActivated(string controllerName)
    {
        if (controllerName.Contains("Left"))
        {
            _leftActive = true;
            _rightActive = false;
            leftGrabPos = LClimbScript.grabPos;
        }
        else if (controllerName.Contains("Right"))
        {
            _leftActive = false;
            _rightActive = true;
            rightGrabPos = RClimbScript.grabPos;
        }
        ClimbActive?.Invoke();
    }

    private void HandDeactivated(string controllerName)
    {
        if (_rightActive == true && controllerName.Contains("Right"))
        {
            _rightActive = false;
            ClimbInactive?.Invoke();
            Vector3 velocity = velocityRight.action.ReadValue<Vector3>();
            PlayerRigid.velocity = (PlayerRigid.rotation * -velocity * 1.15f);
            _velocityStopped = false;
        }
        else if (_leftActive == true && controllerName.Contains("Left"))
        {
            _leftActive = false;
            ClimbInactive?.Invoke();
            Vector3 velocity = velocityLeft.action.ReadValue<Vector3>();
            PlayerRigid.velocity = (PlayerRigid.rotation * -velocity * 1.15f);
            _velocityStopped = false;
        }
    }

    private void Update()
    {
        if (_rightActive == true || _leftActive == true)
        {
            if (!_velocityStopped)
            {
                PlayerRigid.velocity = Vector3.zero;
                _velocityStopped = true;
            }
                
            Climb();
        }
    }

    private void Climb()
    {
        Vector3 velocity = _leftActive ? velocityLeft.action.ReadValue<Vector3>() : velocityRight.action.ReadValue<Vector3>();
        if (_rightActive)
        {
            PlayerParent.transform.position += (PlayerRigid.rotation * -velocity * Time.deltaTime);
            if ((rightGrabPos - PlayerRigid.worldCenterOfMass).magnitude >= (rightIKScript.armLength*0.5))
            {
                PlayerParent.transform.position -= ((rightIKScript.endBone.position - rightGrabPos)) * 5f * Time.deltaTime;
            }
        }
        else if (_leftActive)
        {
            PlayerParent.transform.position += (PlayerRigid.rotation * -velocity * Time.deltaTime);
            if ((leftGrabPos - PlayerRigid.worldCenterOfMass).magnitude >= (leftIKScript.armLength * 0.5))
            {
                PlayerParent.transform.position -= ((leftIKScript.endBone.position - leftGrabPos)) * 5f * Time.deltaTime;
            }
        }
    }
}