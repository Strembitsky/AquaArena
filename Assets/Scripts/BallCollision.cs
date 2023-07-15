using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCollision : MonoBehaviour
{
    public Rigidbody BallBody;
    Vector3 currentVelocity;
    public AudioSource softHit, hardHit;
    // Start is called before the first frame update
    void Start()
    {
        currentVelocity = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        currentVelocity = BallBody.velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag.Contains("Soft"))
        {
            softHit.Play();
        }
        if (collision.collider.gameObject.tag.Contains("Metal"))
        {
            hardHit.Play();
        }
        BallBody.velocity = Vector3.Reflect(currentVelocity, collision.contacts[0].normal)*0.5f;
    }

}
