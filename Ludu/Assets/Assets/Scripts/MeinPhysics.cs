using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts
{
    public class MeinPhysics
    {
        public Collider[] ProjectOverlap(Transform cellTransform, LayerMask layer)
        {
            Vector3 pos = new(cellTransform.position.x, cellTransform.position.y + 0.3f, cellTransform.position.z);
            Vector3 squareHeight = new(cellTransform.localScale.x - 0.1f, cellTransform.localScale.y + 1, cellTransform.localScale.z - 0.1f);

            //remove unwanted objects
            List<Collider> colliders = new();
            foreach(Collider collider in Physics.OverlapBox(pos, squareHeight / 2, Quaternion.identity, layer))
            {
                string tag = collider.tag;
                //use here to remove unwanted gameobjects that might have been overlapped
                if (tag != null && tag.ToLower().Contains("seed"))
                {
                    colliders.Add(collider);
                }
            }
            return Physics.OverlapBox(pos, squareHeight / 2, Quaternion.identity, layer);
        }

        public Collider[] SideKickOverlap(Transform transform, LayerMask layer, string coordinate)
        {
            float x = 0;
            switch (StringToIntConverter(coordinate[1].ToString()))
            {
                case 0:
                    x = transform.position.x + 1f;
                    break;
                case 2:
                    x = transform.position.x - 1f;
                    break;
            }
            Vector3 pos = new(x, transform.position.y + 1.0f, transform.position.z);
            Vector3 squareHeight = new(transform.localScale.x + 4, 2f, transform.localScale.z - 0.1f);

            return Physics.OverlapBox(pos, squareHeight / 2, Quaternion.identity, layer);
        }

        public GameObject WhatIsBelow(Transform anchorTransform,LayerMask expected)
        {
            RaycastHit hit;
            if (Physics.Raycast(anchorTransform.position, Vector3.down,out hit ,Mathf.Infinity, expected))
            {
                return hit.transform.gameObject;
            }
            return null;
        }

        public RaycastHit[] HowManyBelow(Vector3 anchorPostion, LayerMask expected)
        {
            return Physics.RaycastAll(anchorPostion, Vector3.down, Mathf.Infinity, expected);
        }

        public int StringToIntConverter(String str)
        {
            int i;
            bool success = int.TryParse(str, out i);
            return success ? i : -1;
        }



    }

   
}
