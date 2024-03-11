using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

public class PremiumController : MonoBehaviour {

    // banner info:                https://developers.google.com/admob/unity/banner
    // info on test ads:           https://developers.google.com/admob/unity/test-ads#enable_test_devices
    // test ad id:                 ca-app-pub-3940256099942544/6300978111
    // actual sushi neko ad id:    ca-app-pub-3968697709394788/6119658708

    public bool IsGamePremium { get; private set; }
    
    private BannerView bannerView;
    private bool isAdRunning;
    
    private const string adUnitId = "ca-app-pub-3968697709394788/6119658708";
    
    //--------------------------------------------------------------------------------------------------------
    private void Awake() {
        return; // disabled until we get premium payments working
        
        VerifyPremiumStatus();

        if (!IsGamePremium) {
            SetAdBannerVisible(true);
        }
    }

    //--------------------------------------------------------------------------------------------------------
    private void VerifyPremiumStatus() {
        return;
        
        Debug.Log("Premium status: " + IsGamePremium);

        if (IsGamePremium && isAdRunning) {
            SetAdBannerVisible(false);
        }
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void GoPremium() {
        return;
        
        Debug.Log("Going premium!");
        IsGamePremium = true;
        VerifyPremiumStatus();
    }

    //--------------------------------------------------------------------------------------------------------
    private void SetAdBannerVisible(bool isVisible) {
        return;
        
        if (isVisible) {
            MobileAds.Initialize(initStatus => { });
            bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top); // default size: 320x50
            AdRequest request = new AdRequest.Builder().Build();
            bannerView.LoadAd(request);
        }
        else {
            bannerView.Destroy();
        }
        
        isAdRunning = isVisible;
    }
}
