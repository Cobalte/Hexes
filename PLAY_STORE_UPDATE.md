# Sushi Neko Swipe — Google Play Relaunch Plan

Tracking the work to update Sushi Neko Swipe (`com.CloakQuillGames.SushiNekoSwipe`), bring it back into Play policy compliance, optimize it for mobile, and republish.

Policy dates below are from Google's published schedule as of late 2026 — confirm current requirements in Play Console before submitting.

## Starting point (assessed 2026-11-02)

- Unity 2022.3.60f1, IL2CPP, ARMv7 + ARM64, custom Gradle templates.
- Gameplay is ~2,200 lines of C# in a single scene (`Assets/Scenes/SushiNekoSwipe.unity`); 2D UI-based, no real performance concerns.
- `minSdk 34`, `targetSdk 34`, version 1.0.3 (code 31).
- Release keystore: `E:\Latticework\Cloak & Quill\Games\Sushi Neko Swipe\Sushi Neko Swipe Builds\sushinekoswipe.keystore`.
- Third-party SDKs: Kwalee publisher SDK 9.1.2 (custom), Facebook SDK 17, leftover pieces of Google Mobile Ads (plugin folder deleted, AAR and Gradle entries remain). Ads code is commented out.
- Repo state: ~200 uncommitted changes from previous work (deleted `Assets/GoogleMobileAds/`, `.utmp` native build cache, script changes). The last commit's `ProjectSettings.asset` contains Unity 6 fields while the working copy is 2022.3 — the project was opened in a newer Unity at some point.

## 1. Repo housekeeping

- [x] Fix git "dubious ownership" error: `git config --global --add safe.directory "E:/Latticework/Unity Projects/Hexes"`
- [x] Review and commit (or discard) the pre-existing uncommitted changes
- [x] Stop ignoring `Assets/Plugins/` in `.gitignore` so the Android manifest and Gradle templates are versioned
- [x] Ignore `.utmp/` (native build cache) and `UserSettings/` and remove them from the index
- [x] Back up the release keystore somewhere off this machine
- [x] Confirm in Play Console whether the app is enrolled in Play App Signing — yes ("App signing key in use"); Google holds the app signing key
- [x] Confirm the local keystore is the registered **upload** key (SHA-1 matches): compare its SHA-1 (`keytool -list -v -keystore sushinekoswipe.keystore`) with "Upload key certificate" on Play Console's App signing page

## 2. Remove the Kwalee SDK

- [x] Delete `Assets/Kwalee/` (backed up outside the project, untracked by git)
- [x] Remove the Kwalee `Bootstrap` GameObject from `SushiNekoSwipe.unity`
- [x] Remove `KWCore.Umbrella.GameCore.ProgressManager` telemetry calls from `GameController.cs` and `UnlockProgressBar.cs`
- [x] Remove `KWALEE_DEBUG` scripting define (Android, iOS)
- [x] Open the project in Unity and confirm it compiles with no errors and the scene has no missing scripts (verified in 6.3 on 2026-11-02)
- [x] ~~Force Resolve to drop Kwalee's Android dependencies~~ — moot: the old Gradle templates were deleted (see section 3)

## 3. Remove other unused SDKs

- [x] Remove the Facebook SDK (`Assets/FacebookSDK/`, ~18 MB) — not referenced by game code (backed up outside the project, untracked by git)
- [x] Remove Meta leftovers in `Assets/StreamingAssets/` (`meta-logo.png`, `meta.mp4`; only used by Facebook example scenes) and the now-empty folder
- [x] Remove `Assets/Editor/DisableBitcode.cs` (Facebook iOS build helper)
- [x] Remove stale AdMob leftovers before installing the fresh plugin: `googlemobileads-unity.aar`, `GoogleMobileAdsPlugin.androidlib` in `Assets/Plugins/Android/`, and `GADUAdNetworkExtras.h` / `NativeTemplates/` in `Assets/Plugins/iOS/`
- [x] Delete the old 2022-era custom Android files (`mainTemplate.gradle`, `settingsTemplate.gradle`, `gradleTemplate.properties`, `proguard-user.txt`, `AndroidManifest.xml`) — Unity 6.3 now uses its defaults. When installing AdMob, tick **Custom Main Gradle Template** and **Custom Gradle Properties Template** in Publishing Settings so Unity generates fresh 6.3 versions
- [x] ~~Force Resolve to drop Facebook's Android dependencies~~ — moot: templates deleted
- Keep the External Dependency Manager — the Google Mobile Ads plugin depends on it (the plugin ships a newer copy)

