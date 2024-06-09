using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class HelperUtils 
    {
        public int StringToIntConverter(String str)
        {
            int i;
            bool success = int.TryParse(str, out i);
            return success ? i : -1;
        }

        public bool AreAllTrue(List<bool> boolList)
        {
            // Check if all elements in the boolList are true
            return boolList.All(b => b);
        }

        public void ClearGameObject(GameObject parent)
        {
            int childCount = parent.transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(parent.transform.GetChild(i).gameObject);
            }
        }
    }

}