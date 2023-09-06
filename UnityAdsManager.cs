using System;
using System.Collections;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Events;

public class UnityAdsManager : Singleton<UnityAdsManager>, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public bool IsInitialized { get; set; } = false;

    [SerializeField] OnAdShown OnAdShownEvent;

    [SerializeField] string _androidGameId;
    [SerializeField] string _iOSGameId;
    [SerializeField] bool _testMode = true;
    private string _gameId;


    [SerializeField] string _androidInterstitialID = "Interstitial_Android";
    [SerializeField] string _iOsInterstitialID = "Interstitial_iOS";
    string _adUnitIdIntersitial;


    [SerializeField] string _androidAdUnitIdRewarded = "Rewarded_Android";
    [SerializeField] string _iOSAdUnitIdRewarded = "Rewarded_iOS";
    string _adUnitIdRewarded = null; // This will remain null for unsupported platforms
    private UnityAction rewardedCallback;


    [SerializeField] BannerPosition _bannerPosition = BannerPosition.BOTTOM_CENTER;

    [SerializeField] string _androidAdUnitId = "Banner_Android";
    [SerializeField] string _iOSAdUnitId = "Banner_iOS";
    string _adUnitId = null; // This will remain null for unsupported platforms.


    protected override void Awake()
    {
        base.Awake();

    }

    private async void Start()
    {
        try
        {
            var options = new InitializationOptions()
                .SetEnvironmentName("production");

            await UnityServices.InitializeAsync(options);
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
        }
    }

    public void InitializeAds()
    {
#if UNITY_IOS
            _gameId = _iOSGameId;
#elif UNITY_ANDROID
        _gameId = _androidGameId;
#elif UNITY_EDITOR
            _gameId = _androidGameId; //Only for testing the functionality in the Editor
#endif
        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(_gameId, _testMode, this);
        }

        // Get the Ad Unit ID for the current platform:
        _adUnitIdIntersitial = (Application.platform == RuntimePlatform.IPhonePlayer)
            ? _iOsInterstitialID
            : _androidInterstitialID;

        // Get the Ad Unit ID for the current platform:
#if UNITY_IOS
        _adUnitId = _iOSAdUnitId;
#elif UNITY_ANDROID
        _adUnitIdRewarded = _androidAdUnitIdRewarded;
