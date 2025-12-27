using UnityEditor;
using UnityEngine;
using System;

/// <summary>
/// Build script for iOS deployment via CodeMagic CI/CD
/// This script is called from command line with -executeMethod BuildScript.BuildIOS
/// </summary>
public class BuildScript
{
    /// <summary>
    /// Build iOS app from command line
    /// Usage: Unity -quit -batchmode -executeMethod BuildScript.BuildIOS
    /// </summary>
    public static void BuildIOS()
    {
        Debug.Log("========================================");
        Debug.Log("Starting iOS build for CodeMagic...");
        Debug.Log("========================================");

        // Get build path from environment variable or use default
        string buildPath = Environment.GetEnvironmentVariable("IOS_BUILD_PATH");
        if (string.IsNullOrEmpty(buildPath))
        {
            buildPath = "build/ios";
            Debug.Log($"IOS_BUILD_PATH not set, using default: {buildPath}");
        }

        Debug.Log($"Build path: {buildPath}");
        Debug.Log($"Bundle Identifier: {PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS)}");
        Debug.Log($"Product Name: {PlayerSettings.productName}");
        Debug.Log($"Bundle Version: {PlayerSettings.bundleVersion}");

        // Configure company and product names for App Store compliance
        PlayerSettings.companyName = "Failaka Games";
        PlayerSettings.productName = "Failaka Island";
        PlayerSettings.bundleVersion = "1.0.3";  // Third submission - all popups fixed
        Debug.Log("✅ Set company: Failaka Games");
        Debug.Log("✅ Set product: Failaka Island");
        Debug.Log("✅ Set version: 1.0.2");

    // Set app icon automatically
    string iconPath = "Assets/AppIcon.png";
    if (System.IO.File.Exists(iconPath))
    {
        Debug.Log($"Setting app icon from: {iconPath}");
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
        if (icon != null)
        {
            // Set default icon for all platforms
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { icon });
            // Set iOS specific icons
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.iOS, new Texture2D[] { icon });
            Debug.Log("✅ App icon set successfully");
        }
        else
        {
            Debug.LogWarning("⚠️ Could not load app icon from Assets/AppIcon.png");
        }
    }
    else
    {
        Debug.LogWarning("⚠️ App icon not found at Assets/AppIcon.png - using default");
    }

    // Configure iOS build settings
        PlayerSettings.iOS.buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? "1";
        
        // Set bundle identifier if provided via environment
        string bundleId = Environment.GetEnvironmentVariable("BUNDLE_ID");
        if (!string.IsNullOrEmpty(bundleId))
        {
            Debug.Log($"Setting bundle identifier to: {bundleId}");
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, bundleId);
        }

        // Get all enabled scenes from build settings
        string[] scenes = GetEnabledScenes();
        Debug.Log($"Building {scenes.Length} scenes:");
        foreach (string scene in scenes)
        {
            Debug.Log($"  - {scene}");
        }

        // Configure build options
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        // Perform the build
        Debug.Log("Starting build...");
        UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        UnityEditor.Build.Reporting.BuildSummary summary = report.summary;

        Debug.Log("========================================");
        if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"✅ Build succeeded!");
            Debug.Log($"Build time: {summary.totalTime}");
            Debug.Log($"Build size: {summary.totalSize} bytes");
            Debug.Log($"Output path: {summary.outputPath}");
            Debug.Log("========================================");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"❌ Build failed!");
            Debug.LogError($"Result: {summary.result}");
            Debug.LogError($"Total errors: {summary.totalErrors}");
            Debug.LogError($"Total warnings: {summary.totalWarnings}");
            Debug.Log("========================================");
            EditorApplication.Exit(1);
        }
    }

    /// <summary>
    /// Get all enabled scenes from EditorBuildSettings
    /// </summary>
    private static string[] GetEnabledScenes()
    {
        System.Collections.Generic.List<string> scenes = new System.Collections.Generic.List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                scenes.Add(scene.path);
            }
        }
        
        if (scenes.Count == 0)
        {
            Debug.LogWarning("No enabled scenes found in build settings! Build will fail.");
        }
        
        return scenes.ToArray();
    }
}
