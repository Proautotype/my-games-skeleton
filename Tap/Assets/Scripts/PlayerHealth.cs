using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHealth : MonoBehaviour
{
    public GameObject HealthContainer;
    public GameObject healthCard;

    public int TotalLifeCapacity = 5;
    private int Lives = 3;

    public GameObject lightLitEffect1;
    public GameObject destroyExplosionEffect1;

    public GameObject hitPoint;

    public int hitCount = 5;

    private void Start()
    {
        SetHealthBars();
    }

    private void Update()
    {
        if(Input.GetMouseButton(3))
        {
            
            print("-* mouse roll-event *-");
        }
    }

    private void SetHealthBars()
    {
        clearBars();
        for (int i = 0; i < Lives; i++)
        {
            GameObject _healthCard = Instantiate(healthCard);
            _healthCard.transform.SetParent(HealthContainer.transform, false);
        }
    }

    internal void ReduceLife(int v)
    {
        Lives -= v;
        if (Lives <= 0)
        {
            Lives = 0;
            DoExplosion(destroyExplosionEffect1, transform, 0.5f);
            gameObject.SetActive(false);
        }
        SetHealthBars();
    }

    private void clearBars()
    {
        int childCount = HealthContainer.transform.childCount;
        for(int i = 0; i < childCount;i++)
        {
            Destroy(HealthContainer.transform.GetChild(i).gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        print("theres triggering " + other.transform.tag);
        if (other.transform.CompareTag("Bullet"))
        {
            DoExplosion(lightLitEffect1, other.transform, 5f);
            DoExplosion(lightLitEffect1, hitPoint.transform, 5f);
            Destroy(other.gameObject, 0.5f);
            if(hitCount < 0)
            {
                ReduceLife(other.gameObject.GetComponent<bullet>().livesToTake);
                hitCount = 5;
            }
            hitCount--;
        }
    }

    private void DoExplosion(GameObject Effect, Transform parent, float timer)
    {
        Destroy(Instantiate(Effect, parent.position, Quaternion.identity), timer);
    }
}
