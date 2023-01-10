using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ForceInteractor : MonoBehaviour
{
    public XRNode node;
    public LayerMask forceInteractLayerMask;
    public Transform selectTransform = null;
    public float forceStrength = 100f;

    [HideInInspector]
    public List<ForceInteractable> potentialForceTargets = new List<ForceInteractable>(); // targetable at range with force
    public List<GameObject> normalTargets; // targetable with direct contact (normal grabbing)

    private GameObject heldObject = null;
    private ForceInteractable heldController = null;
    private bool usingForce = false;
    private InputDevice device;
    private bool isGripping = false;
    private bool isTriggering = false;
    private float foundNewTime = 0f;
    private ForceInteractable oldTarget = null;
    private ForceInteractable forceTarget;



    void Start()
    {
        normalTargets = new List<GameObject>();

    }

    void FixedUpdate()
    {
        bool wasGripping = isGripping;
        device = InputDevices.GetDeviceAtXRNode(node);
        device.TryGetFeatureValue(CommonUsages.gripButton, out isGripping);
        if (!wasGripping && !isGripping)
        {
            Idle();
        } else if (!wasGripping && isGripping)
        {
            Grab();
        }
        else if (wasGripping && isGripping)
        {
            Hold();
        }
        else
        {
            Release();
        }

        bool wasTriggering = isTriggering;
        device.TryGetFeatureValue(CommonUsages.triggerButton, out isTriggering);
        if (!wasTriggering && !isTriggering)
        {
            
        }
        else if (!wasTriggering && isTriggering)
        {
            TriggerPress();
        }
        else if (wasTriggering && isTriggering)
        {
            TriggerHold();
        }
        else
        {
            TriggerRelease();
        }
    }

    // runs when not gripping
    // trigger haptics and visual indicator of possible interactions
    void Idle()
    {
        if (oldTarget != null)
        {
            oldTarget.targetedBy.Remove(this);
        }
        forceTarget = null;
        RaycastHit[] hits = new RaycastHit[0];
        hits = Physics.RaycastAll(transform.position, selectTransform.forward, 20f, forceInteractLayerMask);
        foreach (var hit in hits)
        {
            ForceInteractable potentialTarget = hit.transform.gameObject.GetComponent<ForceInteractable>();
            if (potentialTarget != null)
            {
                forceTarget = potentialTarget;
                break;
            }
        }

        if (forceTarget == null)
        {
            forceTarget = GetNearestTargetInCone();
        }
        
        if (forceTarget != null)
        {
            forceTarget.targetedBy.Add(this);
            if (forceTarget != oldTarget)
            {
                foundNewTime = 0.2f;
                oldTarget = forceTarget;
            }
            
            foundNewTime -= Time.deltaTime;
            float amplitude = 0.1f;
            if (foundNewTime > 0f)
            {
                amplitude = 0.8f;
            }
            device.SendHapticImpulse(0, amplitude, 0.05f);
        } else
        {
            
        }
    }

    // runs for first frame user grips
    public void Grab()
    {
        if (normalTargets.Count > 0)
        {
            heldObject = normalTargets[0];
            heldController = heldObject.GetComponent<ForceInteractable>();
            usingForce = false;
            heldController.OnGrab(this);
        }
        else if (forceTarget != null)
        {
            heldObject = forceTarget.gameObject;
            heldController = forceTarget;
            usingForce = true;
            heldController.OnForceGrab(this);
            forceTarget.targetedBy.Remove(this);
            forceTarget = null;
            oldTarget = null;
        }
        else
        {
            heldObject = null;
            usingForce = true;
        }

    }

    // runs while user grips
    void Hold()
    {
        if (heldObject)
        {
            if (usingForce)
            {
                heldController.OnForceHold(this);
                device.SendHapticImpulse(0, 0.1f, 0.1f);
            } else
            {
                heldController.OnHold(this);
            }
        }
    }

    // runs first frame user is not gripping
    public void Release()
    {
        if (heldObject)
        {
            if (usingForce)
            {
                heldController.OnForceRelease(this);
            }
            else
            {
                heldController.OnRelease(this);
            }
        }
        heldObject = null;
        heldController = null;
        usingForce = false;
    }

    // runs first frame that trigger is held
    private void TriggerPress()
    {
        if (heldObject)
        {
            if (usingForce)
            {
                heldController.OnForceTriggerPress(this);
            }
            else
            {
                heldController.OnTriggerPress(this);
            }

        }
    }

    // runs while trigger is held
    private void TriggerHold()
    {
        if (heldObject)
        {
            if (usingForce)
            {
                heldController.OnForceTriggerHold(this);
            }
            else
            {
                heldController.OnTriggerHold(this);
            }
        }
    }

    // runs first frame that trigger is released
    private void TriggerRelease()
    {
        if (heldObject)
        {
            if (usingForce)
            {
                heldController.OnForceTriggerRelease(this);
            }
            else
            {
                heldController.OnTriggerRelease(this);
            }
        }
    }


    private ForceInteractable GetNearestTargetInCone()
    {
        ForceInteractable best = null;
        float bestDist = 999f;
        foreach (var interactable in potentialForceTargets)
        {
            Vector3 aimDir = selectTransform.forward;
            Vector3 actualDir = interactable.transform.position - transform.position;
            Vector3 projectedPoint = (Vector3.Dot(actualDir, aimDir) / aimDir.magnitude) * aimDir.normalized;
            float selectDist = (actualDir - projectedPoint).magnitude;
            float maxDist = projectedPoint.magnitude * 0.5f;
            if (selectDist < maxDist && selectDist < bestDist) // and > 0 !!!!!!
            {
                best = interactable;
                bestDist = selectDist;
            }
        }
        return best;
    }
}
