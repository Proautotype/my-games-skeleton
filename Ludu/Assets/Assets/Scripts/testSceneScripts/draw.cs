using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class draw : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isNegative = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.T))
        {
            show();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Vector3 pos = new(transform.position.x + (!isNegative ? 2.4f : -2.4f), transform.position.y + 1f, transform.position.z);
        Vector3 squareHeight = new(transform.localScale.x + 0.2f, 2f, 0.1f);
        Gizmos.DrawWireCube(pos, squareHeight);
    }

    void show()
    {
        // The position of the wire cube's center
        Vector3 pos = new(transform.position.x + (!isNegative ? 2.4f : -2.4f) , transform.position.y + 1f, transform.position.z);

        // The size of the wire cube (dimensions)
        Vector3 squareHeight = new(transform.localScale.x + 0.2f, 2f, 0.1f);

        // Create an overlap box to detect colliders within the box's bounds
        Collider[] colliders = Physics.OverlapBox(pos, squareHeight / 2f);

        foreach (Collider collider in colliders)
        {
            // Handle the collider or do something with the information
            Debug.Log("Overlapped with: " + collider.gameObject.name);
            collider.gameObject.GetComponent<MeshRenderer>().material.color = GetRandomColor();
        }
    }

    private Color GetRandomColor()
    {
        // Generate random RGB values between 0 and 1
        float r = Random.Range(0f, 1f);
        float g = Random.Range(0f, 1f);
        float b = Random.Range(0f, 1f);

        // Create a new Color object with the random RGB values
        return new Color(r, g, b);
    }
}
