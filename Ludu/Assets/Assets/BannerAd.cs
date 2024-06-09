using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class BannerAd : MonoBehaviour
{
    [SerializeField] string _androidGameId;
    [SerializeField] string _iOSGameId;
    [SerializeField]
    BannerPosition _bannerPosition = new BannerPosition();

    private string _gameId;

    private void Awake()
    {
        #if UNITY_IOS
            _gameId = _iOSGameId;
        #elif UNITY_ANDROID
           _gameId = _androidGameId;
        #elif UNITY_EDITOR
            _gameId = _androidGameId;
#endif

        Advertisement.Banner.SetPosition(_bannerPosition);
    }

    private void Start()
    {
        StartCoroutine(LoadBanner());
    }

    IEnumerator LoadBanner()
    {
        //yield return new WaitForSeconds(1);
        // Set up options to notify the SDK of load events:
        BannerLoadOptions options = new BannerLoadOptions()
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };
        // Load the add Unit with banner content
        Advertisement.Banner.Load(_gameId, options);
        yield return new WaitForSeconds(3);
        // Show the loaded Banner Ad Unit:
        Advertisement.Banner.Show(_gameId, null);
    }

    void OnBannerLoaded() {
        Debug.Log("Banner loaded");
    }
    
    void OnBannerError(string message) {
        Debug.Log($"Banner Error: {message}");
    }
}
