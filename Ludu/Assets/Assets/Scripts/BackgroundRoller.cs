using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundRoller : MonoBehaviour
{
    // Start is called before the first frame update
    public float scrollSpeed = 2.0f;
    private RectTransform panelRect;
    private Vector2 initialPosition;

    private void Start()
    {
        panelRect = GetComponentInChildren<RectTransform>();
        initialPosition = panelRect.anchoredPosition;
    }

    private void Update()
    {
        float offset = Time.time * scrollSpeed;
        Vector2 newPosition = initialPosition + Vector2.up * offset;
        panelRect.anchoredPosition = newPosition;
    }
}
