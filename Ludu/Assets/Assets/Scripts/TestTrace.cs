using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTrace : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private GameObject lineRendererGO;
    [SerializeField]
    private GameObject pather;
    [SerializeField]
    private GameObject tracer;
    public List<int> traceNumber;
    private GameObject patherGO;
    public bool direction = false;
    void Start()
    {
        
    }

    private void OnMouseDown()
    {
        //direction ? TestTForward() : TestTBackward();
        if(!direction)
        {
            TestTForward();
        }
        else
        {
            TestTBackward();
        }
    }

    public void TestTForward()
    {;
        if(patherGO == null)
        {
             patherGO = Instantiate(pather, transform.position, Quaternion.identity);
        }
        Vector3 oldPosition = transform.position;
        oldPosition.y = 0.8f;
        patherGO.transform.position = oldPosition;
        //lineRenderer.transform.SetParent(transform, true);
        StartCoroutine(GameManager.gmInstance.SimpleForwardTrace(traceNumber, patherGO, tracer));
    }

    public void TestTBackward()
    {
        ;
        if (patherGO == null)
        {
            patherGO = Instantiate(pather, transform.position, Quaternion.identity);
        }
        Vector3 oldPosition = transform.position;
        oldPosition.y = 0.8f;
        patherGO.transform.position = oldPosition;
        //lineRenderer.transform.SetParent(transform, true);
        StartCoroutine(GameManager.gmInstance.SimpleBackwardTrace(traceNumber, patherGO, tracer));
    }
}
