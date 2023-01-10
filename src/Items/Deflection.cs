using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deflection : MonoBehaviour
{

    public Transform headTrans;
    public Transform tip;
    public Transform hilt;
    public float lowAngleSensitivity;
    public float highAngleSensitivity;
    public float motionSensitivity;
    public AudioSource[] deflectLaserSounds;
    public GameObject deflectLaserEffect;
    public GameObject smokeEffect;

    private Vector3 lastTipPos;
    private Vector3 lastHiltPos;
    private Vector3 tipVel;
    private Vector3 hiltVel;


    // Start is called before the first frame update
    void Start()
    {
        lastTipPos = tip.transform.position;
        lastHiltPos = hilt.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        tipVel = tip.transform.position - lastTipPos;
        hiltVel = hilt.transform.position - lastHiltPos;
        lastTipPos = tip.transform.position;
        lastHiltPos = hilt.transform.position;
    }

    private Vector3 GetDeflectionDir(Vector3 laserDir)
    {
        laserDir = -laserDir;
        Vector3 lookDir = -laserDir;
        Vector3 rotateAxis = Vector3.Cross(lookDir, transform.up);
        float laserAngle = Vector3.SignedAngle(transform.up, laserDir, rotateAxis);
        if (laserAngle > 90)
        {
            laserAngle = 180 - laserAngle;
        }
        float lowAngleWeight = laserAngle / 90f;
        float highAngleWeight = 1.0f - lowAngleWeight;
        float realismPercent = lowAngleWeight * lowAngleSensitivity + highAngleWeight * highAngleSensitivity;
        if (realismPercent > 0.5f)
        {
            realismPercent = 0.5f;
        }
        float angle = realismPercent * 2 * Vector3.SignedAngle(Vector3.Cross(rotateAxis, transform.up), laserDir, rotateAxis);
        //float angle = 0.3f * 2 * Vector3.SignedAngle(Vector3.Cross(rotateAxis, transform.up), laserDir, rotateAxis);
        return (Quaternion.AngleAxis(-angle, rotateAxis) * laserDir).normalized;
    }

    private Vector3 GetSwingVector(Vector3 point)
    {
        float hiltDist = Vector3.Distance(point, hilt.transform.position);
        float tipDist = Vector3.Distance(point, tip.transform.position);
        float totalDist = hiltDist + tipDist;
        

        return ((hiltDist * hiltVel) + (tipDist * tipVel)) / totalDist;
    }

    public void Deflect(LaserController laser)
    {
        
       

        float laserMag = laser.vel.magnitude;
        GetSwingVector(laser.transform.position);
        Vector3 swingVector = GetSwingVector(laser.transform.position);
        if (swingVector.magnitude < 0.075)
        {
            swingVector = Vector3.zero;
        }
        Vector3 laserDir = (GetDeflectionDir(laser.vel) + swingVector * motionSensitivity);
        laserDir.Normalize();
        laser.vel = laserDir * (laserMag);

        deflectLaserSounds[(int)Random.Range(0, deflectLaserSounds.Length)].Play();
        GameObject go = Instantiate(deflectLaserEffect, laser.transform.position, Quaternion.LookRotation(laser.vel));
        go.transform.parent = laser.transform;
        Destroy(go, 5f);
        go = Instantiate(smokeEffect, laser.transform.position, Quaternion.identity);
        go.transform.parent = laser.transform;
        Destroy(go, 5f);
    }


}
