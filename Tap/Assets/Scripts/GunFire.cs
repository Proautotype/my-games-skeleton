using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFire : MonoBehaviour
{
    public GameObject projectile;
    public GameObject gunPointHolder;
    public GameObject gunPoint;
    public float fireSpeed;
    public float fireRate;
    public float fireRateTimer;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            GameObject bullet = Instantiate(projectile, gunPoint.transform.position, gunPoint.transform.rotation);
            bullet.GetComponent<Rigidbody>().velocity = gunPoint.transform.up * fireSpeed;
            fireRate = fireRateTimer;
        }

        float scrollWheelValue = Input.GetAxis("Mouse ScrollWheel");
        //print(scrollWheelValue);
        if(scrollWheelValue != 0)
        {
            Quaternion rotation = gunPointHolder.transform.localRotation;
            float xValue = rotation.x;
            print(xValue);
            rotation.y *= 0;
            rotation.z *= 0;
            rotation.x = Mathf.Clamp(rotation.x += scrollWheelValue, 1f, 1.5f);

            gunPointHolder.transform.localRotation *= rotation;
        }
    }
}
