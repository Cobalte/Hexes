using System.Collections.Generic;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

public class PremiumController : MonoBehaviour {

    // banner info:                https://developers.google.com/admob/unity/banner
    // consent (UMP) info:         https://developers.google.com/admob/unity/privacy
    // info on test ads:           https://developers.google.com/admob/unity/test-ads

    public bool IsGamePremium { get; private set; }

    [Header("Testing only - ignored in release builds")]
    [Tooltip("Treat this device as being in the EEA so the consent form and Privacy Options button show up.")]
    public bool TestConsentAsEea;
    [Tooltip("On a real phone, also add its hashed device ID (printed in logcat by the UMP SDK).")]
    public List<string> ConsentTestDeviceIds = new List<string>();

    // players covered by privacy rules (e.g. in the EEA/UK) must be able to change their consent later
    public bool IsPrivacyOptionsRequired {
        get {
#if UNITY_EDITOR
            // the editor's stand-in consent sdk always reports "required", so preview with the test checkbox
            return TestConsentAsEea;
#else
            return ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
#endif
        }
    }

    private BannerView bannerView;
    private bool areAdsStarted;

    // development builds always use google's sample banner so we never click our own real ads
    private const string testAdUnitId = "ca-app-pub-3940256099942544/6300978111";
    private const string realAdUnitId = "ca-app-pub-3968697709394788/6119658708";
    private static string AdUnitId => Debug.isDebugBuild ? testAdUnitId : realAdUnitId;

    //--------------------------------------------------------------------------------------------------------
    private void Start() {
        if (IsGamePremium) {
            return;
        }

        // note: ad sdk callbacks can arrive off the main thread, so anything touching unity objects goes
        // through MobileAdsEventExecutor.ExecuteInUpdate
        MobileAds.SetRequestConfiguration(new RequestConfiguration {
            // plenty of kids play this, so only allow ads that are suitable for all ages
            MaxAdContentRating = MaxAdContentRating.G
        });

        // consent from an earlier session lets us start ads right away
        if (ConsentInformation.CanRequestAds()) {
            StartAds();
        }

        // consent still has to be refreshed every launch - this shows the consent form if the player needs it
        ConsentInformation.Update(CreateConsentRequest(), OnConsentInfoUpdated);
    }

    //--------------------------------------------------------------------------------------------------------
    private ConsentRequestParameters CreateConsentRequest() {
        ConsentRequestParameters request = new ConsentRequestParameters();

        if (Debug.isDebugBuild && TestConsentAsEea) {
            // start from a clean slate each launch so the consent form shows every time while testing
            ConsentInformation.Reset();
            request.ConsentDebugSettings = new ConsentDebugSettings {
                DebugGeography = DebugGeography.EEA,
                TestDeviceHashedIds = ConsentTestDeviceIds
            };
        }

        return request;
    }

    //--------------------------------------------------------------------------------------------------------
    private void OnDestroy() {
        if (bannerView != null) {
            bannerView.Destroy();
            bannerView = null;
        }
    }

    //--------------------------------------------------------------------------------------------------------
    public void ShowPrivacyOptions() {
        ConsentForm.ShowPrivacyOptionsForm(error => {
            if (error != null) {
                Debug.LogError("Privacy options form failed: " + error.Message);
            }

            MobileAdsEventExecutor.ExecuteInUpdate(StartAdsIfAllowed);
        });
    }

    //--------------------------------------------------------------------------------------------------------
    private void OnConsentInfoUpdated(FormError updateError) {
        if (updateError != null) {
            // carry on with whatever consent we had from before
            Debug.LogError("Consent info update failed: " + updateError.Message);
            MobileAdsEventExecutor.ExecuteInUpdate(StartAdsIfAllowed);
            return;
        }

        ConsentForm.LoadAndShowConsentFormIfRequired(formError => {
            if (formError != null) {
                Debug.LogError("Consent form failed: " + formError.Message);
            }

            MobileAdsEventExecutor.ExecuteInUpdate(StartAdsIfAllowed);
        });
    }

    //--------------------------------------------------------------------------------------------------------
    private void StartAdsIfAllowed() {
        if (ConsentInformation.CanRequestAds()) {
            StartAds();
        }
    }

    //--------------------------------------------------------------------------------------------------------
    private void StartAds() {
        if (areAdsStarted) {
            return;
        }
        areAdsStarted = true;

        MobileAds.Initialize(status => {
            MobileAdsEventExecutor.ExecuteInUpdate(() => {
                if (status == null) {
                    Debug.LogError("Google Mobile Ads failed to initialize.");
                    areAdsStarted = false;
                    return;
                }

                ShowBanner();
            });
        });
    }

    //--------------------------------------------------------------------------------------------------------
    private void ShowBanner() {
        bannerView = new BannerView(AdUnitId, AdSize.Banner, AdPosition.Top); // default size: 320x50

        // push the game's ui down so the banner never covers it
        CanvasHelper.SetTopInset(bannerView.GetHeightInPixels());

        bannerView.OnBannerAdLoaded += () => {
            MobileAdsEventExecutor.ExecuteInUpdate(() => {
                if (bannerView != null) {
                    CanvasHelper.SetTopInset(bannerView.GetHeightInPixels());
                }
            });
        };
        bannerView.OnBannerAdLoadFailed += error => {
            Debug.LogError("Banner failed to load: " + error);
        };

        bannerView.LoadAd(new AdRequest());
    }
}
