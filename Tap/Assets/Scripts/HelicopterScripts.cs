using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopterScripts : MonoBehaviour
{
    [SerializeField]
    private GameObject rator;
    
    [SerializeField]
    private GameObject tailRator;

    public float MaxSpeed = 750;
    public float startSpeed = 0;

    public bool clockwise = true;


    private void Start()
    {
        StartCoroutine(RotateRator(rator));
        StartCoroutine(RotateRator(tailRator));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {

        }
    }

    IEnumerator RotateRator(GameObject blade) {
        float count = 0;
        while (true)
        {
            if(clockwise)
            {
                startSpeed += count;
                if (startSpeed >= MaxSpeed)
                {
                    startSpeed = MaxSpeed;
                }
            }
            else
            {
                startSpeed -= count;
                if (startSpeed < 0)
                {
                    startSpeed = 0;
                }
            }
            if (blade != null)
            {
                // Get the current rotation of the rator
                Quaternion rotation = blade.transform.rotation;

                // Modify the rotation angle based on time and speed
                rotation *= Quaternion.Euler(0f, 0f, Time.deltaTime * startSpeed);

                // Apply the new rotation
                blade.transform.rotation = rotation;
            }
            if (clockwise)
            {
                count += 0.005f;
            }
            else
            {
                count -= 0.001f;
            }
            
            yield return new WaitForSeconds(1/10);
        }


    }

    public void ControlRator(bool start)
    {
        clockwise = start;
    }
}
