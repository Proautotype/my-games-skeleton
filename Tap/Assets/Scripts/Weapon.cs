using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject player;
    public GameObject body;
    public GameObject projectile;
    public GameObject GunPoint;

    //weapon instructions
    public float fireRateTimer = 5.0f;
    public float fireRate = 5.0f;
    public LayerMask playerMask;
    public float fireSpeed = 0.05f;
    public float weaponRadius = 30f;
    public GameObject activeEffect;
    private bool activated = false;
    private GameObject holdingActiveEffect;
    private void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, weaponRadius, LayerMask.GetMask("Player"));
        //print("size " + colliders.Length);
        if (colliders.Length > 0 && colliders[0].gameObject.CompareTag("Player"))
        {
            //target the player
            body.transform.LookAt(player.transform.position);
            //shooot
            Fire();
            //let user know that weapon is activated
            activated = true;
        }
        else
        {
            activated = false;
        }
        ShowActivation(activated);
    }

    void ShowActivation(bool turnOn) {
        activeEffect.SetActive(turnOn);
    }

    void Fire()
    {
        if(fireRate <= 1)
        {
            GameObject bullet = Instantiate(projectile, GunPoint.transform.position, GunPoint.transform.rotation);
            bullet.GetComponent<Rigidbody>().velocity = GunPoint.transform.up * fireSpeed;
            fireRate = fireRateTimer;
        }
        else
        {

            fireRate -= Time.deltaTime;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //body.transform.LookAt(player.transform.position);
        //Fire();
        //fireRate -= Time.deltaTime;
    }
}
