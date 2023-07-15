using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AquaSphere : MonoBehaviour
{
    public GameObject Center;
    public Rigidbody AquaBallBody;
    public Rigidbody PlayerBody;
    public AudioSource ballLeaveWater, ballJoinWater;
    bool ballWasOut;

    private void FixedUpdate()
    {
        if (Vector3.Distance(Center.transform.position, AquaBallBody.worldCenterOfMass) >= 50)
        {
            if (!ballWasOut)
            {
                ballLeaveWater.Play();
                ballWasOut = true;
            }
            AquaBallBody.AddForce(((Center.transform.position - AquaBallBody.worldCenterOfMass).normalized * 10f), ForceMode.Acceleration);
        }
        else if ((ballWasOut) && Vector3.Distance(Center.transform.position, AquaBallBody.worldCenterOfMass) < 50)
        {
            ballJoinWater.Play();
            ballWasOut = false;
        }
        if (Vector3.Distance(Center.transform.position, PlayerBody.worldCenterOfMass) >= 50)
        {
            PlayerBody.AddForce(((Center.transform.position - PlayerBody.worldCenterOfMass).normalized * 10f), ForceMode.Acceleration);
        }
    }
}
