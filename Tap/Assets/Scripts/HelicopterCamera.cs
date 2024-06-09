using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopterCamera : MonoBehaviour
{
    public Transform target; // The helicopter GameObject to follow
    public Vector3 offset = new Vector3(-0.03f, 2.65f, -5.24f);   // The offset from the helicopter

    public float followSpeed = 5f; // Speed at which the camera follows the helicopter

    private void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogError("Target not assigned to the helicopter camera!");
            return;
        }

        // Calculate the desired position of the camera based on the target position and offset
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);

        // Smoothly move the camera towards the desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Make the camera look at a point slightly above the helicopter
        Vector3 lookAtPosition = target.position + Vector3.up;
        transform.LookAt(lookAtPosition);
    }
}
