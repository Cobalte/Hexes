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
- [ ] Confirm in Play Console whether the app is enrolled in Play App Signing

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
- [ ] Implement UMP consent (required for EEA/UK) before initializing ads
- [ ] Restore and modernize banner code in `PremiumController.cs` (real ad unit ID is in the comments; use test IDs during development)
- [ ] After the first AdMob build, check the merged manifest's permissions. The game itself needs none; the last release (v1.0.9, 2024) had only AdMob's set: `INTERNET`, `ACCESS_NETWORK_STATE`, `com.google.android.gms.permission.AD_ID`, `ACCESS_ADSERVICES_AD_ID` / `_ATTRIBUTION` / `_TOPICS`, `WAKE_LOCK`, `FOREGROUND_SERVICE` (via WorkManager). Anything beyond that list should be investigated
- [ ] Set up `app-ads.txt` on the developer website listed in Play Console
- [ ] Decide whether to offer a "remove ads" IAP (would need Unity IAP / Play Billing Library)

## 5. Engine and Android target upgrade

- [x] Upgrade to Unity 6.3 LTS (now on 6000.3.27f2; compiles and plays in editor; TextMesh Pro merged into uGUI automatically) — supported to Dec 2027; needed for API 36 target; also fixes CVE-2025-59489, which affects 2022.3.60f1. Not 6.0 (support ended Oct 2026) or 6.5/7.x (non-LTS; 6.5 deprecates the Built-In Render Pipeline this project uses)
- [x] Fix obsolete `EditorGUILayout.ObjectField` overload in `LocalizedFontInspector.cs` and `LocalizedTextInspector.cs` (warning CS0618)
- [x] Set `targetSdk` to 36 (Android 16) — required for updates since Aug 31, 2026
- [x] Lower `minSdk` from 34 to 25 (Android 7.1 — the lowest Unity 6.3 supports)
- [x] Application entry point: Activity (already set); custom `AndroidManifest.xml` removed
- [x] Consider dropping ARMv7 (ARM64 only) — decided to keep ARMv7 + ARM64: no extra cost to players (Play serves each device only its own architecture) and it keeps 32-bit-only low-end phones supported. Revisit if an SDK drops 32-bit support
- [x] Remove unused engine modules and packages (AI, cloth, terrain, umbra, Unity Analytics, vehicles, video, VR, wind, XR, XR Management + `Assets/XR/`, Multiplayer Center)
- [ ] Bump version (e.g. 1.1.0, code 32+). Note: old build files are named `SNS-Ver109-Pkg26` (v1.0.9, code 26) while the project says 1.0.3 / code 31 — check Play Console for the highest version code ever uploaded and go above it

## 6. Mobile optimization

- [x] Set `Application.targetFrameRate = 60` in `GameController.Start` (mobile defaults to 30)
- [x] Replace per-frame LINQ in `IsAnyBlockMoving` with a plain loop (`blocks.RemoveAll` is allocation-free and LINQ in `ExecuteSwipe` runs once per swipe — left as is)
- [x] Review texture import settings on large images (`BG_Texture.png`, `SplashScreen.png`) — already compressed sprites, no mipmaps, 2048 max; sprite atlases optional
- [x] Remove per-frame garbage: `Block.Update` (LINQ closure every frame per block), `Celebration.Update` (LINQ `All`), `ScoreDisplay` (could stall a hair below the score and rebuild its string every frame forever — now snaps and only updates when the number changes)
- [x] Stop UI Animators dirtying canvases every frame: new `AnimatorSleeper` switches one-shot animators off once their clip finishes (blocks, hungry nekos, menus, loading screen, camera/safe-area shake); code wakes them before playing. Buttons are left alone (Unity's button code needs their animators enabled); looping tutorial arrows never sleep. Unlock progress slider also stops nudging once it's within 0.1% of its target
- [x] Pool blocks and celebration effects (new `PrefabPool`) instead of Instantiate/Destroy; `Block` gets `GameController`/`UI Canvas` passed in instead of `GameObject.Find`. Blocks reset their animated pose (`Block.ResetPose`) before going back in the pool — without this, a block pooled mid-bounce came back with its sushi permanently off-center. Combo badges (`ScoreMultiplierPanel`) are **not** pooled: some of their particle systems destroy themselves when finished, so reused copies would be missing effects
- [x] Fix: red combo background disc (`ScoreMultiplierGroup/BG`, `Combo_BG.png`) peeked out from behind the x2–x16 badges — `ScoreMultiplierPanel` now shows it only at x1. The badges were also off-center from the meter's round end cap (`Meter_BG.png`): badge `Anchor` nudge (-2, 3) zeroed in all 5 combo prefabs, and `ScoreMultiplierGroup` moved from (5, 2) to (4.25, 0.25) so it's centered on the cap (measured from the sprite: center 114.5, 114.5 px at 0.5 units/px)
- [ ] Play-test the above in the editor and on a device: block spawn/merge/arrival animations, neko hungry/full, options + results menus open/close, camera shake, results screen board copy, a second game after game over
- [ ] Replace per-turn `BinaryFormatter` save with `JsonUtility` (keep reading the old binary save once so existing players keep their games)
- [ ] Battery: lower the render rate while the board is idle (`OnDemandRendering.renderFrameInterval`), full 60 fps while blocks move
- [ ] Profile on a low-end device (memory, frame time, startup time)
- [ ] Verify safe-area handling on modern aspect ratios and edge-to-edge display (enforced when targeting API 35+)

## 7. Build and verify

- [ ] Open the project in Unity and confirm it still compiles and plays after the module removal and Android settings changes

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
