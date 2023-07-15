using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMusic : MonoBehaviour
{
    public AudioSource audioAudio;
    public XRGrabVelocityTracked ballScript;
    // Start is called before the first frame update
    void Start()
    {
        audioAudio = GetComponent<AudioSource>();
        audioAudio.Play();
        ballScript.canBackBoost = true;
        ballScript.canChargeBoost = true;
    }
}