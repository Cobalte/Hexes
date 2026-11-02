using System;
using System.IO;
//using GoogleMobileAds.Api;
using UnityEditor;
using UnityEngine;

public class PremiumController : MonoBehaviour {

    // banner info:                https://developers.google.com/admob/unity/banner
    // info on test ads:           https://developers.google.com/admob/unity/test-ads#enable_test_devices

    public bool IsGamePremium { get; private set; }
    
    //private BannerView bannerView;
    //private bool isAdRunning;
    
    // test ad id:                 ca-app-pub-3940256099942544/6300978111
    // new test ad id?:            ca-app-pub-3940256099942544/9214589741
    // actual sushi neko ad id:    ca-app-pub-3968697709394788/6119658708
    //private const string adUnitId = "ca-app-pub-3968697709394788/6119658708";
    
    //--------------------------------------------------------------------------------------------------------
    private void Awake() {
    }
}
