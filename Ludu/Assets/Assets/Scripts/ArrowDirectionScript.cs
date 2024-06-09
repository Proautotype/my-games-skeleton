using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowDirectionScript : MonoBehaviour
{
    // Start is called before the first frame update
    private Quaternion stableRotation;
    public Vector3 newRotation;
    private RectTransform rectTransform;
    private bool oldDirection = true;

    private void Awake()
    {
        //GameManager.gmInstance.directionChangeEvent.AddListener(ChangePosition);
    }
    private void Start()
    {
        stableRotation = transform.localRotation;
        GameManager.gmInstance.directionChangeEvent.AddListener(ChangePosition);
    }

    public void ChangePosition()
    {
        if (oldDirection)
        {
            GetComponent<SpriteRenderer>().flipY = true;
            oldDirection = false;
        }
        else
        {
            GetComponent<SpriteRenderer>().flipY = false;
            oldDirection = true;
        }
    }
}
