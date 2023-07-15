using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UI;

public class Speedometer : MonoBehaviour
{
    Rigidbody xrBody;
    TextMeshProUGUI speed;
    // Start is called before the first frame update
    void Start()
    {
        xrBody = GameObject.Find("XR Origin").GetComponent<Rigidbody>();
        speed = GetComponent<TextMeshProUGUI>();
    }



    void FixedUpdate()
    {
        speed.text = xrBody.velocity.magnitude.ToString("0.0") + " m/s";
    }
}
