using System;
using UnityEngine;

#if UNITY_ANDROID
using GoogleMobileAds.Api;
#endif

/// <summary>
/// Manages AdMob advertisements for the Fruit Slicer game.
/// Supports Banner, Interstitial, and Rewarded ads.
/// </summary>
public class AdMobManager : MonoBehaviour
{
    public static AdMobManager Instance { get; private set; }

    [Header("Ad Unit IDs (Replace with your own for production)")]
    [SerializeField] private string bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111"; // Test ID
    [SerializeField] private string interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712"; // Test ID
    [SerializeField] private string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917"; // Test ID

    [Header("Settings")]
    [SerializeField] private bool showBannerOnStart = true;
    [SerializeField] private AdPosition bannerPosition = AdPosition.Bottom;

#if UNITY_ANDROID
    private BannerView bannerView;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;
#endif

    // Events for game integration
    public event Action OnRewardedAdCompleted;
    public event Action OnRewardedAdFailed;
    public event Action OnInterstitialClosed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
#if UNITY_ANDROID
        // Initialize the Google Mobile Ads SDK
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("AdMob SDK initialized successfully");

            // Load ads after initialization
            if (showBannerOnStart)
            {
                RequestBannerAd();
            }
            RequestInterstitialAd();
            RequestRewardedAd();
        });
#else
        Debug.Log("AdMob is only supported on Android in this build");
#endif
    }

#if UNITY_ANDROID
    #region Banner Ads

    /// <summary>
    /// Requests and displays a banner ad
    /// </summary>
    public void RequestBannerAd()
    {
        // Clean up existing banner
        if (bannerView != null)
        {
            bannerView.Destroy();
        }

        // Create a banner view
        bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, bannerPosition);

        // Register for ad events
        bannerView.OnAdLoaded += HandleBannerAdLoaded;
        bannerView.OnAdFailedToLoad += HandleBannerAdFailedToLoad;

        // Create an ad request
        AdRequest request = new AdRequest.Builder().Build();

        // Load the banner ad
        bannerView.LoadAd(request);
    }

    /// <summary>
    /// Shows the banner ad if loaded
    /// </summary>
    public void ShowBannerAd()
    {
        if (bannerView != null)
        {
            bannerView.Show();
        }
    }

    /// <summary>
    /// Hides the banner ad
    /// </summary>
    public void HideBannerAd()
    {
        if (bannerView != null)
        {
            bannerView.Hide();
        }
    }

    private void HandleBannerAdLoaded(object sender, EventArgs args)
    {
        Debug.Log("Banner ad loaded successfully");
    }

    private void HandleBannerAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.LogError($"Banner ad failed to load: {args.LoadAdError.GetMessage()}");
    }

    #endregion

    #region Interstitial Ads

    /// <summary>
    /// Requests an interstitial ad
    /// </summary>
    public void RequestInterstitialAd()
    {
        // Clean up existing interstitial
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
        }

        // Create an ad request
        AdRequest request = new AdRequest.Builder().Build();

        // Load the interstitial ad
        InterstitialAd.Load(interstitialAdUnitId, request, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError($"Interstitial ad failed to load: {error?.GetMessage()}");
                return;
            }

            Debug.Log("Interstitial ad loaded successfully");
            interstitialAd = ad;

            // Register for ad events
            interstitialAd.OnAdFullScreenContentClosed += HandleInterstitialClosed;
            interstitialAd.OnAdFullScreenContentFailed += HandleInterstitialFailed;
        });
    }

    /// <summary>
    /// Shows the interstitial ad if ready
    /// </summary>
    public void ShowInterstitialAd()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.Show();
        }
        else
        {
            Debug.Log("Interstitial ad is not ready yet");
            RequestInterstitialAd();
        }
    }

    /// <summary>
    /// Checks if interstitial ad is ready to show
    /// </summary>
    public bool IsInterstitialReady()
    {
        return interstitialAd != null && interstitialAd.CanShowAd();
    }

    private void HandleInterstitialClosed()
    {
        Debug.Log("Interstitial ad closed");
        OnInterstitialClosed?.Invoke();
        RequestInterstitialAd(); // Preload next ad
    }

    private void HandleInterstitialFailed(AdError error)
    {
        Debug.LogError($"Interstitial ad failed to show: {error.GetMessage()}");
        RequestInterstitialAd();
    }

    #endregion

    #region Rewarded Ads

    /// <summary>
    /// Requests a rewarded ad
    /// </summary>
    public void RequestRewardedAd()
    {
        // Create an ad request
        AdRequest request = new AdRequest.Builder().Build();

        // Load the rewarded ad
        RewardedAd.Load(rewardedAdUnitId, request, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError($"Rewarded ad failed to load: {error?.GetMessage()}");
                return;
            }

            Debug.Log("Rewarded ad loaded successfully");
            rewardedAd = ad;

            // Register for ad events
            rewardedAd.OnAdFullScreenContentClosed += HandleRewardedAdClosed;
            rewardedAd.OnAdFullScreenContentFailed += HandleRewardedAdFailed;
        });
    }

    /// <summary>
    /// Shows the rewarded ad if ready
    /// </summary>
    public void ShowRewardedAd()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"User earned reward: {reward.Amount} {reward.Type}");
                OnRewardedAdCompleted?.Invoke();
            });
        }
        else
        {
            Debug.Log("Rewarded ad is not ready yet");
            OnRewardedAdFailed?.Invoke();
            RequestRewardedAd();
        }
    }

    /// <summary>
    /// Checks if rewarded ad is ready to show
    /// </summary>
    public bool IsRewardedAdReady()
    {
        return rewardedAd != null && rewardedAd.CanShowAd();
    }

    private void HandleRewardedAdClosed()
    {
        Debug.Log("Rewarded ad closed");
        RequestRewardedAd(); // Preload next ad
    }

    private void HandleRewardedAdFailed(AdError error)
    {
        Debug.LogError($"Rewarded ad failed to show: {error.GetMessage()}");
        OnRewardedAdFailed?.Invoke();
        RequestRewardedAd();
    }

    #endregion
#endif

    private void OnDestroy()
    {
#if UNITY_ANDROID
        // Clean up ad resources
        bannerView?.Destroy();
        interstitialAd?.Destroy();
        rewardedAd = null;
#endif
    }
}
