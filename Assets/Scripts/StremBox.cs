using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SpatialTracking;

public class StremBox : MonoBehaviour
{
    // declarations can clean up
    GameObject xrRig;
    Rigidbody XR_Rigidbody;
    float stremCatchSpeed;
    Vector3 postCollisionRigidbodySpeed;
    Vector3 currentRigidbodySpeed;
    Vector3 preCollisionRigidbodySpeed;
    public bool colliding;
    GameObject mainCam;
    Camera camCamera;
    TrackedPoseDriver trackedDriver;
    GameObject Stremming;
    Collider meshColl;


    // Start is called before the first frame update
    void Start()
    {
        colliding = true;
        xrRig = GameObject.Find("XR Origin");
        XR_Rigidbody = xrRig.GetComponent<Rigidbody>();
        mainCam = GameObject.Find("Main Camera");
        camCamera = mainCam.GetComponent<Camera>();
        Stremming = GameObject.Find("Complete XR Origin Set Up");
        meshColl = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // updates independent of framerate
    void FixedUpdate()
    {
        // gets the current rigidbody speed
        currentRigidbodySpeed = XR_Rigidbody.velocity;

        // if the player is currently within the stremsphere
        if (colliding == true)
        {
            // if the player is moving slowly
            if (XR_Rigidbody.velocity.magnitude <= 0.05f)
            {
                // make the stremsphere follow player slowly
                stremCatchSpeed = 0.05f;
                transform.position = Vector3.MoveTowards(transform.position, XR_Rigidbody.worldCenterOfMass, stremCatchSpeed * Time.fixedDeltaTime);
            }
            else
            {
                // if the player is moving fast, make stremsphere follow player at the same speed as the player
                stremCatchSpeed = XR_Rigidbody.velocity.magnitude;
                transform.position = Vector3.MoveTowards(transform.position, XR_Rigidbody.worldCenterOfMass, stremCatchSpeed * Time.fixedDeltaTime);
            }
        }
        else
        {
            // if the player is NOT within the stremsphere
            // increase the stremsphere speed over and over
            stremCatchSpeed += 0.05f;

            // and also move the stremsphere towards the player camera at this increased speed
            transform.position = Vector3.MoveTowards(transform.position, XR_Rigidbody.worldCenterOfMass, stremCatchSpeed * Time.fixedDeltaTime);

            // and also move the player parent object back towards the stremsphere at this increase speed
            Stremming.transform.position = Vector3.MoveTowards(Stremming.transform.position, transform.position-XR_Rigidbody.worldCenterOfMass, (stremCatchSpeed * Time.fixedDeltaTime));
        }
    }

    // player just entered the sphere
    void OnTriggerEnter(Collider collision)
    {
        colliding = true;
        Debug.Log("Strem Collision Enter");
    }


    // if the player is inside the sphere, this will get called
    void OnTriggerStay(Collider collision)
    {
        if (!colliding)
            colliding = true;
        
    }

    // first instance of player exiting sphere
    void OnTriggerExit(Collider collision)
    {
        Debug.Log("Strem Collision Exit");
        colliding = false;
    }
}
