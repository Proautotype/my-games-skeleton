using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class Tools : MonoBehaviour
    {

        // Use this for initialization
        public static RaycastHit CheckBelowObject(Transform transform,String maskName, float magnetReach = 1.5f)
        {
            RaycastHit hitInfo;
            RaycastHit GetInFo()
            {
                return hitInfo;
            }
            bool hit = Physics.Raycast(transform.position, Vector3.down, out hitInfo, magnetReach, LayerMask.GetMask(maskName));
            return hitInfo;
        }
    }
}