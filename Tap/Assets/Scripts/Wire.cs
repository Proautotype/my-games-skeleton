using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

public class Wire : MonoBehaviour
{
    [SerializeField] Transform startTransform, endTransform;
    [SerializeField] int segmentCount = 10;
    [SerializeField] float totalLength = 10;

    [SerializeField] float radius = 0.5f;

    [SerializeField] float totalWeight = 10;
    [SerializeField] float drag = 1;
    [SerializeField] float angularDrag = 1;

    [SerializeField] bool usePhysics = false;

    Transform[] segments = new Transform[0]; 
    [SerializeField] Transform segmentParent;
    private int prevSegmentCount = 0;

    [SerializeField]
    private GameObject ropePart;


    private void Update()
    {
        if(prevSegmentCount != segmentCount)
        {
            segments = new Transform[segmentCount];
            RemoveSegment();
            GenerateSegments();
        }
        prevSegmentCount = segmentCount;
    }

    private void RemoveSegment()
    {
        print("segments -> " + segments.Length);
       foreach(Transform t in segments)
        {
            if (t)
            {
             Destroy(t.gameObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if(segments.Length > 0)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                Gizmos.DrawWireSphere(segments[i].position, 0.1f);
            }
        }
    }

    private void GenerateSegments()
    {
        JoinSegment(startTransform, null, true);
        Transform prevTransform = startTransform;

        Vector3 direction = (endTransform.position - startTransform.position);

        /*
            Instantiate(partPrefab, new Vector3(transform.position.x, transform.position.y + (partDistance * (x + 1)) , transform.position.z), Quaternion.identity, parentObject.transform);
         */

        for (int i = 0; i < segmentCount; i++) {
            GameObject segment = Instantiate(ropePart);
            
            segment.transform.parent = segmentParent;

            Vector3 pos = prevTransform.position + ((direction / segmentCount) / 1.1f);
            segment.transform.position = pos;

            segments[i] = segment.transform;

            JoinSegment(segment.transform, prevTransform);

            prevTransform = segment.transform;

        }
        JoinSegment(endTransform, prevTransform, false, true);

    }

    private void JoinSegment(Transform current, Transform connectedTransform, bool isKinetic = false, bool isCloseConnected = false)
    {
        if(current.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rigidbody = current.AddComponent<Rigidbody>();
            rigidbody.isKinematic = isKinetic;
            rigidbody.mass = totalWeight / segmentCount;
            rigidbody.drag = drag;
            rigidbody.angularDrag = angularDrag;
        }

        if(usePhysics)
        {
            SphereCollider sphereCollider = current.AddComponent<SphereCollider>();
            sphereCollider.radius = radius;

        }
        
        if(connectedTransform != null)
        {
            ConfigurableJoint joint = current.GetComponent<ConfigurableJoint>();
            if(joint == null)
            {
                joint = current.AddComponent<ConfigurableJoint>();
            }
            joint.connectedBody = connectedTransform.GetComponent<Rigidbody>();

            joint.autoConfigureConnectedAnchor = false;

            if (isCloseConnected)
            {
                joint.connectedAnchor = Vector3.forward * 0.1f;
            }
            else
            {
                joint.connectedAnchor = Vector3.forward * (totalLength / segmentCount);
            }

            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;

            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Limited;

            SoftJointLimit softJointLimit = new SoftJointLimit();
            softJointLimit.limit = 0;
            joint.angularZLimit = softJointLimit;

            JointDrive jointDrive = new JointDrive();
            jointDrive.positionDamper = 0;
            jointDrive.positionSpring = 0;
            joint.angularXDrive = jointDrive;
            joint.angularYZDrive = jointDrive;
        }

    }
}
