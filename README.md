# Fruit Slicer - Godot Android Game with AdMob

A Fruit Ninja-style game built with Godot 4.2, featuring AdMob integration for Android.

## Game Features

- **Three Game Modes:**
  - **Classic Mode**: 3 lives, lose a life when fruit falls, instant game over on bomb hit
  - **Arcade Mode**: 60-second timer, -10 points for bomb hits
  - **Zen Mode**: 90-second timer, no bombs, relaxed gameplay

- **Combo System**: Slice multiple fruits in quick succession for bonus points
- **High Score Persistence**: Saves and loads high scores
- **AdMob Integration**: Banner, Interstitial, and Rewarded ads

## Requirements

- **Godot Version**: 4.2 or later
- **Export Templates**: Android export templates installed
- **JDK**: OpenJDK 17
- **Android SDK**: API Level 24+ (Android 7.0 Nougat)
- **Target SDK**: API Level 33 (Android 13)
- **AdMob Plugin**: Poing Studios Godot AdMob Plugin (optional)

## Quick Start

1. Open the project in Godot 4.2+
2. Install the AdMob plugin (see below)
3. Go to **Project > Export** and configure Android settings
4. Build using **Export Project** or **Export With Debug**

---

## Project Structure

```
FruitSlicer/
├── project.godot          # Godot project configuration
├── scripts/
│   ├── game_manager.gd    # Game state, scoring, lives (Autoload)
│   ├── blade.gd           # Player input and slicing
│   ├── fruit.gd           # Fruit behavior
│   ├── bomb.gd            # Bomb behavior
│   ├── spawner.gd         # Fruit/bomb spawning
│   ├── fruit_half.gd      # Sliced fruit halves
│   ├── audio_manager.gd   # Sound effects and music (Autoload)
│   ├── admob_manager.gd   # AdMob ad management (Autoload)
│   ├── main.gd            # Main scene controller
│   └── ui/
│       └── game_ui.gd     # UI controller
├── scenes/
│   ├── main.tscn          # Main game scene
│   ├── fruit.tscn         # Fruit prefab
│   ├── bomb.tscn          # Bomb prefab
│   ├── fruit_half.tscn    # Sliced fruit half prefab
│   └── ui/
│       └── game_ui.tscn   # UI scene
├── assets/
│   ├── sprites/           # Game graphics
│   └── audio/
│       └── sfx/           # Sound effects
└── addons/                # Godot plugins (AdMob)
```

---

## Installing AdMob Plugin

### Method 1: Poing Studios Plugin (Recommended)

1. Download from: https://github.com/poing-studios/godot-admob-plugin/releases

2. Extract to your project's `addons/` folder

3. Enable in **Project > Project Settings > Plugins**

4. Update Ad Unit IDs in `scripts/admob_manager.gd`:
   ```gdscript
   const PROD_BANNER_ID: String = "ca-app-pub-XXXXX/XXXXX"
   const PROD_INTERSTITIAL_ID: String = "ca-app-pub-XXXXX/XXXXX"
   const PROD_REWARDED_ID: String = "ca-app-pub-XXXXX/XXXXX"
   ```

### Test Ad Unit IDs (Default)

The project uses Google's test Ad Unit IDs:
- Banner: `ca-app-pub-3940256099942544/6300978111`
- Interstitial: `ca-app-pub-3940256099942544/1033173712`
- Rewarded: `ca-app-pub-3940256099942544/5224354917`

**Important**: Replace these with your actual AdMob IDs before releasing!

---

## Android Export Setup

### Prerequisites

1. **Install Export Templates**:
   - Go to **Editor > Manage Export Templates**
   - Download templates for your Godot version

2. **Configure Android SDK**:
   - Go to **Editor > Editor Settings > Export > Android**
   - Set Android SDK path
   - Set debug keystore path (or create one)

### Setting Up Export

1. Go to **Project > Export**

2. Click **Add...** and select **Android**

3. Configure export settings:
   - **Package Unique Name**: `com.yourcompany.fruitslicer`
   - **Version Code**: 1
   - **Version Name**: 1.0.0
   - **Min SDK**: 24
   - **Target SDK**: 33

4. Under **Keystore**:
   - For debug: Use default debug keystore
   - For release: Create and configure a release keystore

### Creating Release Keystore

```bash
keytool -genkey -v -keystore my-release-key.jks \
  -keyalg RSA -keysize 2048 -validity 10000 \
  -alias my-key-alias
```

### Building APK

1. Go to **Project > Export**
2. Select Android preset
3. Click **Export Project** (release) or **Export With Debug** (debug)
4. Choose output location and filename

