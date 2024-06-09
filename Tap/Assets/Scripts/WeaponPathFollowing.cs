using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPathFollowing : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 5f;

    private int currentWaypointIndex = 0;

    void Update()
    {
        if (currentWaypointIndex < waypoints.Length)
        {
            // Move towards the current waypoint
            transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex].position, speed * Time.deltaTime);

            // If the weapon reaches the current waypoint, move to the next one
            if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < 0.1f)
            {
                currentWaypointIndex++;
            }
        }
        else
        {
            // Reset the waypoint index to loop the path
            currentWaypointIndex = 0;
        }
    }
}
