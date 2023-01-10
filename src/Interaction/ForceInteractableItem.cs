using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ForceInteractableItem : ForceInteractable
{
    public Transform snapTo = null;
    public UnityEvent activateMethod;
    public UnityEvent deactivateMethod;

    private bool active = false;

    // changes default grab to allow for snapping
    override public void OnGrab(ForceInteractor interactor)
    {
        rb.isKinematic = true;
        if (snapTo)
        {
            gameObject.transform.parent = interactor.transform;
            gameObject.transform.position = interactor.transform.position + snapTo.transform.localPosition;


            gameObject.transform.rotation = interactor.transform.rotation * Quaternion.Inverse(snapTo.transform.localRotation);
        }
        else
        {
            gameObject.transform.parent = interactor.transform;   
        }

    }

    // grabbed with force (first frame)
    override public void OnForceGrab(ForceInteractor interactor)
    {

        
        if (heldBy.Count == 0)
        {
            heldBy.Add(interactor);
            beingForceHeld = true;
            originalUseGravity = rb.useGravity;
            rb.useGravity = false;
            originalDrag = rb.drag;
            targetTrans.position = interactor.transform.position;
            targetTrans.rotation = interactor.transform.rotation;
            targetTrans.parent = interactor.transform;
        } else
        {
            Debug.Log("Can't double grab lightsaber");
        }

    }

    // grabbed with force (continuous)
    override public void OnForceHold(ForceInteractor interactor)
    {
        float dist = (targetTrans.position - transform.position).magnitude;
        float targetSpeed = maxSpeed;
        if (dist < 1f)
        {
            targetSpeed = 2f + (dist * (maxSpeed - 2));
        }
        Vector3 targetVelocity = targetSpeed * (targetTrans.position - transform.position).normalized;
        Vector3 diff = targetVelocity - rb.velocity;
        Vector3 force = diff * interactor.forceStrength;
        if (force.magnitude > interactor.forceStrength)
        {
            force *= (interactor.forceStrength / force.magnitude);
        }
        rb.AddForce(force * Time.fixedDeltaTime);

        // if close to hand then perform normal grab
        if (interactor.normalTargets.Contains(this.gameObject))
        {
            interactor.Release();
            interactor.Grab();
        }

    }

    public override void OnTriggerRelease(ForceInteractor interactor)
    {
        active = !active;
        if (active)
        {
            if (activateMethod != null)
            {
                activateMethod.Invoke();
            }

        }
        else
        {
            if (deactivateMethod != null)
            {
                deactivateMethod.Invoke();
            }
        }
    }
}
