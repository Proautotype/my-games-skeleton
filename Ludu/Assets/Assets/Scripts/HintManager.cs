using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    public int numberOfHints = 10;
    public GameObject tracer;
    public GameObject pather;

    public static HintManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void DoHint()
    {
        List<GameObject> seeds = GameManager.gmInstance.AllSeedsOnBoard();
        List<int> accumulatedDices = GameManager.gmInstance.accumulatedDices;

        GameObject pathGo = Instantiate(pather, transform.position, Quaternion.identity);
        Vector3 oldPosition = seeds[0].transform.position;
        oldPosition.y = 0.8f;
        pathGo.transform.position = oldPosition;
        print("size " + accumulatedDices.Count);
        StartCoroutine(GameManager.gmInstance.SimpleForwardTrace(accumulatedDices, pathGo, tracer));

        //foreach (GameObject seed in seeds)
        //{
        //    GameObject th = Instantiate(pather, transform.position, Quaternion.identity);
        //    Vector3 oldPosition = seed.transform.position;
        //    oldPosition.y = 0.8f;
        //    th.transform.position = oldPosition;
        //    StartCoroutine(GameManager.gmInstance.SimpleForwardTrace(accumulatedDices.ToArray(), th, tracer));
        //    //StartCoroutine(GameManager.gmInstance.SimpleBackwardTrace(accumulatedDices.ToArray(), seed, tracer));
        //}
    }
}

