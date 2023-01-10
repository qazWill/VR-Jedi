using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingSphereController : EnemyController
{
    public Transform target;
    public GameObject laserPrefab;
    public GameObject smokeTrailPrefab;
    public float targetDist;
    public AudioSource fireSound;
    public float initialWait;

    private Rigidbody rb;
    private Vector3 waypoint;
    private float waitTime;
    private float shootTime;
    private bool disabled = false;
    private bool grounded = false;
    private Vector3 crashDir;
    bool reachedTarget = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        waitTime = 3.0f + initialWait;
        shootTime = 3.0f;
    }



    void FixedUpdate()
    {
        
        if (disabled)
        {
            if (!grounded) {
                rb.drag = 0.2f;
                rb.AddForce(crashDir, ForceMode.Acceleration);
                rb.AddForce(Vector3.down * 2, ForceMode.Acceleration);
            }
            
            return;
        }


        // brake
        rb.drag = 0.2f;
        if (rb.velocity.magnitude > 5.0f)
        {
            rb.drag = 2f;
            //Vector3 dir = -rb.velocity.normalized;
            //rb.AddForce(dir * 200 * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
        


        waitTime -= Time.fixedDeltaTime;
        shootTime -= Time.fixedDeltaTime;
        Vector3 dif = waypoint - transform.position;
        if (dif.magnitude > 0.5 && !reachedTarget)
        {
           
            Vector3 dir = dif.normalized;
            rb.AddForce(dir * 5, ForceMode.Acceleration);
            
        } else
        {
            reachedTarget = true;
            rb.drag = 10f;
            //Vector3 dir = -rb.velocity.normalized;
            //rb.AddForce(dir * 200 * Time.fixedDeltaTime, ForceMode.Acceleration);
            //rb.velocity = Vector3.zero;

            if (shootTime <= 0)
            {
                shoot();
            shootTime = Random.Range(0.5f, 2f);
            }
        }
       
        
        if (waitTime <= 0)
        {
            chooseWaypoint();
            waitTime = 3.0f;
        }
    }

    private void shoot()
    {
        fireSound.Play();
        Vector3 dir = (target.position - transform.position).normalized;
        GameObject laser = Instantiate(laserPrefab, transform.position + dir * 1f, Quaternion.identity);
        Destroy(laser, 20f);
        LaserController laserController = laser.GetComponent<LaserController>();
        laserController.shoot(target.position + new Vector3(Random.Range(-0.15f, 0.15f), Random.Range(0.15f, -0.15f), Random.Range(-0.15f, 0.15f)));
        


    }

    void chooseWaypoint()
    {
        Vector3 dif = new Vector3(target.transform.position.x, 0, target.transform.position.z);
        dif -= new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 dir = dif.normalized;
        Vector3 perp = Vector3.Cross(dir, Vector3.up);

        waypoint = target.transform.position - dir * targetDist;
        waypoint += perp * Random.Range(-1f, 1f);
        waypoint += Vector3.up * Random.Range(-0.5f, 0.5f);
        reachedTarget = false;
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (disabled)
        {
            rb.useGravity = true;
            grounded = true;
            rb.drag = 1f;
            return;
        }
        chooseWaypoint();
        
    }

    public override void ReceiveLaserHit(LaserController laser)
    {
        disabled = true;
        crashDir = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
        GameObject smokeTrail = Instantiate(smokeTrailPrefab, transform);
        smokeTrail.transform.parent = transform;
        Destroy(smokeTrail, 10f);
    }
}
