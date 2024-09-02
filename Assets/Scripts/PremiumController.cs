using System;
using System.IO;
using GoogleMobileAds.Api;
using UnityEditor;
using UnityEngine;

public class PremiumController : MonoBehaviour {

    // banner info:                https://developers.google.com/admob/unity/banner
    // info on test ads:           https://developers.google.com/admob/unity/test-ads#enable_test_devices

    public bool IsGamePremium { get; private set; }
    
    private BannerView bannerView;
    //private bool isAdRunning;
    
    // test ad id:                 ca-app-pub-3940256099942544/6300978111
    // new test ad id?:            ca-app-pub-3940256099942544/9214589741
    // actual sushi neko ad id:    ca-app-pub-3968697709394788/6119658708
    private const string adUnitId = "ca-app-pub-3968697709394788/6119658708";
    
    //--------------------------------------------------------------------------------------------------------
    private void Awake() {
        if (bannerView != null) {
            bannerView.Destroy();
            bannerView = null;
        }
        
        try {
            MobileAds.Initialize(initStatus => { });
            
            bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top); // default size: 320x50
            AdRequest request = new AdRequest();
            bannerView.LoadAd(request);
            Debug.Log("Ads initialized.");
            
            ListenToAdEvents();
        }
        catch (Exception exception) {
            StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/debug-log.txt", true);
            writer.WriteLine("Error encountered:");
            writer.WriteLine(exception);
            writer.Close();
            Debug.LogError("Error encountered while trying to initialize ads: " + exception);
        }
    }

    //--------------------------------------------------------------------------------------------------------
    private void OnAdsInitialized() {
        bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top); // default size: 320x50
        //AdRequest request = new AdRequest.Builder().Build(); // this is from an older google ads sdk
        AdRequest request = new AdRequest();
        bannerView.LoadAd(request);
        Debug.Log("Ads initialized.");
    }
    
    //--------------------------------------------------------------------------------------------------------
    private void ListenToAdEvents()
    {
        // Raised when an ad is loaded into the banner view.
        bannerView.OnBannerAdLoaded += () => {
            Debug.Log("Banner view loaded an ad with response : "
                + bannerView.GetResponseInfo());
        };
        
        // Raised when an ad fails to load into the banner view.
        bannerView.OnBannerAdLoadFailed += (LoadAdError error) => {
            Debug.LogError("Banner view failed to load an ad with error : " + error);
        };
        
        // Raised when the ad is estimated to have earned money.
        bannerView.OnAdPaid += (AdValue adValue) => {
            Debug.Log(String.Format("Banner view paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        
        // Raised when an impression is recorded for an ad.
        bannerView.OnAdImpressionRecorded += () => {
            Debug.Log("Banner view recorded an impression.");
        };
        
        // Raised when a click is recorded for an ad.
        bannerView.OnAdClicked += () => {
            Debug.Log("Banner view was clicked.");
        };
        
        // Raised when an ad opened full screen content.
        bannerView.OnAdFullScreenContentOpened += () => {
            Debug.Log("Banner view full screen content opened.");
        };
        
        // Raised when the ad closed full screen content.
        bannerView.OnAdFullScreenContentClosed += () => {
            Debug.Log("Banner view full screen content closed.");
        };
    }
}
