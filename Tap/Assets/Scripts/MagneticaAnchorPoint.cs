using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class MagneticaAnchorPoint : MonoBehaviour
{
    // Start is called before the first frame update
    public int capacity = 1;
    public Magnet magnet;
    public bool doAttract = true;
    public float itemPositionOffset = 0.1f;
    private void Update()
    {
        RaycastHit raycastHit = Tools.CheckBelowObject(transform,"BarCollectible");
        if (raycastHit.transform && doAttract)
        {
            GameObject other = raycastHit.transform.gameObject;
            Vector3 pos = Vector3.MoveTowards(raycastHit.transform.position, transform.position, itemPositionOffset);
            raycastHit.transform.position = pos;

            float distance = Vector3.Distance(transform.position, other.transform.position);

            if(distance <= 0.3f )
            {
                doAttract = false;
                other.transform.SetParent(transform);
            }
        }
    }
    
}
