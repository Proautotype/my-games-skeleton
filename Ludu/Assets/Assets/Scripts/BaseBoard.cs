using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBoard : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag.ToLower().Contains("seed"))
        {
            Vector3 pos = collision.transform.position;
            pos.y = 2.5f;
            collision.transform.position = pos;
        }
    }
}
