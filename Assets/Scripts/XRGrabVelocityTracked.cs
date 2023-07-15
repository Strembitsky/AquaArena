using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class XRGrabVelocityTracked : XRGrabInteractable
{
    public bool ballIsOrangeThree = false, ballIsGreenThree = false;
    public Rigidbody ballBody, playerBody;
    public GameObject orangeBubble, greenBubble;
    public bool canBackBoost = true, canChargeBoost = true, normalizeSpeed = false;
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
    }

    protected void FixedUpdate()
    {
        if (normalizeSpeed && playerBody.velocity.magnitude > 4.7f)
        {
            playerBody.velocity = playerBody.velocity.normalized * 4.7f;
        }
    }

    public void checkIfThree()
    {
        if (Vector3.Distance(ballBody.worldCenterOfMass, orangeBubble.transform.position) >= 22.5)
        {
            ballIsOrangeThree = true;
        }
        else
        {
            ballIsOrangeThree = false;
        }
        if (Vector3.Distance(ballBody.worldCenterOfMass, greenBubble.transform.position) >= 22.5)
        {
            ballIsGreenThree = true;
        }
        else
        {
            ballIsGreenThree = false;
        }
    }

    public void normalizePlayerSpeed()
    {
        canBackBoost = false;
        canChargeBoost = false;
        normalizeSpeed = true;
    }

    public void allowBoosting()
    {
        canBackBoost = true;
        canChargeBoost = true;
        normalizeSpeed = false;
    }

}