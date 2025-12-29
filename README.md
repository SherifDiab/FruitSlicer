# Fruit Slicer - Unity Android Game with AdMob

A Fruit Ninja-style game built with Unity, configured for Android deployment with Google AdMob integration.

## Project Overview

This project is based on the [Zigurous Fruit Ninja Tutorial](https://github.com/zigurous/unity-fruit-ninja-tutorial) and has been enhanced with:
- Android build configuration
- Google AdMob integration (Banner, Interstitial, and Rewarded ads)
- Custom Gradle templates for Android dependencies
- Editor build helper scripts

## Requirements

- **Unity Version**: 6000.3.2f1 (Unity 6) or later
- **Android Build Support**: Install via Unity Hub
- **JDK**: OpenJDK 17 (included with Unity 6)
- **Android SDK**: API Level 24+ (Android 7.0 Nougat)
- **Target SDK**: API Level 34 (Android 14)
- **Gradle**: 8.4+ (included with Unity 6)
- **Android Gradle Plugin**: 8.3.0+

## Quick Start

1. Open the project in Unity 6 (6000.3.2f1 or later)
2. Install the Google Mobile Ads Unity Plugin (see below)
3. Go to **Build > Configure Android Settings** from the menu
4. Build using **Build > Build Android APK** or **Build > Build Android AAB**

---

# Debugging and Export Guidelines

## Installing Google Mobile Ads Unity Plugin

Before building, you need to install the Google Mobile Ads Unity SDK:

### Method 1: Using Unity Package Manager (Recommended)

1. Download the latest Google Mobile Ads Unity plugin from:
   https://github.com/googleads/googleads-mobile-unity/releases

2. In Unity, go to **Assets > Import Package > Custom Package**

3. Select the downloaded `.unitypackage` file

4. Import all assets

### Method 2: Using External Dependency Manager

1. Download the plugin from the releases page
2. Import the package
3. Go to **Assets > External Dependency Manager > Android Resolver > Resolve**

## Android Build Configuration

### Setting Up Android SDK

1. Open **Edit > Preferences > External Tools** (Windows/Linux) or **Unity > Settings > External Tools** (macOS)

2. Verify the following paths are set:
   - **JDK**: Use Unity's bundled JDK 17 (recommended for Unity 6)
   - **Android SDK**: Path to your Android SDK
   - **Android NDK**: Path to your Android NDK (required for IL2CPP builds in Unity 6)

3. Install required SDK components via Android SDK Manager:
   - Android SDK Platform 34 (API 34)
   - Android SDK Build-Tools 34.0.0+
   - Android SDK Platform-Tools
   - Android SDK Command-line Tools
   - NDK (Side by side) 26.1+

### Build Settings

1. Go to **File > Build Settings**

2. Select **Android** platform and click **Switch Platform**

3. Configure Player Settings:
   - **Company Name**: FruitSlicer
   - **Product Name**: Fruit Slicer
   - **Package Name**: com.fruitslicer.game
   - **Minimum API Level**: Android 7.0 (API 24) - Required for Unity 6
   - **Target API Level**: Android 14 (API 34) - Required for Google Play
   - **Scripting Backend**: IL2CPP (required for Unity 6 on Android)
   - **Target Architectures**: ARM64 (ARMv7 is deprecated in Unity 6)

### Building APK

1. From the Unity menu, select **Build > Build Android APK**

2. Or manually:
   - Go to **File > Build Settings**
   - Ensure Android is selected
   - Click **Build**
   - Choose output location

### Building AAB (App Bundle) for Google Play

1. From the Unity menu, select **Build > Build Android AAB**

2. Or manually:
   - Go to **File > Build Settings**
   - Check **Build App Bundle (Google Play)**
   - Click **Build**

---

## Debugging

### Unity Editor Debugging

1. **Console Window**:
   - Open via **Window > General > Console**
   - Shows all Debug.Log, warnings, and errors
   - Enable "Error Pause" to stop play mode on errors

2. **Profiler**:
   - Open via **Window > Analysis > Profiler**
   - Monitors CPU, GPU, Memory usage
   - Enable "Deep Profile" for detailed call stacks

3. **Frame Debugger**:
   - Open via **Window > Analysis > Frame Debugger**
   - Step through rendering commands
   - Identify rendering issues

### Android Device Debugging

#### Using ADB (Android Debug Bridge)

1. **Enable Developer Options** on your Android device:
   - Go to Settings > About Phone
   - Tap "Build Number" 7 times
   - Go back to Settings > Developer Options
   - Enable "USB Debugging"

2. **Connect Device** and verify:
   ```bash
   adb devices
   ```

3. **View Logs**:
   ```bash
   # All Unity logs
   adb logcat -s Unity

   # Filter by tag
   adb logcat -s Unity:V ActivityManager:I

   # Clear logs first
   adb logcat -c && adb logcat -s Unity
   ```

4. **Install APK**:
   ```bash
   adb install -r path/to/FruitSlicer.apk
   ```

5. **Uninstall App**:
   ```bash
   adb uninstall com.fruitslicer.game
   ```

#### Using Unity Remote Debugging

1. In Unity, go to **File > Build Settings**

2. Check **Development Build**

3. Check **Script Debugging** (optional, slower)

4. Check **Wait for Managed Debugger** (optional)

5. Build and run on device

6. In Unity, connect to device via **Window > Analysis > Profiler**

#### Using Android Studio Logcat

1. Open Android Studio

2. Go to **View > Tool Windows > Logcat**

3. Select your device from the dropdown

4. Filter by package name: `com.fruitslicer.game`

### AdMob Debugging

#### Enable Test Mode

The project is configured with Google's test Ad Unit IDs by default:
- Banner: `ca-app-pub-3940256099942544/6300978111`
- Interstitial: `ca-app-pub-3940256099942544/1033173712`
- Rewarded: `ca-app-pub-3940256099942544/5224354917`
- App ID: `ca-app-pub-3940256099942544~3347511713`

**Important**: Replace these with your actual AdMob IDs before releasing!

#### Ad Inspector

1. In your app, shake the device or call:
   ```csharp
   MobileAds.OpenAdInspector(error => { });
   ```

2. View ad request status, errors, and configurations

#### Common AdMob Issues

1. **Ads not showing**:
   - Verify internet permission is enabled
   - Check AdMob App ID in AndroidManifest.xml
   - Ensure Google Mobile Ads SDK is properly imported
   - Wait 15-30 minutes for new ad units to activate

2. **Test ads showing in production**:
   - Replace test Ad Unit IDs with real IDs
   - Remove test device IDs from AdRequest

3. **App crashes on ad load**:
   - Verify SDK initialization before loading ads
   - Check for null references in ad callbacks

---

## Exporting for Release

### Preparing for Release

1. **Update AdMob IDs**:
   - Replace test IDs in `Assets/Scripts/Ads/AdMobManager.cs`
   - Update App ID in `Assets/Plugins/Android/AndroidManifest.xml`

2. **Create Keystore** (one-time setup):
   ```bash
   keytool -genkey -v -keystore fruitslicer.keystore -alias fruitslicer -keyalg RSA -keysize 2048 -validity 10000
   ```

3. **Configure Signing**:
   - Go to **Edit > Project Settings > Player > Publishing Settings**
   - Check **Custom Keystore**
   - Select your keystore file
   - Enter keystore password
   - Select key alias
   - Enter key password

### Building Release APK

1. Go to **File > Build Settings**

2. Ensure **Development Build** is UNCHECKED

3. Click **Build**

4. The APK will be signed with your keystore

### Building Release AAB

1. Go to **File > Build Settings**

2. Check **Build App Bundle (Google Play)**

3. Ensure **Development Build** is UNCHECKED

4. Click **Build**

### Uploading to Google Play

1. Go to [Google Play Console](https://play.google.com/console)

2. Create a new app or select existing

3. Go to **Release > Production**

4. Upload your AAB file

5. Fill in store listing details

6. Submit for review

---

## Project Structure

```
FruitSlicer/
├── Assets/
│   ├── Editor/
│   │   └── AndroidBuildHelper.cs      # Build automation scripts
│   ├── Fonts/                         # Game fonts
│   ├── Materials/                     # Materials for fruits/effects
│   ├── Models/                        # 3D models
│   ├── Physics/                       # Physics materials
│   ├── Plugins/
│   │   └── Android/
│   │       ├── AndroidManifest.xml    # Android manifest with AdMob
│   │       ├── mainTemplate.gradle    # Main gradle with dependencies
│   │       ├── baseProjectTemplate.gradle
│   │       └── gradleTemplate.properties
│   ├── Prefabs/                       # Game prefabs
│   ├── Scenes/
│   │   └── FruitNinja.unity          # Main game scene
│   ├── Scripts/
│   │   ├── Ads/
│   │   │   └── AdMobManager.cs       # AdMob integration
│   │   ├── Blade.cs                  # Slicing mechanic
│   │   ├── Bomb.cs                   # Bomb behavior
│   │   ├── Fruit.cs                  # Fruit behavior
│   │   ├── GameManager.cs            # Game state management
│   │   └── Spawner.cs                # Fruit/bomb spawning
│   └── Textures/                     # Textures and sprites
├── Packages/
│   └── manifest.json                 # Package dependencies
├── ProjectSettings/                  # Unity project settings
└── README.md                         # This file
```

## AdMob Integration

### Using AdMob in Your Game

The `AdMobManager` is a singleton that persists across scenes:

```csharp
// Show banner ad
AdMobManager.Instance.ShowBannerAd();

// Hide banner ad
AdMobManager.Instance.HideBannerAd();

// Show interstitial ad (e.g., between levels)
if (AdMobManager.Instance.IsInterstitialReady())
{
    AdMobManager.Instance.ShowInterstitialAd();
}

// Show rewarded ad (e.g., for extra lives)
if (AdMobManager.Instance.IsRewardedAdReady())
{
    AdMobManager.Instance.OnRewardedAdCompleted += OnRewardEarned;
    AdMobManager.Instance.ShowRewardedAd();
}

void OnRewardEarned()
{
    // Give player reward
    AdMobManager.Instance.OnRewardedAdCompleted -= OnRewardEarned;
}
```

### Setting Up AdMobManager

1. Create an empty GameObject in your first scene

2. Add the `AdMobManager` component

3. Configure Ad Unit IDs in the Inspector (or leave defaults for testing)

4. The manager will automatically initialize and load ads

---

## Troubleshooting

### Build Errors

| Error | Solution |
|-------|----------|
| "Gradle build failed" | Check Android SDK path, update Gradle, clear Library folder |
| "JDK not found" | Configure JDK path in Preferences > External Tools |
| "Minimum API level error" | Update minSdkVersion in Player Settings |
| "Duplicate class" | Resolve dependency conflicts in gradle files |

### Runtime Errors

| Error | Solution |
|-------|----------|
| "AdMob not initialized" | Wait for MobileAds.Initialize callback |
| "No fill" | Normal for test ads, use test device ID |
| "App crash on launch" | Check AndroidManifest.xml for AdMob App ID |

### Common Issues

1. **Black screen on Android**:
   - Check graphics API settings
   - Verify scene is in Build Settings

2. **Touch not working**:
   - Check Input settings
   - Verify EventSystem exists in scene

3. **Performance issues**:
   - Use Profiler to identify bottlenecks
   - Reduce draw calls with batching
   - Optimize textures and meshes

---

## License

This project is for educational purposes. The original tutorial is by [Zigurous](https://github.com/zigurous).

## Support

For issues and questions:
- Check Unity documentation
- Visit [Google AdMob Help](https://support.google.com/admob)
- Review [Unity Android documentation](https://docs.unity3d.com/Manual/android.html)
