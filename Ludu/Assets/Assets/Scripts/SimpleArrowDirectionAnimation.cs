using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SimpleArrowDirectionAnimation : MonoBehaviour
{
    [HideInInspector]
    public enum Direction {UP,DOWN,LEFT,RIGHT}
    public Direction direction = Direction.UP;
    public float refreshRate = 0.5f;
    public float distance = 0.05f;
    private void Start()
    {
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        yield return new WaitForSeconds(1);
        bool forward = false;
        while (true)
        {
            switch (direction)
            {
                case Direction.UP:
                    Vector3 oldPosition = transform.position;
                    if (forward)
                    {
                        oldPosition.z -= distance;
                        transform.position = oldPosition;
                        forward = false;
                    }
                    else
                    {
                        oldPosition.z += distance;
                        transform.position = oldPosition;
                        forward = true;
                    }
                    break; 
                case Direction.DOWN:
                    Vector3 oldPositionD = transform.position;
                    if (forward)
                    {
                        oldPositionD.z += distance;
                        transform.position = oldPositionD;
                        forward = false;
                    }
                    else
                    {
                        oldPositionD.z -= distance;
                        transform.position = oldPositionD;
                        forward = true;
                    }
                    break; 
                case Direction.LEFT:
                    Vector3 oldPositionL = transform.position;
                    if (forward)
                    {
                        oldPositionL.x -= distance;
                        transform.position = oldPositionL;
                        forward = false;
                    }
                    else
                    {
                        oldPositionL.x += distance;
                        transform.position = oldPositionL;
                        forward = true;
                    }
                    break;
                case Direction.RIGHT:
                    Vector3 oldPositionR = transform.position;
                    if (forward)
                    {
                        oldPositionR.x -= distance;
                        transform.position = oldPositionR;
                        forward = false;
                    }
                    else
                    {
                        oldPositionR.x += distance;
                        transform.position = oldPositionR;
                        forward = true;
                    }
                    break;
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
