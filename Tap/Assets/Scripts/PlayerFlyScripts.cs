using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerFlyScripts : MonoBehaviour
{
    public CharacterController controller;

    public float forwardSpeed = 2f;
    public float VerticalSpeed = 1f;
    public float rotateSpeed = 20f;

    public float turnSmoothTime = 0.1f;

    //The amount of fuel this copter can take
    public float FuelCapacity = 200f;
    //current fuel 
    public float FuelLeft = 0f;
    //how much fuel is consumed every second
    public float FuelConsumption = 0.5f;

    public bool consumeFuel = false;
    public bool fuelFinished = false;
    //for fuel fill 
    public RectTransform FillRect;

    public bool isGrounded = true;


    //for rator controls
    [SerializeField]
    private GameObject rator;

    [SerializeField]
    private GameObject tailRator;
    public float MaxSpeed = 750;
    public float startSpeed = 0;
    public bool clockwise = true;

    public float HoverTimeLimit = 0;
    //end of rator controls

    public Vector3 GizmosPosition;
    public float GizmosRadius = 20;
    public float GizmosVertical = 0.44f;

    private void Start()
    {
        FuelLeft = FuelCapacity;
        HoverTimeLimit = FuelCapacity + 50;
        consumeFuel = true;
        StartCoroutine(RotateRator(rator));
        StartCoroutine(RotateRator(tailRator));

        StartCoroutine(ConsumeFuel());
    }


    IEnumerator ConsumeFuel()
    {
        yield return new WaitForSeconds(1f);
        while (consumeFuel)
        {
            FuelLeft -= FuelConsumption + Time.deltaTime;    
            if(FuelLeft <= 0f)
            {
                FuelLeft = 0f;
                if (FuelLeft == 0f)
                {
                    transform.GetComponent<Rigidbody>().useGravity = true;
                    fuelFinished = true;
                    ControlRator(false);
                }
            }
            TimeLeft(FuelLeft);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public IEnumerator FillFuel()
    {
        yield return new WaitForSeconds(1.5f);
        FuelLeft += FuelConsumption + Time.deltaTime;
        if (FuelLeft > FuelCapacity) {
            FuelLeft = FuelCapacity;
        }
        if (FuelLeft >= (FuelCapacity / 2))
        {
            transform.GetComponent<Rigidbody>().useGravity = false;
            fuelFinished = false;
            //start the rator
            ControlRator(true);
            
        }
        TimeLeft(FuelLeft);
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hitInfo = Tools.CheckBelowObject(transform, "Heliport");
        isGrounded = hitInfo.transform != null;


        if (!fuelFinished && startSpeed > (MaxSpeed / 1.5))
        {
            if(HoverTimeLimit > 0 && (transform.GetComponent<Rigidbody>().velocity.magnitude <= 0))
            {
                HoverTimeLimit -= Time.deltaTime;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                HoverTimeLimit += Time.deltaTime;
                if(HoverTimeLimit > FuelCapacity + 50)
                {
                    HoverTimeLimit = FuelCapacity + 50;
                }
            }
            ///end hover behavior

            if (Input.GetKey(KeyCode.A) && !isGrounded)
            {
                transform.Rotate(-Vector3.up.normalized * rotateSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.D) && !isGrounded)
            {
                transform.Rotate(Vector3.up.normalized * rotateSpeed * Time.deltaTime);
            }
            //
            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.Z))
            {
                float v = Mathf.Clamp((Vector3.up.normalized).y, 0.05f, 4f);
                transform.Translate(new Vector3(0, v, 0) * VerticalSpeed * Time.deltaTime);
            }
            if ((Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.C)) && !isGrounded)
            {
                transform.Translate(-Vector3.up.normalized * VerticalSpeed * Time.deltaTime);
            }
            //
            if (Input.GetKey(KeyCode.W) && !isGrounded)
            {
                transform.Translate(Vector3.forward.normalized * forwardSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.S) && !isGrounded)
            {
                transform.Translate(Vector3.back.normalized * forwardSpeed * Time.deltaTime);
            }
        }    

    }

    IEnumerator RotateRator(GameObject blade)
    {
        float count = 0;
        while (true)
        {
            if (clockwise)
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

            yield return new WaitForSeconds(1 / 10);
        }


    }

    void TimeLeft(float fuelLeft)
    {
        float totalTime = FuelCapacity;
        float _width = 3.25f;
        // Get the Image component of the fill rect
        if (FillRect != null) // Ensure the RectTransform is valid
        {
            float fillAmount = (fuelLeft / FuelCapacity) * _width; // Calculate the fill amount
            FillRect.sizeDelta = new Vector2(fillAmount, FillRect.sizeDelta.y); // Set the new height
        }
        else
        {
            Debug.LogWarning("RectTransform not found in one of the TimePanels game objects.");
        }
    }

    public void ControlRator(bool start)
    {
        clockwise = start;
    }

    private void OnDrawGizmos()
    {
        GizmosPosition = new Vector3(transform.position.x - 0.1f, transform.position.y - 0.3f, transform.position.z + 0.1f);
        Gizmos.color = Color.white;
        //Gizmos.DrawSphere(GizmosPosition, GizmosRadius);
    }

    //void CheckBelowObject(String maskName)
    //{
    //    RaycastHit hitInfo;
    //   bool hit = Physics.Raycast(transform.position, Vector3.down, out hitInfo, 1.5f, LayerMask.GetMask(maskName) );
    //    isGrounded = hit;
    //}
}
