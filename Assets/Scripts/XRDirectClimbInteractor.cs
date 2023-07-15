using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRDirectClimbInteractor : XRDirectInteractor
{
    public static event Action<string> ClimbHandActivated;
    public static event Action<string> ClimbHandDeactivated;
    public GameObject hand;

    private string _controllerName;
    public Vector3 grabPos;

    Collider ballCol, headCol;

    protected override void Start()
    {
        base.Start();
        _controllerName = gameObject.name;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {

        base.OnSelectEntered(args);

        if (args.interactableObject.transform.gameObject.tag == "Climb")
        {
            grabPos = hand.transform.position;
            ClimbHandActivated?.Invoke(_controllerName);
        }
        
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        ClimbHandDeactivated?.Invoke(_controllerName);
    }


}