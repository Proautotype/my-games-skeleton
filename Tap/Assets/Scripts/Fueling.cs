using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fueling : MonoBehaviour
{
    public bool isBusy;
    public float waitTime = 200f;
    private float counter = 0f;
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject player = other.gameObject;

            if ((player.GetComponent<Rigidbody>().velocity.x == 0f && player.GetComponent<Rigidbody>().velocity.y ==0 && player.GetComponent<Rigidbody>().velocity.z == 0))
            {
                counter += 1;
                if (counter > waitTime)
                {
                    isBusy = true;
                    PlayerFlyScripts playerFlyScript = player.GetComponent<PlayerFlyScripts>();
                    if (playerFlyScript != null)
                    {
                        StartCoroutine(playerFlyScript.FillFuel());
                    }
                }
            }
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        print("Stop fuelling");
        counter = 0;
        StartCoroutine(MakeFuellingReady());
    }

    IEnumerator MakeFuellingReady()
    {
        yield return new WaitForSeconds(2);
        isBusy = false;
    }

}
