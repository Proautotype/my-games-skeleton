using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    Camera mainCam;

    public float minX = -5f;
    public float maxX = 5f;

    public float minZoom = 10;
    public float maxZoom = 20f;

    public float zoomSpeed = 1.5f;
    private float initialOrthographicSize;
    private float initialDistance;

    //for vertical camera movement
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private bool isCameraTouching;

    private float touchTimeElapse = 3;
    private float touchTimeElapseCounter;
    private bool cameraMovementPermission = false; 
    public float moveSpeed = 0.1f;
    public float edgeThickness = 20f;

    private void Start()
    {
        mainCam = Camera.main;
        initialOrthographicSize = mainCam.orthographicSize;
    }

    private void Update()
    {
        if(Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);
            //check for movement on the pinched position
            if(touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                initialDistance = Vector2.Distance(touch0.position, touch1.position);
                initialOrthographicSize = mainCam.orthographicSize;
            }else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                float currentDistance = Vector2.Distance(touch0.position,touch1.position);
                float deltaDistance = currentDistance - initialDistance;

                float zoomDelta = -deltaDistance * zoomSpeed * Time.deltaTime;
                float newOrthographicSize = Mathf.Clamp(initialOrthographicSize + zoomDelta,minZoom, maxZoom);

                mainCam.orthographicSize = newOrthographicSize;
            }
        }

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPos = touch.position;

            // Check if touch position is within the defined edge thickness
            if (touchPos.y < edgeThickness)
            {
                MoveCamera(Vector3.back); // Move camera backwards on Z axis
            }
            else if (touchPos.y > Screen.height - edgeThickness)
            {
                MoveCamera(Vector3.forward); // Move camera forwards on Z axis
            }

            if (touchPos.x < edgeThickness)
            {
                MoveCamera(Vector3.left); // Move camera left on X axis
            }
            else if (touchPos.x > Screen.width - edgeThickness)
            {
                MoveCamera(Vector3.right); // Move camera right on X axis
            }
        }
    }

    void MoveCamera(Vector3 direction)
    {
        float moveX = direction.x * moveSpeed * Time.deltaTime;
        float moveZ = direction.z * moveSpeed * Time.deltaTime;

        // Apply movement to the camera
        transform.Translate(new Vector3(moveX, 0, moveZ));
    }

    public void HorizontalMovement(float value)
    {
        // Get the current position of the camera
        Vector3 currentPosition = mainCam.transform.position;

        // Update the x-coordinate based on the input value
        currentPosition.x = value;

        // Set the camera's position to the updated position, preserving the y-coordinate
        mainCam.transform.position = currentPosition;
    }

    public void ZoomMovement(float delta)
    {
        mainCam.orthographicSize = Mathf.Clamp(delta, minZoom, maxZoom);
    }
}