#endif



    }

    // Implement a method to call when the Load Banner button is clicked:
    public void LoadBanner()
    {
        // Set up options to notify the SDK of load events:
        BannerLoadOptions options = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };

        // Load the Ad Unit with banner content:
        //Advertisement.Load(_adUnitId);
        Advertisement.Banner.SetPosition(_bannerPosition);
        Advertisement.Banner.Load(_androidAdUnitId, options);
    }

    void OnBannerLoaded()
    {
        Debug.Log("Banner loaded");
    }

    // Implement code to execute when the load errorCallback event triggers:
    void OnBannerError(string message)
    {
        Debug.Log($"Banner Error: {message}");
        // Optionally execute additional code, such as attempting to load another ad.
    }

    // Implement a method to call when the Show Banner button is clicked:
    public void ShowBannerAd()
    {
        // Set up options to notify the SDK of show events:
        BannerOptions options = new BannerOptions
        {
            clickCallback = OnBannerClicked,
            hideCallback = OnBannerHidden,
            showCallback = OnBannerShown
        };

        Debug.Log($"Banner Show Call || Banner State:{Advertisement.Banner.isLoaded}");
        // Show the loaded Banner Ad Unit:
        if (Advertisement.Banner.isLoaded)
        {
            Advertisement.Banner.Show(_androidAdUnitId, options);
        }
    }

    // Implement a method to call when the Hide Banner button is clicked:
    public void HideBannerAd()
    {
        // Hide the banner:
        Advertisement.Banner.Hide();
    }

    void OnBannerClicked() { }
    void OnBannerShown() { }
    void OnBannerHidden() { }



    // Call this public method when you want to get an ad ready to show.
    public void LoadAdRewarded()
    {
        // IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
        Debug.Log("Loading Ad: " + _adUnitIdRewarded);
        Advertisement.Load(_adUnitIdRewarded, this);
    }


    // Implement a method to execute when the user clicks the button:
    public void ShowAdRewarded(UnityAction rewardedCallback)
    {
        this.rewardedCallback = rewardedCallback;
        // Then show the ad:
        Advertisement.Show(_adUnitIdRewarded, this);
    }


    // Load content to the Ad Unit:
    public void LoadAdInterstitial()
    {
        // IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
        Debug.Log("Loading Ad: " + _adUnitIdIntersitial);
        Advertisement.Load(_adUnitIdIntersitial, this);
    }

    // Show the loaded content in the Ad Unit:
    public void ShowAdInterstitial()
    {
        // Note that if the ad content wasn't previously loaded, this method will fail
        Debug.Log("Showing Ad: " + _adUnitIdIntersitial);
        Advertisement.Show(_adUnitIdIntersitial, this);
    }

    // Implement Load Listener and Show Listener interface methods: 
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        // Optionally execute code if the Ad Unit successfully loads content.
    }

    public void OnUnityAdsFailedToLoad(string _adUnitId, UnityAdsLoadError error, string message)
    {
        if (_adUnitIdRewarded.Equals(_adUnitId))
            rewardedCallback = null;

        Debug.Log($"Error loading Ad Unit: {_adUnitId} - {error.ToString()} - {message}");
        // Optionally execute code if the Ad Unit fails to load, such as attempting to try again.
    }

    public void OnUnityAdsShowFailure(string _adUnitId, UnityAdsShowError error, string message)
    {
        if (_adUnitIdRewarded.Equals(_adUnitId))
            rewardedCallback = null;

        Debug.Log($"Error showing Ad Unit {_adUnitId}: {error.ToString()} - {message}");
        // Optionally execute code if the Ad Unit fails to show, such as loading another ad.

        if (_adUnitIdRewarded.Equals(_adUnitId))
        {
            Debug.Log("Unity Ads Rewarded Ad Completed");
            // Grant a reward.
            rewardedCallback?.Invoke();
            LoadAdRewarded();
        }

        if (_adUnitIdIntersitial.Equals(_adUnitId))
        {
            LoadAdInterstitial();
        }
        else
        {
            LoadAdRewarded();
        }
        DispatchOnShowEvent(false);
        AdLoadingPanel.Instance.CloseAdPanel();
    }

    public void OnUnityAdsShowStart(string _adUnitId)
    {
        DispatchOnShowEvent(false);
        AdLoadingPanel.Instance.ShowIconAndText(false);
        AdLoadingPanel.Instance.CloseAdPanel();
    }
    public void OnUnityAdsShowClick(string _adUnitId) { }
    IEnumerator DispatchOnShowEvent(bool _show, float waitTime = 0)
    {
        if (waitTime == 0)
            yield return new WaitForEndOfFrame();
        else
            yield return new WaitForSeconds(waitTime);
        AdLoadingPanel.Instance.ShowIconAndText(_show);
        OnAdShownEvent?.InvokeEvent(_show);
    }

    public void DispatchOnShowEvent(bool _show)
    {
        Debug.Log("DispatchOnShowEvent(): Dispatching -" + _show);
        AdLoadingPanel.Instance.ShowIconAndText(_show);
        OnAdShownEvent?.InvokeEvent(_show);
    }

    public void OnUnityAdsShowComplete(string _adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (_adUnitIdRewarded.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Unity Ads Rewarded Ad Completed");
            // Grant a reward.

            rewardedCallback?.Invoke();
            rewardedCallback = null;
            LoadAdRewarded();
        }

        if (_adUnitIdIntersitial.Equals(_adUnitId))
        {
            LoadAdInterstitial();
        }
        else
        {
            LoadAdRewarded();
        }

        UnityMainThreadDispatcher.Instance().Enqueue(DispatchOnShowEvent(false, 0));
        AdLoadingPanel.Instance.ShowIconAndText(false);
        AdLoadingPanel.Instance.CloseAdPanel();

    }

    public void OnInitializationComplete()
    {
        IsInitialized = true;
        Debug.Log("Unity Ads initialization complete.");
        LoadAdInterstitial();
        LoadAdRewarded();
        LoadBanner();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }
}