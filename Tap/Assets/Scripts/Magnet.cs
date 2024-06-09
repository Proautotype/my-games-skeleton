using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magnet : MonoBehaviour
{
    public float magneticStrength = 1.0f;
    private GameObject selected;
    public GameObject magnetAnchorPoint;
    public bool magneticEnabled = true;
    public float distanceAway = -0.25f;

    private void Update()
    {
        magneticEnabled = (magnetAnchorPoint.transform.childCount == 0);
    }

    private void OnTriggerStay(Collider other)
    {
        if (magneticEnabled && other.gameObject.CompareTag("BarCollectible"))
        {
            if(magnetAnchorPoint.transform.childCount  == 0 )
            {
                selected = other.gameObject;
                Vector3 pos = Vector3.MoveTowards(selected.transform.position, magnetAnchorPoint.transform.position, distanceAway);
                selected.transform.position = pos;
                //selected.transform.rotation = Quaternion.Euler(Vector3.zero);
            }
            //if(selected == null)
            //{
            //    selected = other.gameObject;
            //    selected.gameObject.transform.SetParent(magnetAnchorPoint.transform);
            //}
        }
    }

    public void SetSelected(GameObject go)
    {
        selected = go;
    }
}
