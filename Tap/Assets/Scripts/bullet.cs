using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 0.03f;
    private Vector3 direction = Vector3.zero;
    public int livesToTake { get; set; } = 1;
    float countDownBeforeSinking = 5f;
    void Start()
    {
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        if(countDownBeforeSinking < 0)
        {
            GetComponent<Rigidbody>().useGravity = true;
        }
    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    

}
