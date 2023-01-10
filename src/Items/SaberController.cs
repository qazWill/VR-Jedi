using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaberController : MonoBehaviour
{
    public GameObject particleObject;
    public GameObject lightObject;
    public GameObject hitboxObject;
    public AudioSource igniteSound;
    public AudioSource humSound;
    public AudioSource deactivateSound;

    [HideInInspector]
    public bool ignited = false;
    private ParticleSystem ps;
    private float originalLifeTime;
    private float originalPlaybackSpeed;
    private Light light;
    private MeshRenderer trailEffect;

    // Start is called before the first frame update
    void Start()
    {
        ps = particleObject.GetComponent<ParticleSystem>();
        light = lightObject.GetComponent<Light>();
        originalLifeTime = ps.startLifetime;
        originalPlaybackSpeed = ps.playbackSpeed;
        hitboxObject.GetComponent<CapsuleCollider>().enabled = false;
        trailEffect = GetComponentInChildren<MeshRenderer>();
        trailEffect.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!ignited)
        {
            if (ps.startLifetime > 0) {
                ps.startLifetime -= 8 * Time.deltaTime;
                if (ps.startLifetime <= 0.01)
                {
                    ps.startLifetime = 0.01f;
                    ps.Stop();
                    light.enabled = false;
                }
            }
        }
    }

    public void Ignite()
    {
        ignited = !ignited;

        if (ignited)
        {
            ps.startLifetime = originalLifeTime;
            ps.playbackSpeed = originalPlaybackSpeed;
            ps.Play();
            igniteSound.Play();
            humSound.Play();
            light.enabled = true;
            hitboxObject.SetActive(true);
            hitboxObject.GetComponent<CapsuleCollider>().enabled = true;
            trailEffect.enabled = true;


        }
        else
        {
            ps.playbackSpeed *= 6f;
            igniteSound.Stop();
            humSound.Stop();
            deactivateSound.Play();
            hitboxObject.SetActive(false);
            hitboxObject.GetComponent<CapsuleCollider>().enabled = false;
            trailEffect.enabled = false;
        }
    }
}
