using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//using UnityEngine.UIElements;
using UnityEngine.UI;

public class CubeReshaping : MonoBehaviour
{
    // Start is called before the first frame update
    public float waitTime = 10.0f;
    public float minY = 0;
    public float maxY = 0;
    public bool activated = false;
    public bool drawGizmo = false;
    public float gizmoVerticalPosition = 0.8f;
    public float NoticeRadius = 5f;
    //public List<GameObject> TimeFillRects; // 0242705749

    public RectTransform[] TimePanels;
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 position = transform.position;
        position.y = gizmoVerticalPosition;
        if (drawGizmo)
        {
            Gizmos.DrawSphere(position, NoticeRadius);
        }
    }
    private void Update()
    {
        ToggleTimerHUD(false);
        if (activated)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, NoticeRadius);
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    ToggleTimerHUD(true);
                }
            }
        }
    }

    public IEnumerator Equalize()
    {
        float count = waitTime;
        while (activated)
        {
            if (count == 0)
            {
                Vector3 localTransform = transform.position;
                transform.position = new(localTransform.x, Random.Range(minY, maxY), localTransform.z);
                count = waitTime;
                TimeLeft(count);
            }
            else {
                TimeLeft(count);
            }
            count--;
            yield return new WaitForSeconds(1);
        }
    }

    public void ToggleTimerHUD(bool visible)
    {
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).CompareTag("Canvas")) {
                transform.GetChild(i).gameObject.SetActive(visible);
            }
        }
    }

    void TimeLeft(float timeLeft) {
        float totalTime = 10f;
        float _height = 3f;
        foreach (RectTransform panelRect in TimePanels)
        {
            // Get the Image component of the fill rect
            if (panelRect != null) // Ensure the RectTransform is valid
            {
                float fillAmount = (timeLeft / totalTime) * _height; // Calculate the fill amount
                 // Calculate the new height based on fill amount
                panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, fillAmount); // Set the new height
            }
            else
            {
                Debug.LogWarning("RectTransform not found in one of the TimePanels game objects.");
            }
        }
    }

}
