using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bubbleScript : MonoBehaviour
{
    public float maxEnlargement = 30.0f;  // Maximum scale factor
    public float enlargementSpeed = 0.05f;  // Rate of enlargement
    public float enlargementInterval = 1.0f;  // Time between enlargements
    private float enlargementTimer = 0.0f;
    private float currentScale = 1.0f;
    private Vector3 initialScale;
    private Vector3 targetScale;

    void Start()
    {
        initialScale = transform.localScale;
        CalculateNextEnlargement();
    }

    void Update()
    {
        if (currentScale < maxEnlargement)
        {
            enlargementTimer += Time.deltaTime;
            if (enlargementTimer >= enlargementInterval)
            {
                currentScale += enlargementSpeed;
                if (currentScale > maxEnlargement)
                {
                    currentScale = maxEnlargement;
                }
                transform.localScale = Vector3.Lerp(initialScale, targetScale, currentScale);
                enlargementTimer = 0.0f;
                CalculateNextEnlargement();
            }
        }
    }

    void CalculateNextEnlargement()
    {
        // Randomize the next enlargement target
        float randomEnlargement = Random.Range(1.0f, maxEnlargement);
        targetScale = initialScale * randomEnlargement;
    }
}