---

## Debugging

### Enable Developer Mode on Android Device

1. Go to **Settings > About Phone**
2. Tap **Build Number** 7 times
3. Go back to **Settings > Developer Options**
4. Enable **USB Debugging**

### ADB Commands

```bash
# Check connected devices
adb devices

# Install APK
adb install -r fruit_slicer.apk

# View Godot logs
adb logcat -s godot:* GodotAdMob:*

# Clear logs and view fresh
adb logcat -c && adb logcat -s godot

# Clear app data
adb shell pm clear com.yourcompany.fruitslicer

# Uninstall app
adb uninstall com.yourcompany.fruitslicer

# Take screenshot
adb exec-out screencap -p > screenshot.png

# Record screen
adb shell screenrecord /sdcard/recording.mp4
```

### Remote Debugging with Godot

1. Export with **Export With Debug** option
2. In Godot, go to **Editor > Editor Settings > Remote Debug**
3. Enable remote debugging
4. Run the app on device
5. Connect via Godot's **Remote** tab in the Scene dock

### Logcat Filtering

```bash
# AdMob specific logs
adb logcat | grep -E "(AdMob|Godot)"

# All game logs
adb logcat -s godot

# Errors only
adb logcat *:E | grep -i godot
```

---

## AdMob Integration

### Using AdMob in Your Game

The `AdMobManager` is an autoload singleton:

```gdscript
# Show banner ad
AdMobManager.load_banner("BOTTOM")
AdMobManager.show_banner()

# Hide banner ad
AdMobManager.hide_banner()

# Show interstitial ad
if AdMobManager.interstitial_loaded_flag:
    AdMobManager.show_interstitial()

# Show rewarded ad
if AdMobManager.is_rewarded_ready():
    AdMobManager.show_rewarded()

# Listen for reward
AdMobManager.rewarded_earned.connect(_on_reward_earned)

func _on_reward_earned(type: String, amount: int) -> void:
    # Give player reward
    pass
```

### AdMob Signals

```gdscript
signal banner_loaded
signal banner_failed(error_code: int)
signal interstitial_loaded
signal interstitial_failed(error_code: int)
signal interstitial_closed
signal rewarded_loaded
signal rewarded_failed(error_code: int)
signal rewarded_earned(type: String, amount: int)
signal rewarded_closed
```

---

## Troubleshooting

### Build Errors

| Error | Solution |
|-------|----------|
| Export templates not found | Install via Editor > Manage Export Templates |
| Android SDK not found | Configure in Editor Settings > Export > Android |
| JDK not found | Install JDK 17 and set path |
| Keystore error | Create keystore or use debug keystore |

### Runtime Errors

| Error | Solution |
|-------|----------|
| AdMob not initialized | Ensure plugin is installed and enabled |
| No fill (no ads) | Normal for test ads, check network |
| App crash on launch | Check logcat for errors |

### Common Issues

1. **Touch not working**:
   - Verify `emulate_touch_from_mouse` is enabled in Project Settings
   - Check Area2D collision layers

2. **Performance issues**:
   - Use mobile renderer in Project Settings
   - Reduce particle counts
   - Pool frequently spawned objects

3. **Ads not showing**:
   - Verify internet permission
   - Check Ad Unit IDs
   - Wait 15-30 minutes for new ad units

4. **Black screen on Android**:
   - Check if main scene is set correctly
   - Verify renderer settings

---

## Testing Checklist

- [ ] Classic mode: Lives decrease when fruit missed
- [ ] Classic mode: Game over on bomb hit
- [ ] Arcade mode: Timer counts down from 60
- [ ] Arcade mode: -10 points on bomb hit
- [ ] Zen mode: No bombs spawn
- [ ] Zen mode: Timer counts down from 90
- [ ] Combo system triggers at 3+ rapid slices
- [ ] High score saves and persists
- [ ] Banner ad loads and displays
- [ ] Interstitial ad shows after game over
- [ ] Rewarded ad grants extra life
- [ ] Touch input works smoothly
- [ ] Audio plays correctly

---

## Building for Release

1. Set `use_test_ads = false` in AdMobManager
2. Configure release keystore in export settings
3. Update version code and version name
4. Test thoroughly on multiple devices
5. Export release APK
6. Sign with release keystore

---

## License

This project is for educational purposes. Original gameplay based on Fruit Ninja.

## Support

For issues and questions:
- Check [Godot documentation](https://docs.godotengine.org)
- Visit [Google AdMob Help](https://support.google.com/admob)
- Review [Godot Android documentation](https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_android.html)
