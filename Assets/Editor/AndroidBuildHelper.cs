using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using System.IO;

/// <summary>
/// Helper script for Android build configuration.
/// Provides menu items and build settings for the Fruit Slicer game.
/// </summary>
public class AndroidBuildHelper : MonoBehaviour
{
    private const string COMPANY_NAME = "FruitSlicer";
    private const string PRODUCT_NAME = "Fruit Slicer";
    private const string BUNDLE_IDENTIFIER = "com.fruitslicer.game";
    private const int BUNDLE_VERSION_CODE = 1;
    private const string BUNDLE_VERSION = "1.0.0";

    [MenuItem("Build/Configure Android Settings")]
    public static void ConfigureAndroidSettings()
    {
        // Set company and product name
        PlayerSettings.companyName = COMPANY_NAME;
        PlayerSettings.productName = PRODUCT_NAME;

        // Set Android specific settings
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, BUNDLE_IDENTIFIER);
        PlayerSettings.Android.bundleVersionCode = BUNDLE_VERSION_CODE;
        PlayerSettings.bundleVersion = BUNDLE_VERSION;

        // Set minimum SDK version (API 21 = Android 5.0 Lollipop)
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;

        // Set target SDK version (API 33 = Android 13)
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;

        // Use IL2CPP for better performance
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

        // Target ARM64 architecture for modern devices
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;

        // Enable custom gradle templates
        PlayerSettings.Android.useCustomGradlePropertiesTemplate = true;
        PlayerSettings.Android.useCustomMainGradleTemplate = true;
        PlayerSettings.Android.useCustomBaseGradleTemplate = true;
        PlayerSettings.Android.useCustomMainManifest = true;

        // Set screen orientation
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;

        // Require internet access for AdMob
        PlayerSettings.Android.forceInternetPermission = true;

        Debug.Log("Android settings configured successfully!");
        EditorUtility.DisplayDialog("Success", "Android build settings have been configured!", "OK");
    }

    [MenuItem("Build/Build Android APK")]
    public static void BuildAndroidAPK()
    {
        // Configure settings first
        ConfigureAndroidSettings();

        // Get scenes to build
        string[] scenes = GetEnabledScenes();

        if (scenes.Length == 0)
        {
            Debug.LogError("No scenes found to build!");
            return;
        }

        // Set build path
        string buildPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Builds/Android");
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }

        string apkPath = Path.Combine(buildPath, "FruitSlicer.apk");

        // Build options
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = apkPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        // Start build
        var report = BuildPipeline.BuildPlayer(buildOptions);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded: {apkPath}");
            EditorUtility.DisplayDialog("Build Successful", $"APK built at:\n{apkPath}", "OK");
            EditorUtility.RevealInFinder(apkPath);
        }
        else
        {
            Debug.LogError($"Build failed with {report.summary.totalErrors} errors");
            EditorUtility.DisplayDialog("Build Failed", $"Build failed with {report.summary.totalErrors} errors. Check console for details.", "OK");
        }
    }

    [MenuItem("Build/Build Android AAB (App Bundle)")]
    public static void BuildAndroidAAB()
    {
        // Configure settings first
        ConfigureAndroidSettings();

        // Enable App Bundle
        EditorUserBuildSettings.buildAppBundle = true;

        // Get scenes to build
        string[] scenes = GetEnabledScenes();

        if (scenes.Length == 0)
        {
            Debug.LogError("No scenes found to build!");
            return;
        }

        // Set build path
        string buildPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Builds/Android");
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }

        string aabPath = Path.Combine(buildPath, "FruitSlicer.aab");

        // Build options
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = aabPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        // Start build
        var report = BuildPipeline.BuildPlayer(buildOptions);

        // Reset App Bundle setting
        EditorUserBuildSettings.buildAppBundle = false;

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded: {aabPath}");
            EditorUtility.DisplayDialog("Build Successful", $"AAB built at:\n{aabPath}", "OK");
            EditorUtility.RevealInFinder(aabPath);
        }
        else
        {
            Debug.LogError($"Build failed with {report.summary.totalErrors} errors");
            EditorUtility.DisplayDialog("Build Failed", $"Build failed with {report.summary.totalErrors} errors. Check console for details.", "OK");
        }
    }

    private static string[] GetEnabledScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        var enabledScenes = new System.Collections.Generic.List<string>();

        foreach (var scene in scenes)
        {
            if (scene.enabled)
            {
                enabledScenes.Add(scene.path);
            }
        }

        // If no scenes in build settings, try to find the main scene
        if (enabledScenes.Count == 0)
        {
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                enabledScenes.Add(path);
            }
        }

        return enabledScenes.ToArray();
    }
}