## 4. Monetization: AdMob

- [x] Decide: ads via AdMob
- [x] Install the current Google Mobile Ads Unity plugin — v11.6.0 via OpenUPM (`com.google.ads.mobile` in `Packages/manifest.json`, scoped registry `https://package.openupm.com`, scope `com.google`); brings `com.unity.external-dependency-manager` 2.1.0. Old `Assets/ExternalDependencyManager` (1.2.179) deleted. Compiles cleanly in 6.3
- [x] Enter the AdMob Android app ID (`ca-app-pub-…~…`) in **Assets → Google Mobile Ads → Settings**
- [x] Tick **Custom Main Gradle Template** and **Custom Gradle Properties Template** (Player Settings → Publishing Settings), then **Assets → External Dependency Manager → Android Resolver → Force Resolve** (needs Android as the active platform in File → Build Profiles). Resolved: `play-services-ads` 25.5.0, `user-messaging-platform` 4.0.0, AndroidX constraintlayout/fragment/lifecycle-process; AndroidX + Jetifier enabled
- [x] Turn on "Resolve on build" in the Android Resolver settings (auto-resolution on file change stays off)
- [x] Implement UMP consent (required for EEA/UK) before initializing ads — `PremiumController` refreshes consent every launch, shows the form if required, and only then starts `MobileAds`
- [x] Restore and modernize banner code in `PremiumController.cs` — fixed 320x50 banner at the top; development builds use Google's test unit, release builds the real one; `CanvasHelper.SetTopInset` pushes the UI down by the banner's height
- [x] Child safety in code: all ad requests capped at `MaxAdContentRating.G`. Not tagged child-directed (the game doesn't target under-13s)
- [x] Add a **Privacy Options** button: `PrivacyOptionsButtonAnchor`, a long flat button under the Privacy Policy / sound row in the Language (settings) panel, styled like the Privacy Policy button, labels in all 8 languages using only glyphs already in the static font atlases (Chinese "隐私与广告" and Korean "광고 개인 정보" mean "privacy & ads" / "ad privacy"). Shown only when UMP says it's required; the settings panel then stretches by one row (`OptionsMenuController.PrivacyRowHeight` / `SettingsGrowUpward`) and the Credits and New Game panels move down to make room
- [ ] Test the Privacy Options button: tick **Test Consent As EEA** on `PremiumController` (Game Controller object), play, open the options menu. On a real phone also add the hashed device ID from logcat. Untick before release (it's ignored in release builds anyway). Note: in the editor, Google's stand-in consent SDK always reports privacy options as required, so the editor shows the button only when the test checkbox is ticked. The real show/hide decision can only be checked on a phone
- [ ] Play-test ads: test banner appears at top without covering the score/settings; consent form shows when testing with an EEA debug geography
- [x] AdMob console: set the app's maximum ad content rating to **G**
- [ ] AdMob console: block sensitive ad categories (Blocking controls → Sensitive categories), e.g. gambling, dating, alcohol
- [x] "Go Premium" button stays hidden: its whole panel (`IAPAnchor`) is inactive in the scene, so it never shows regardless of `UpdatePremiumText`
- [ ] After the first AdMob build, check the merged manifest's permissions. The game itself needs none; the last release (v1.0.9, 2024) had only AdMob's set: `INTERNET`, `ACCESS_NETWORK_STATE`, `com.google.android.gms.permission.AD_ID`, `ACCESS_ADSERVICES_AD_ID` / `_ATTRIBUTION` / `_TOPICS`, `WAKE_LOCK`, `FOREGROUND_SERVICE` (via WorkManager). Anything beyond that list should be investigated
- [ ] Set up `app-ads.txt` on the developer website listed in Play Console
- [x] Decide whether to offer a "remove ads" IAP — not for this release (would need Unity IAP / Play Billing Library)

## 5. Engine and Android target upgrade

- [x] Upgrade to Unity 6.3 LTS (now on 6000.3.27f2; compiles and plays in editor; TextMesh Pro merged into uGUI automatically) — supported to Dec 2027; needed for API 36 target; also fixes CVE-2025-59489, which affects 2022.3.60f1. Not 6.0 (support ended Oct 2026) or 6.5/7.x (non-LTS; 6.5 deprecates the Built-In Render Pipeline this project uses)
- [x] Fix obsolete `EditorGUILayout.ObjectField` overload in `LocalizedFontInspector.cs` and `LocalizedTextInspector.cs` (warning CS0618)
- [x] Set `targetSdk` to 36 (Android 16) — required for updates since Aug 31, 2026
- [x] Lower `minSdk` from 34 to 25 (Android 7.1 — the lowest Unity 6.3 supports)
- [x] Application entry point: Activity (already set); custom `AndroidManifest.xml` removed
- [x] Consider dropping ARMv7 (ARM64 only) — decided to keep ARMv7 + ARM64: no extra cost to players (Play serves each device only its own architecture) and it keeps 32-bit-only low-end phones supported. Revisit if an SDK drops 32-bit support
- [x] Remove unused engine modules and packages (AI, cloth, terrain, umbra, Unity Analytics, vehicles, video, VR, wind, XR, XR Management + `Assets/XR/`, Multiplayer Center)
- [x] Bump version: **1.1.0, code 32** (Play Console highest was 1.0.2 / code 31)
- [x] Turn off the Unity splash screen ("Made with Unity")
- [x] Adaptive app icon (stand-in): `Assets/Art/Icons/AppIconAdaptiveBackground.png` is the original flat icon art at 92% of the visible area, bottom-aligned so the sushi runs off the edge, on #2AB5C5 teal; the foreground layer is transparent. Set for all 6 Android adaptive sizes. Replace with real separate background/foreground layers from the artist if they turn up

## 6. Mobile optimization

- [x] Set `Application.targetFrameRate = 60` in `GameController.Start` (mobile defaults to 30)
- [x] Replace per-frame LINQ in `IsAnyBlockMoving` with a plain loop (`blocks.RemoveAll` is allocation-free and LINQ in `ExecuteSwipe` runs once per swipe — left as is)
- [x] Review texture import settings on large images (`BG_Texture.png`, `SplashScreen.png`) — already compressed sprites, no mipmaps, 2048 max; sprite atlases optional
- [x] Remove per-frame garbage: `Block.Update` (LINQ closure every frame per block), `Celebration.Update` (LINQ `All`), `ScoreDisplay` (could stall a hair below the score and rebuild its string every frame forever — now snaps and only updates when the number changes)
- [x] Stop UI Animators dirtying canvases every frame: new `AnimatorSleeper` switches one-shot animators off once their clip finishes (blocks, hungry nekos, menus, loading screen, camera/safe-area shake); code wakes them before playing. Buttons are left alone (Unity's button code needs their animators enabled); looping tutorial arrows never sleep. Unlock progress slider also stops nudging once it's within 0.1% of its target
- [x] Pool blocks and celebration effects (new `PrefabPool`) instead of Instantiate/Destroy; `Block` gets `GameController`/`UI Canvas` passed in instead of `GameObject.Find`. Blocks reset their animated pose (`Block.ResetPose`) before going back in the pool — without this, a block pooled mid-bounce came back with its sushi permanently off-center. Combo badges (`ScoreMultiplierPanel`) are **not** pooled: some of their particle systems destroy themselves when finished, so reused copies would be missing effects
- [x] Fix: red combo background disc (`ScoreMultiplierGroup/BG`, `Combo_BG.png`) peeked out from behind the x2–x16 badges — `ScoreMultiplierPanel` now shows it only at x1. The badges were also off-center from the meter's round end cap (`Meter_BG.png`): badge `Anchor` nudge (-2, 3) zeroed in all 5 combo prefabs, and `ScoreMultiplierGroup` moved from (5, 2) to (4.25, 0.25) so it's centered on the cap (measured from the sprite: center 114.5, 114.5 px at 0.5 units/px)
- [ ] Play-test the above in the editor and on a device: block spawn/merge/arrival animations, neko hungry/full, options + results menus open/close, camera shake, results screen board copy, a second game after game over
- [ ] (Later update, not this release) Replace per-turn `BinaryFormatter` save with `JsonUtility` (keep reading the old binary save once so existing players keep their games)
- [x] Battery: render at 30fps when idle (`OnDemandRendering.renderFrameInterval = 2` after 3 s with no touches or moving blocks; tutorials always full speed). Input is still read at 60fps
- [ ] Profile on a low-end device (memory, frame time, startup time)
- [ ] Verify safe-area handling on modern aspect ratios and edge-to-edge display (enforced when targeting API 35+)

## 7. Build and verify

- [x] Open the project in Unity and confirm it still compiles and plays after the module removal and Android settings changes
- [ ] Build an Android App Bundle (AAB)
- [ ] Verify 16 KB page size compliance for all native libraries (APK Analyzer / Play Console)
- [ ] Test on physical devices (low-end and current Android version)
- [ ] Upload to internal testing track and review the pre-launch report

## 8. Play Console compliance

- [ ] Confirm developer account status and identity verification are current
- [x] Update Data safety form (saved in Play Console) to match the remaining SDKs and data collection. The form covers SDK data too; the game's own save files stay on the device, so they're not "collected". Answers, based on Google's disclosure for Mobile Ads SDK 25.5.0 (re-check it if the SDK is updated):
  - Collects or shares required user data: **Yes**. All data encrypted in transit: **Yes** (TLS). Way to request deletion: **No** (no accounts; users reset their ad ID in Android settings)
  - **Location → Approximate location** (estimated from IP address)
  - **App activity → App interactions** (app launches, taps, video views)
  - **App info and performance → Diagnostics** (launch time, hang rate, energy use)
  - **Device or other IDs** (advertising ID, app set ID)
  - For each: **Collected: yes, Shared: yes**, not processed ephemerally, collection required, purposes **Advertising or marketing**, **Analytics**, **Fraud prevention, security, and compliance**
- [x] Draft updated privacy policy: [Docs/PrivacyPolicy.md](Docs/PrivacyPolicy.md) — covers AdMob and what it collects (sent to Google, not to Cloak & Quill), consent + Privacy options, children (G-rated ads only), contact
- [ ] Fill in the contact email in `Docs/PrivacyPolicy.md`, then publish it at https://www.cloakquillgames.com/privacy-policy (the URL the in-game Privacy Policy button and Play Console use)
- [ ] Ads declaration: **contains ads — yes**
- [ ] Re-do content rating questionnaire (IARC)
- [ ] Target audience and content settings: pick age groups 13+ only (the game isn't designed for under-13s), and answer the "could the app unintentionally appeal to children?" question honestly — Google reviews this, and if it decides the app appeals to kids it may require the Families policy (child-directed ad treatment)
- [ ] Refresh store listing: screenshots, feature graphic, description
- [ ] Release to production (staged rollout)
