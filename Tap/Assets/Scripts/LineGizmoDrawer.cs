using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LineGizmoDrawer : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 StartPoint;
    public Vector3 EndPoint;
    public Color gizmoColor;
    private void Start()
    {
        StartPoint = transform.localPosition;
    }
    private void OnDrawGizmos()
    {
        StartPoint = transform.localPosition;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(StartPoint, EndPoint);
    }
}
