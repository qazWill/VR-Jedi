using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    public float speed;
    public AudioSource hitWallSound;
    public GameObject effectObj;

    [HideInInspector]
    public Vector3 vel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += vel * Time.fixedDeltaTime;
        transform.rotation = Quaternion.LookRotation(vel);
    }

    public void shoot(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        vel = dir * speed;
        transform.rotation = Quaternion.LookRotation(vel);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Blade"))
        {
            other.gameObject.GetComponent<Deflection>().Deflect(this);
            return;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (LayerMask.GetMask("Enemy") == (LayerMask.GetMask("Enemy") | (1 << other.gameObject.layer)))
        {
            other.gameObject.GetComponent<EnemyController>().ReceiveLaserHit(this);
            return;
        }

        hitWallSound.Play();
        this.gameObject.GetComponent<CapsuleCollider>().enabled = false;
        //this.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        effectObj.SetActive(false);
    }
    
}
