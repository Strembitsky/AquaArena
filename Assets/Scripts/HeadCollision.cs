using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeadCollision : MonoBehaviour
{
    public Rigidbody XROBody;
    Vector3 currentVelocity;
    public AudioSource headHit;
    // Start is called before the first frame update
    void Start()
    {
        currentVelocity = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        currentVelocity = XROBody.velocity;
    }

    private void FixedUpdate()
    {
        transform.position = Camera.main.transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        XROBody.velocity = Vector3.Reflect(currentVelocity, collision.contacts[0].normal);
        headHit.Play();
    }
}
