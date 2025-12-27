using UnityEditor;
using UnityEngine;
using System.IO;

public class AndroidBuildScript
{
    [MenuItem("Build/Build Android APK")]
    public static void BuildAPK()
    {
        // Set Android as build target
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        // Configure Android settings
        PlayerSettings.Android.bundleVersionCode = 3; // Increment for each build
        PlayerSettings.bundleVersion = "1.0.3";
        PlayerSettings.productName = "Failaka Island";
        PlayerSettings.companyName = "Failaka Games";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.failaka.games.adventure");

        // Android-specific settings
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24; // Android 7.0
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33; // Android 13
        
        // Graphics settings
        PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64; // Required for Play Store

        // Get all scenes
        string[] scenes = new string[]
        {
            "Assets/Scenes/Intro.unity",
            "Assets/Scenes/Map.unity",
            "Assets/Scenes/Menu.unity",
            "Assets/Scenes/Map 1 Museum/Museum Gate.unity",
            "Assets/Scenes/Map 1 Museum/Museum 1.unity",
            "Assets/Scenes/Map 2 Wells/wells Gate.unity",
            "Assets/Scenes/Map 2 Wells/wells.unity",
            "Assets/Scenes/Map 3 gemstones/gemstones Gate.unity",
            "Assets/Scenes/Map 3 gemstones/gemstones.unity",
            "Assets/Scenes/Map 4 Cinema/cinema Gate.unity",
            "Assets/Scenes/Map 4 Cinema/cinema.unity"
        };

        // Build output path
        string buildPath = "build/android/FailakaIsland.apk";
        
        // Create directory if it doesn't exist
        string directory = Path.GetDirectoryName(buildPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Build options
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.Android,
            options = BuildOptions.None // Use BuildOptions.Development for testing
        };

        Debug.Log("Starting Android APK build...");
        Debug.Log($"Output: {buildPath}");
        Debug.Log($"Version: {PlayerSettings.bundleVersion}");
        Debug.Log($"Version Code: {PlayerSettings.Android.bundleVersionCode}");

        // Build
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded! APK size: {report.summary.totalSize / (1024 * 1024)} MB");
            Debug.Log($"APK location: {Path.GetFullPath(buildPath)}");
            
            // Open folder
            EditorUtility.RevealInFinder(buildPath);
        }
        else
        {
            Debug.LogError($"Build failed! Result: {report.summary.result}");
            Debug.LogError($"Total errors: {report.summary.totalErrors}, Total warnings: {report.summary.totalWarnings}");
            
            // Show detailed error messages
            foreach (var step in report.steps)
            {
                foreach (var message in step.messages)
                {
                    if (message.type == UnityEngine.LogType.Error || 
                        message.type == UnityEngine.LogType.Exception)
                    {
                        Debug.LogError($"Build Error: {message.content}");
                    }
                }
            }
        }
    }

    [MenuItem("Build/Build Android App Bundle (AAB) for Play Store")]
    public static void BuildAAB()
    {
        // Set to build App Bundle instead of APK
        EditorUserBuildSettings.buildAppBundle = true;
        
        // Same settings as APK
        PlayerSettings.Android.bundleVersionCode = 2;
        PlayerSettings.bundleVersion = "1.0.2";
        PlayerSettings.productName = "Failaka Island";
        PlayerSettings.companyName = "Failaka Games";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.failaka.games.adventure");
        
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        string[] scenes = new string[]
        {
            "Assets/Scenes/Intro.unity",
            "Assets/Scenes/Map.unity",
            "Assets/Scenes/Menu.unity",
            "Assets/Scenes/Map 1 Museum/Museum Gate.unity",
            "Assets/Scenes/Map 1 Museum/Museum 1.unity",
            "Assets/Scenes/Map 2 Wells/wells Gate.unity",
            "Assets/Scenes/Map 2 Wells/wells.unity",
            "Assets/Scenes/Map 3 gemstones/gemstones Gate.unity",
            "Assets/Scenes/Map 3 gemstones/gemstones.unity",
            "Assets/Scenes/Map 4 Cinema/cinema Gate.unity",
            "Assets/Scenes/Map 4 Cinema/cinema.unity"
        };

        string buildPath = "build/android/FailakaIsland.aab";
        
        string directory = Path.GetDirectoryName(buildPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        Debug.Log("Starting Android App Bundle build for Google Play Store...");
        
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"AAB Build succeeded! Size: {report.summary.totalSize / (1024 * 1024)} MB");
            Debug.Log($"AAB location: {Path.GetFullPath(buildPath)}");
            EditorUtility.RevealInFinder(buildPath);
        }
        else
        {
            Debug.LogError($"Build failed with {report.summary.totalErrors} errors");
        }
        
        // Reset to APK for future builds
        EditorUserBuildSettings.buildAppBundle = false;
    }
}
