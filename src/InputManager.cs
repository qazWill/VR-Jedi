using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class InputManager : MonoBehaviour
{

    // the transforms of the virtual hands
    public Transform leftTransform;
    public Transform rightTransform;
    public Transform headTransform;

    private List<InputDevice> leftDevices = new List<InputDevice>();
    private List<InputDevice> rightDevices = new List<InputDevice>();

    // for reference by rest of program
    private bool leftGrip = false;
    private bool leftTrigger = false;

    private bool rightGrip = false;
    private bool rightTrigger = false;


    public bool getLeftGrip()
    {
        return leftGrip;
    }

    public bool getLeftTrigger()
    {
        return leftTrigger;
    }

    public bool getRightGrip()
    {
        return rightGrip;
    }

    public bool getRightTrigger()
    {
        return rightTrigger;
    }

    // Update is called once per frame
    void Update()
    {
        

        // gets left hand input
        InputDevices.GetDevicesWithRole(InputDeviceRole.LeftHanded, leftDevices);
        if (leftDevices.Count >= 1)
        {
            InputDevice leftController = leftDevices[0]; // assumming only one device
            leftController.TryGetFeatureValue(CommonUsages.triggerButton, out leftTrigger);
            leftController.TryGetFeatureValue(CommonUsages.gripButton, out leftGrip);
            //leftController.TryGetFeatureValue(CommonUsages.primaryButton, out leftPrimary);
        }

        // gets right hand input
        InputDevices.GetDevicesWithRole(InputDeviceRole.RightHanded, rightDevices);
        if (leftDevices.Count >= 1)
        {
            InputDevice rightController = rightDevices[0]; // assumming only one device
            rightController.TryGetFeatureValue(CommonUsages.triggerButton, out rightTrigger);
            rightController.TryGetFeatureValue(CommonUsages.gripButton, out rightGrip);
        }
    }
}
