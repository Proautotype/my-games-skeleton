using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TurnsCubeReshaper : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> Cubes;
    public int cube2Start = 3;

    [SerializeField]
    private GameObject CirclePass;
    [SerializeField]
    private Color selectColor;

    void Start()
    {
        StartCoroutine(ShapingOchestrator());
        
    }

    // Update is called once per frame
    IEnumerator ShapingOchestrator() {
        yield return new WaitForSeconds(1);
        List<int> selectedList = new();
        if (true && Cubes.Count > cube2Start)
        {
            while (selectedList.Count < cube2Start)
            {
                int selected = Random.Range(0, Cubes.Count);
                if (selectedList.IndexOf(selected) < 0)
                {
                    selectedList.Add(selected);
                }
            }
            foreach (int i in selectedList)
            {
                GameObject child = Cubes[i];    
                CubeReshaping cubeReshaping = child.GetComponent<CubeReshaping>();
                cubeReshaping.maxY = 4;
                cubeReshaping .minY = -2;
                cubeReshaping.activated = true;
                child.GetComponent<MeshRenderer>().material.color = selectColor;
                StartCoroutine(cubeReshaping.Equalize());
                GameObject instantiatedCirclePass = Instantiate(CirclePass, child.transform);
                Vector3 instantiatedPosition = instantiatedCirclePass.transform.position;
                instantiatedPosition.y = (child.transform.localScale.y / 2) + 0.01f;
                instantiatedPosition.x = child.transform.position.x;
                instantiatedPosition.z = child.transform.position.z;
                instantiatedCirclePass.transform.position = instantiatedPosition;
                instantiatedCirclePass.transform.parent = child.transform;
            }

            yield return new WaitForSeconds(2);
        }
    }
}
