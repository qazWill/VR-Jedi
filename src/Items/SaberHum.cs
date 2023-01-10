using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaberHum : MonoBehaviour
{
    public Transform tip;
    public AudioSource[] swingSounds;

    private Vector3 lastTipPos;
    private Vector3 tipVel;
    private float swingTime;
    private Vector3 lastVel;
    private bool swinging;

    // Start is called before the first frame update
    void Start()
    {
        lastTipPos = tip.transform.position;
        lastVel = Vector3.zero;
        swingTime = 0.3f;
        swinging = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (tipVel.magnitude > 0.15)
        {
            lastVel = tipVel;
        }

        swingTime -= Time.deltaTime;
        tipVel = tip.transform.position - lastTipPos;
        lastTipPos = tip.transform.position;
        

        
        if (tipVel.magnitude > 0.2f)
        {
            if (swinging)
            {
                //changeDirection();
            }
            if (swingTime <= 0.0f)
            {
                Swing();
            }

        }
    }


    void changeDirection()
    {
        float angle = Vector3.Angle(lastVel, tipVel);
        if (angle > 160.0f)
        {
            Debug.Log(angle);
            swinging = false;
            swingTime = 0.0f;
        } else
        {
            //lastVel = tipVel;
        }
    }

    void Swing()
    {
        float volume = 0.3f + (tipVel.magnitude * 2.0f);
        if (volume < 0)
        {
            return;
        }
        if (volume > 1.0f)
        {
            volume = 1.0f;
        }
        int index = Random.Range(0, swingSounds.Length);
        index = 2;
        swingSounds[index].pitch = Random.Range(0.9f, 1.1f);
        swingSounds[index].volume = volume;
        swingSounds[index].Play();
        swingTime = 0.3f;
        swinging = true;
        lastVel = tipVel;
    }
}
