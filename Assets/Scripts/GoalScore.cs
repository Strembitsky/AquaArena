using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GoalScore : Scoreboard
{

    public TextMeshPro GText1, GText2, OText1, OText2;
    public Rigidbody PlayerBodyRigid, BallRigid;
    public GameObject GreenGoal, OrangeGoal, GreenExplosion, OrangeExplosion, GreenExplosionIn, OrangeExplosionIn;
    public Collider GreenTrigger, OrangeTrigger;
    public GameObject GreenSpawn, OrangeSpawn;
    public XRDirectClimbInteractor left, right;
    AudioSource goalScoreSound;
    public XRGrabVelocityTracked ballScript;
    Vector3 maxSize = new Vector3(100f, 100f, 100f);
    // Start is called before the first frame update
    void Start()
    {
        GreenExplosion.transform.localScale = new Vector3(0f, 0f, 0f);
        OrangeExplosion.transform.localScale = new Vector3(0f, 0f, 0f);
        GreenExplosionIn.transform.localScale = new Vector3(0f, 0f, 0f);
        OrangeExplosionIn.transform.localScale = new Vector3(0f, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (GreenExplosion.transform.localScale.magnitude > 0 && GreenExplosion.transform.localScale.magnitude < maxSize.magnitude)
        {
            GreenExplosion.transform.localScale += new Vector3(0.2f, 0.2f, 0.2f);
            GreenExplosionIn.transform.localScale += new Vector3(0.2f, 0.2f, 0.2f);
            BallRigid.velocity = Vector3.zero;
            BallRigid.position = GreenSpawn.transform.position;
        }
        else if (GreenExplosion.transform.localScale.magnitude > 0 && GreenExplosion.transform.localScale.magnitude >= maxSize.magnitude)
        {
            GreenExplosion.transform.localScale = new Vector3(0f, 0f, 0f);
            GreenExplosionIn.transform.localScale = new Vector3(0f, 0f, 0f);
            if (left.allowSelect == false || right.allowSelect == false)
            {
                left.allowSelect = true;
                right.allowSelect = true;
                OrangeTrigger.enabled = true;
                GreenTrigger.enabled = true;   
            }
        }
        if (OrangeExplosion.transform.localScale.magnitude > 0 && OrangeExplosion.transform.localScale.magnitude < maxSize.magnitude)
        {
            OrangeExplosion.transform.localScale += new Vector3(0.2f, 0.2f, 0.2f);
            OrangeExplosionIn.transform.localScale += new Vector3(0.2f, 0.2f, 0.2f);
            BallRigid.velocity = Vector3.zero;
            BallRigid.position = OrangeSpawn.transform.position;
        }
        else if (OrangeExplosion.transform.localScale.magnitude > 0 && OrangeExplosion.transform.localScale.magnitude >= maxSize.magnitude)
        {
            OrangeExplosion.transform.localScale = new Vector3(0f, 0f, 0f);
            OrangeExplosionIn.transform.localScale = new Vector3(0f, 0f, 0f);
            if (left.allowSelect == false || right.allowSelect == false)
            {
                left.allowSelect = true;
                right.allowSelect = true;
                OrangeTrigger.enabled = true;
                GreenTrigger.enabled = true;
            }
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Ball"))
        {
            if (this.name.Contains("Orange"))
            {
                OrangeTrigger.enabled = false;
                left.allowSelect = false;
                right.allowSelect = false;
                BallRigid.velocity = Vector3.zero;
                BallRigid.position = GreenSpawn.transform.position;
                Debug.Log("Green Team Scored!");
                goalScoreSound = GetComponent<AudioSource>();
                goalScoreSound.Play();
                if (ballScript.ballIsOrangeThree)
                {
                    GreenScore += 3;
                }
                else
                {
                    GreenScore += 2;
                }
                GText1.text = GreenScore.ToString();
                GText2.text = GreenScore.ToString();
                PlayerBodyRigid.AddForce((GreenGoal.transform.position - PlayerBodyRigid.position).normalized * 15f, ForceMode.Impulse);
                OrangeExplosion.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                OrangeExplosionIn.transform.localScale = new Vector3(0.045f, 0.045f, 0.045f);
                BallRigid.velocity = Vector3.zero;
                BallRigid.position = GreenSpawn.transform.position;
            }
            else if (this.name.Contains("Green"))
            {
                GreenTrigger.enabled = false;
                left.allowSelect = false;
                right.allowSelect = false;
                BallRigid.velocity = Vector3.zero;
                BallRigid.position = OrangeSpawn.transform.position;
                Debug.Log("Orange Team Scored!");
                goalScoreSound = GetComponent<AudioSource>();
                goalScoreSound.Play();
                if (ballScript.ballIsGreenThree)
                {
                    OrangeScore += 3;
                }
                else
                {
                    OrangeScore += 2;
                }
                OText1.text = OrangeScore.ToString();
                OText2.text = OrangeScore.ToString();
                PlayerBodyRigid.AddForce((OrangeGoal.transform.position - PlayerBodyRigid.position).normalized * 15f, ForceMode.Impulse);
                GreenExplosion.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                GreenExplosionIn.transform.localScale = new Vector3(0.045f, 0.045f, 0.045f);
                BallRigid.velocity = Vector3.zero;
                BallRigid.position = OrangeSpawn.transform.position;
            }
        }
    }

}
