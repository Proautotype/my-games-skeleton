using System;
using System.Collections;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ChangePositionEvent: UnityEvent<Vector3> { }

namespace Assets.Scripts
{
    public class SeedScript : MonoBehaviour
    {
        private GameObject patherGO;
        //[HideInInspector]
        public bool active { get; set; } = false;
        public void Start()
        {
            if(GetComponent<Rigidbody>() == null)
            {
                transform.AddComponent<Rigidbody>();
            }
        }

        public void OnMouseDown()
        {
            //if hinting mode is off
            print("hello " + active);
            if (active)
            {
                if (!GameManager.gmInstance.doHint)
                {
                    StartCoroutine(GameManager.gmInstance.SetAndMoveActiveSeed(gameObject));
                    //GameManager.gmInstance.SeedToMoveByFirstAccumulatedDice(gameObject);
                    GetComponent<Rigidbody>().useGravity = true;
                }
                else
                {
                    TestAtGoal(GameManager.gmInstance.goClockWise);
                }
            }
        }


        void TestAtGoal(bool goClockWise)
        {
            print("TestAtGoal");
            GameObject pather = HintManager.instance.pather;
            GameObject tracer = HintManager.instance.tracer;
            if(pather  != null && tracer != null)
            {
                patherGO = Instantiate(pather, transform.position, Quaternion.identity);
                Vector3 oldPosition = transform.position;
                oldPosition.y = 0.8f;
                patherGO.transform.position = oldPosition;
                //lineRenderer.transform.SetParent(transform, true);
                if (goClockWise)
                {
                    StartCoroutine(GameManager.gmInstance.SimpleForwardTrace(null, patherGO, tracer));
                }
                else
                {
                    StartCoroutine(GameManager.gmInstance.SimpleBackwardTrace(null, patherGO, tracer));
                }
            }
            else
            {
                print("tracer " + tracer + "pather " + pather);
            }
        }

        public void TestTForward()
        {
            GameObject pather = HintManager.instance.pather;
            GameObject tracer = HintManager.instance.tracer;
            if (patherGO == null)
            {
                patherGO = Instantiate(pather, transform.position, Quaternion.identity);
            }
            Vector3 oldPosition = transform.position;
            oldPosition.y = 0.8f;
            patherGO.transform.position = oldPosition;
            //lineRenderer.transform.SetParent(transform, true);
            StartCoroutine(GameManager.gmInstance.SimpleForwardTrace(null, patherGO, tracer));
        }

        public void TestTBackward()
        {
            GameObject pather = HintManager.instance.pather;
            GameObject tracer = HintManager.instance.tracer;
            if (patherGO == null)
            {
                patherGO = Instantiate(pather, transform.position, Quaternion.identity);
            }
            Vector3 oldPosition = transform.position;
            oldPosition.y = 0.8f;
            patherGO.transform.position = oldPosition;
            //lineRenderer.transform.SetParent(transform, true);
            StartCoroutine(GameManager.gmInstance.SimpleBackwardTrace(null, patherGO, tracer));
        }

    }
}
