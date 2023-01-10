using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceInteractable : MonoBehaviour
{
    public Material selectMaterial;
    [HideInInspector]
    public bool beingForceHeld = false;
    public List<ForceInteractor> targetedBy = new List<ForceInteractor>();
    public float maxSpeed = 4f;

    protected List<ForceInteractor> heldBy = new List<ForceInteractor>();
    protected Rigidbody rb;
    protected ForceInteractor[] allForceInteractors;
    protected Transform targetTrans;
    protected CharacterController character;
    protected float originalDrag;
    protected bool originalUseGravity;
    protected MeshRenderer mr;
    protected Color originalMaterialColor;



    private void Awake()
    {
        targetTrans = new GameObject().transform;
        character = (CharacterController)FindObjectOfType(typeof(CharacterController));
        rb = GetComponent<Rigidbody>();
        allForceInteractors = FindObjectsOfType<ForceInteractor>();
        foreach (var interactor in allForceInteractors)
        {
            interactor.potentialForceTargets.Add(this);
        }
        mr = GetComponent<MeshRenderer>();
        if (mr == null) {
            mr = GetComponentInChildren<MeshRenderer>();
        }
        originalMaterialColor = mr.material.color;
    }



    virtual public void FixedUpdate()
    {
        float factor = 0.12f + 0.02f * Mathf.Sin(8f * Time.fixedTime);
        if (targetedBy.Count > 0) {
            mr.material.color = Color.magenta * factor + (1 - factor) * originalMaterialColor;
        } else {
            mr.material.color = originalMaterialColor;
        }
    }


    // grabbed regularly (first frame)
    virtual public void OnGrab(ForceInteractor interactor)
    {
        rb.isKinematic = true;
        gameObject.transform.parent = interactor.gameObject.transform;
    }

    // grabbed regularly (continuous)
    virtual public void OnHold(ForceInteractor interactor)
    {

    }

    // stopped grabbing (last frame)
    virtual public void OnRelease(ForceInteractor interactor)
    {
        rb.isKinematic = false;
        gameObject.transform.parent = null;
    }

    // grabbed with force (first frame)
    virtual public void OnForceGrab(ForceInteractor interactor)
    {
        heldBy.Add(interactor);
        if (heldBy.Count == 1)
        {
            beingForceHeld = true;
            originalUseGravity = rb.useGravity;
            rb.useGravity = false;
            originalDrag = rb.drag;
            targetTrans.position = transform.position;
            targetTrans.rotation = transform.rotation;
            targetTrans.parent = interactor.transform;
        }
    }

    // grabbed with force (continuous)
    virtual public void OnForceHold(ForceInteractor interactor)
    {
        
        float dist = (targetTrans.position - transform.position).magnitude;
        float targetSpeed = maxSpeed;
        if (dist < 1f)
        {
            targetSpeed = dist * maxSpeed;
        }
        Vector3 targetVelocity = targetSpeed * (targetTrans.position - transform.position).normalized;
        Vector3 diff = targetVelocity - rb.velocity;
        Vector3 force = diff * interactor.forceStrength;
        if (force.magnitude > interactor.forceStrength)
        {
            force *= (interactor.forceStrength / force.magnitude);
        }
        rb.AddForce(force * Time.fixedDeltaTime);

        /*Vector3 dir = (targetTrans.position - transform.position).normalized;
        bool brake = false;
        if (Vector3.Distance(targetTrans.position, transform.position) < 1)
        {
            brake = true;
        }
        if (rb.velocity.magnitude > maxSpeed)
        {
            brake = true;
        }
        rb.AddForce(dir * interactor.forceStrength * Time.fixedDeltaTime);
        float angle = Vector3.Angle(rb.velocity, dir);
        if (angle > 90f) // changing direction is encouraged!
        {
            float boost = (angle - 90f) / 90f;
            rb.AddForce(dir * boost * interactor.forceStrength * Time.fixedDeltaTime);
        } 

        if (brake)
        {
            rb.drag = 2.0f;
        }
        else
        {
            rb.drag = 0.0f;
        }*/


    }

    // stopped grabbing with force (lastframe)
    virtual public void OnForceRelease(ForceInteractor interactor)
    {
        heldBy.Remove(interactor);
        if (heldBy.Count == 0)
        {
            beingForceHeld = false;
            rb.useGravity = originalUseGravity;
            rb.drag = originalDrag;
        } else
        {
            targetTrans.parent = heldBy[0].transform;
        }
    }


    virtual public void OnTriggerPress(ForceInteractor interactor)
    {

    }

    virtual public void OnTriggerHold(ForceInteractor interactor)
    {

    }

    virtual public void OnTriggerRelease(ForceInteractor interactor)
    {

    }

    virtual public void OnForceTriggerPress(ForceInteractor interactor)
    {
        OnTriggerPress(interactor);
    }

    virtual public void OnForceTriggerHold(ForceInteractor interactor)
    {
        OnTriggerHold(interactor);
    }

    virtual public void OnForceTriggerRelease(ForceInteractor interactor)
    {
        OnTriggerRelease(interactor);
    }


    virtual public void OnTriggerEnter(Collider other)
    {
        ForceInteractor interactor = other.gameObject.GetComponent<ForceInteractor>();
        if (interactor)
        {
            interactor.normalTargets.Add(this.gameObject);
        }
    }

    virtual public void OnTriggerExit(Collider other)
    {
        ForceInteractor interactor = other.gameObject.GetComponent<ForceInteractor>();
        if (interactor)
        {
            interactor.normalTargets.Remove(this.gameObject);
        }
    }

    virtual public void MarkSelected() {
        /*if (selectMaterial) {
            Material[] currentMats = mr.materials;
            bool contains = false;
            foreach (var mat in currentMats) {
                if (mat.ToString() == selectMaterial.ToString()) {
                    contains = true;
                }
            }
            
        }*/
    }

    virtual public void MarkNotSelected()
    {

    }
}
