using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CirclePass : MonoBehaviour
{
    public float rotationSpeed = 1f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime * Vector3.down);
    }
}
