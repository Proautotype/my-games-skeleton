using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectDice : MonoBehaviour
{
    public Transform diceSpawnPoint;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("dice"))
        {
            collision.gameObject.GetComponent<Rigidbody>().useGravity = true;
            collision.transform.position = diceSpawnPoint.position;
        }
    }
}
