using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class CustomSceneBuilder : MonoBehaviour
    {
        private GameObject cell;
        [SerializeField] private GameObject holder;
        void Start()
        {
            //cell = GameObject.Find("Cells");
            //activating flash to highlight active player seeds
            //StartCoroutine(buildHolders());
        }

       public IEnumerator buildHolders()
        {
            for(int i = 0; i < cell.transform.childCount; i++)
            {
                Transform child = cell.transform.GetChild(i);
                GameObject childInstance = Instantiate(holder, child.transform.position, child.transform.rotation);
                childInstance.transform.SetParent(child);
                childInstance.transform.position
                  = new Vector3(
                          child.transform.position.x,
                          child.transform.position.y + .2f,
                          child.transform.position.z
                          );
                //Destroy(childInstance, 50);
                yield return new WaitForSeconds(0.1f);
            }
        }

    }
}