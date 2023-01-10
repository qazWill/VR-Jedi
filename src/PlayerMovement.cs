using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerMovement : MonoBehaviour
{
    public float speed;
    public float rotateSpeed;
    public float gravityAccel = 9.81f;
    public float fallSpeed = 0.0f;
    public LayerMask groundLayer;
    public XRNode inputNodeLeft;
    public XRNode inputNodeRight;
    private XRRig rig;
    private Vector2 axisLeft;
    private Vector2 axisRight;
    private CharacterController character;

    // Start is called before the first frame update
    void Start()
    {
        character = GetComponent<CharacterController>();
        rig = GetComponent<XRRig>();
    }

    // Update is called once per frame
    void Update()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(inputNodeLeft);
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out axisLeft);

        device = InputDevices.GetDeviceAtXRNode(inputNodeRight);
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out axisRight);
    
        CapsuleFollowHeadset();
        Quaternion headYaw = Quaternion.Euler(0, rig.cameraGameObject.transform.eulerAngles.y, 0);
        Vector3 dir = headYaw * new Vector3(axisLeft.x, 0, axisLeft.y) ;
        character.Move(dir * speed * Time.deltaTime);

        // gravity
        if (CheckIfGrounded())
        {
            fallSpeed = 0f;
        } else
        {
            fallSpeed += gravityAccel * Time.deltaTime;
        }
        character.Move(Vector3.down * fallSpeed * Time.deltaTime);

        RotateRig();
    }

    private void CapsuleFollowHeadset()
    {
        character.height = rig.cameraInRigSpaceHeight + 0.2f;
        Vector3 newCenter = transform.InverseTransformPoint(rig.cameraGameObject.transform.position);
        character.center = new Vector3(newCenter.x, character.height / 2 + character.skinWidth, newCenter.z);


    }

    private void RotateRig()
    {
        Vector3 center = rig.cameraGameObject.transform.position;
        float deltaAngle = rotateSpeed * axisRight.x * Time.fixedDeltaTime;
        rig.gameObject.transform.RotateAround(new Vector3(center.x, 0, center.z), Vector3.up, deltaAngle);
    }

    private bool CheckIfGrounded()
    {
        Vector3 rayStart = transform.TransformPoint(character.center);
        float rayLength = character.center.y + 0.01f;
        return Physics.SphereCast(rayStart, character.radius, Vector3.down, out RaycastHit hitInfo, rayLength, groundLayer);
    }
}
