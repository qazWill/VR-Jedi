using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformTracker : MonoBehaviour
{

    public Transform target;

    private Vector3 _positionOffset;
    private Quaternion _rotationOffset;

    private void Start()
    {
        if (target != null)
        {
            SetFakeParent(target);
        }
    }

    private void FixedUpdate()
    {
        if (target == null)
            return;

        var targetPos = target.position - _positionOffset;
        var targetRot = target.localRotation * _rotationOffset;

        transform.rotation = target.rotation;
        transform.position = RotatePointAroundPivot(targetPos, target.position, targetRot);
        //transform.localRotation = targetRot;


        
    }

    public void SetFakeParent(Transform parent)
    {
        //Offset vector
        _positionOffset = parent.position - transform.position;
        //Offset rotation
        _rotationOffset = Quaternion.Inverse(parent.localRotation * transform.localRotation);
        //Our fake parent
        target = parent;
    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        //Get a direction from the pivot to the point
        Vector3 dir = point - pivot;
        //Rotate vector around pivot
        dir = rotation * dir;
        //Calc the rotated vector
        point = dir + pivot;
        //Return calculated vector
        return point;
    }
}
