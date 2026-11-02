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

- [ ] Fix git "dubious ownership" error: `git config --global --add safe.directory "E:/Latticework/Unity Projects/Hexes"`
- [ ] Review and commit (or discard) the pre-existing uncommitted changes
- [ ] Stop ignoring `Assets/Plugins/` in `.gitignore` so the Android manifest and Gradle templates are versioned
- [ ] Ignore `.utmp/` (native build cache) and remove it from the index
- [ ] Back up the release keystore somewhere off this machine
- [ ] Confirm in Play Console whether the app is enrolled in Play App Signing

## 2. Remove the Kwalee SDK

- [x] Delete `Assets/Kwalee/` (backed up outside the project, untracked by git)
- [x] Remove the Kwalee `Bootstrap` GameObject from `SushiNekoSwipe.unity`
- [x] Remove `KWCore.Umbrella.GameCore.ProgressManager` telemetry calls from `GameController.cs` and `UnlockProgressBar.cs`
- [x] Remove `KWALEE_DEBUG` scripting define (Android, iOS)
- [x] Open the project in Unity and confirm it compiles with no errors and the scene has no missing scripts (verified in 6.3 on 2026-11-02)
- [ ] Run the External Dependency Manager's Android Resolver (Force Resolve) so Kwalee's Android dependencies (`work-runtime-ktx`, `play-services-ads-identifier`) drop out

## 3. Remove other unused SDKs

- [x] Remove the Facebook SDK (`Assets/FacebookSDK/`, ~18 MB) — not referenced by game code (backed up outside the project, untracked by git)
- [x] Remove Meta leftovers in `Assets/StreamingAssets/` (`meta-logo.png`, `meta.mp4`; only used by Facebook example scenes) and the now-empty folder
- [x] Remove `Assets/Editor/DisableBitcode.cs` (Facebook iOS build helper)
- [ ] Remove stale AdMob leftovers before installing the fresh plugin: `googlemobileads-unity.aar`, `GoogleMobileAdsPlugin.androidlib` in `Assets/Plugins/Android/`, and `GADUAdNetworkExtras.h` / `NativeTemplates/` in `Assets/Plugins/iOS/`
- [ ] Remove the obsolete `com.android.support.constraint:constraint-layout:1.1.3` dependency from `mainTemplate.gradle` (or let Unity 6.3 regenerate the templates)
- [ ] Run Force Resolve so Facebook's Android dependencies drop out of the Gradle files
- Keep the External Dependency Manager — the Google Mobile Ads plugin depends on it (the plugin ships a newer copy)

## 4. Monetization: AdMob

- [x] Decide: ads via AdMob
- [ ] Install the current Google Mobile Ads Unity plugin (after the Unity 6.3 upgrade)
- [ ] Implement UMP consent (required for EEA/UK) before initializing ads
- [ ] Restore and modernize banner code in `PremiumController.cs` (real ad unit ID is in the comments; use test IDs during development)
- [ ] Keep the `AD_ID` permission in `AndroidManifest.xml` (or let the plugin add it)
- [ ] Set up `app-ads.txt` on the developer website listed in Play Console
- [ ] Decide whether to offer a "remove ads" IAP (would need Unity IAP / Play Billing Library)

## 5. Engine and Android target upgrade

- [x] Upgrade to Unity 6.3 LTS (now on 6000.3.27f2; compiles and plays in editor; TextMesh Pro merged into uGUI automatically) — supported to Dec 2027; needed for API 36 target; also fixes CVE-2025-59489, which affects 2022.3.60f1. Not 6.0 (support ended Oct 2026) or 6.5/7.x (non-LTS; 6.5 deprecates the Built-In Render Pipeline this project uses)
- [ ] Fix obsolete `EditorGUILayout.ObjectField` overload in `Assets/Editor/LocalizedFontInspector.cs:36` (warning CS0618; pass `allowSceneObjects`)
- [ ] Set `targetSdk` to 36 (Android 16) — required for updates since Aug 31, 2026
- [ ] Lower `minSdk` from 34 to ~24 so older devices can install
- [ ] Choose application entry point (Activity vs GameActivity) and clean up `AndroidManifest.xml` accordingly
- [ ] Consider dropping ARMv7 (ARM64 only)
- [ ] Remove unused engine modules from `Packages/manifest.json` (VR, XR, terrain, cloth, vehicles, wind, etc.)
- [ ] Bump version (e.g. 1.1.0, code 32+)

## 6. Mobile optimization

- [ ] Set `Application.targetFrameRate` (60 in-game; consider 30 on menus) — VSync is off on some quality levels
- [ ] Remove the per-frame `blocks.RemoveAll(...)` in `GameController.Update` and LINQ allocations in `ExecuteSwipe`
- [ ] Review texture import settings on large images (`BG_Texture.png`, `SplashScreen.png`) and use sprite atlases
- [ ] Profile on a low-end device (memory, frame time, startup time)
- [ ] Verify safe-area handling on modern aspect ratios and edge-to-edge display (enforced when targeting API 35+)

## 7. Build and verify

- [ ] Build an Android App Bundle (AAB)
- [ ] Verify 16 KB page size compliance for all native libraries (APK Analyzer / Play Console)
- [ ] Test on physical devices (low-end and current Android version)
- [ ] Upload to internal testing track and review the pre-launch report

## 8. Play Console compliance

- [ ] Confirm developer account status and identity verification are current
- [ ] Update Data safety form to match the remaining SDKs and data collection
- [ ] Update or create privacy policy (URL in store listing and in-app)
- [ ] Ads declaration (contains ads: yes/no)
- [ ] Re-do content rating questionnaire (IARC)
- [ ] Target audience and content settings
- [ ] Refresh store listing: screenshots, feature graphic, description
- [ ] Release to production (staged rollout)
