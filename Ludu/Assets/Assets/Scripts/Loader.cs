using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    [SerializeField] private RectTransform loaderBodyRect;
    [SerializeField] private bool startCountingUp;
    private RectTransform loaderBg;

    private float largestSize;

    // Event to notify when loading percentage changes
    public event Action<float> OnLoadingPercentageChanged;

    // Event to notify when loading is complete
    public event Action OnLoaderFullyLoaded;
    
    // Event to notify when loading is complete
    public event Action OnLoaderFullyUnloaded;

    private void Start()
    {
        loaderBg = GetComponent<RectTransform>();
        if (loaderBodyRect != null)
        {
            largestSize = loaderBodyRect.rect.width;
            InitializeLoader();
        }
        else
        {
            Debug.LogError("loaderBodyRect is not assigned in the inspector.");
        }
    }

    // Method to initialize loader based on start mode
    private void InitializeLoader()
    {
        if (startCountingUp)
        {
            loaderBodyRect.sizeDelta = new Vector2(0, loaderBodyRect.sizeDelta.y);
        }
        else
        {
            loaderBodyRect.sizeDelta = new Vector2(largestSize, loaderBodyRect.sizeDelta.y);
        }
        NotifyPercentageChanged();
    }

    // Method to increment the loader's progress
    public void Increment(float value)
    {
        AdjustSize(loaderBodyRect.sizeDelta.x + value);
    }

    // Method to decrement the loader's progress
    public void Decrement(float value)
    {
        AdjustSize(loaderBodyRect.sizeDelta.x - value);
    }

    // Adjust the size of the loader
    private void AdjustSize(float newSize)
    {
        if (loaderBodyRect != null)
        {
            float clampedSize = Mathf.Clamp(newSize, 0, largestSize);
            loaderBodyRect.sizeDelta = new Vector2(clampedSize, loaderBodyRect.sizeDelta.y);
            NotifyPercentageChanged();

            if (clampedSize >= largestSize)
            {
                NotifyLoaderFullyLoaded();
            }
            else if (clampedSize <= 0)
            {
                NotifyLoaderFullyUnloaded();
            }
        }
        else
        {
            Debug.LogError("loaderBodyRect is not assigned.");
        }
    }

    // Property to get the current loading percentage
    public float LoadingPercentage
    {
        get
        {
            if (loaderBodyRect == null || largestSize == 0)
            {
                return 0;
            }
            return loaderBodyRect.sizeDelta.x / largestSize * 100f;
        }
    }

    private void NotifyPercentageChanged()
    {
        if (OnLoadingPercentageChanged != null)
        {
            OnLoadingPercentageChanged(LoadingPercentage);
        }
    }


    // Notify subscribers that the loader is fully loaded
    private void NotifyLoaderFullyLoaded()
    {
        OnLoaderFullyLoaded?.Invoke();
    }

    // Notify subscribers that the loader is fully unloaded
    private void NotifyLoaderFullyUnloaded()
    {
        OnLoaderFullyUnloaded?.Invoke();
    }
}
