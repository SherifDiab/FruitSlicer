using System;
using UnityEngine;

#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
#endif

/// <summary>
/// Manages AdMob advertisements for the Fruit Slicer game.
/// Supports Banner, Interstitial, and Rewarded ads.
///
/// IMPORTANT: You must install the Google Mobile Ads Unity plugin for this to work.
/// Download from: https://github.com/googleads/googleads-mobile-unity/releases
/// After importing, the GOOGLE_MOBILE_ADS scripting define symbol will be automatically added.
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
    [SerializeField] private BannerAdPosition bannerPosition = BannerAdPosition.Bottom;

    /// <summary>
    /// Banner position enum (mirrors Google's AdPosition when SDK is not installed)
    /// </summary>
    public enum BannerAdPosition
    {
        Top = 0,
        Bottom = 1,
        TopLeft = 2,
        TopRight = 3,
        BottomLeft = 4,
        BottomRight = 5,
        Center = 6
    }

#if GOOGLE_MOBILE_ADS
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
#if GOOGLE_MOBILE_ADS
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
        Debug.LogWarning("Google Mobile Ads SDK is not installed. Please import the SDK from: https://github.com/googleads/googleads-mobile-unity/releases");
#endif
    }

#if GOOGLE_MOBILE_ADS
    /// <summary>
    /// Converts our BannerAdPosition enum to Google's AdPosition
    /// </summary>
    private AdPosition ConvertBannerPosition(BannerAdPosition position)
    {
        return (AdPosition)(int)position;
    }

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
        bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, ConvertBannerPosition(bannerPosition));

        // Register for ad events
        bannerView.OnBannerAdLoaded += HandleBannerAdLoaded;
        bannerView.OnBannerAdLoadFailed += HandleBannerAdFailedToLoad;

        // Create an ad request
        AdRequest request = new AdRequest();

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

    private void HandleBannerAdLoaded()
    {
        Debug.Log("Banner ad loaded successfully");
    }

    private void HandleBannerAdFailedToLoad(LoadAdError error)
    {
        Debug.LogError($"Banner ad failed to load: {error.GetMessage()}");
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
        AdRequest request = new AdRequest();

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
        AdRequest request = new AdRequest();

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

#else
    // Stub methods when SDK is not installed
    public void RequestBannerAd() { Debug.LogWarning("AdMob SDK not installed"); }
    public void ShowBannerAd() { Debug.LogWarning("AdMob SDK not installed"); }
    public void HideBannerAd() { Debug.LogWarning("AdMob SDK not installed"); }
    public void RequestInterstitialAd() { Debug.LogWarning("AdMob SDK not installed"); }
    public void ShowInterstitialAd() { Debug.LogWarning("AdMob SDK not installed"); }
    public bool IsInterstitialReady() { return false; }
    public void RequestRewardedAd() { Debug.LogWarning("AdMob SDK not installed"); }
    public void ShowRewardedAd() { OnRewardedAdFailed?.Invoke(); }
    public bool IsRewardedAdReady() { return false; }
#endif

    private void OnDestroy()
    {
#if GOOGLE_MOBILE_ADS
        // Clean up ad resources
        bannerView?.Destroy();
        interstitialAd?.Destroy();
        rewardedAd = null;
#endif
    }
}
