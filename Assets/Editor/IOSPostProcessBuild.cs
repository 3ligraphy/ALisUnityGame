#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

/// <summary>
/// Post-process build script to configure iOS Info.plist for App Store compliance
/// Adds required privacy descriptions and encryption export compliance
/// </summary>
public class IOSPostProcessBuild
{
    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.iOS)
        {
            string plistPath = Path.Combine(path, "Info.plist");
            
            if (!File.Exists(plistPath))
            {
                UnityEngine.Debug.LogError($"Info.plist not found at: {plistPath}");
                return;
            }
            
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            
            PlistElementDict rootDict = plist.root;
            
            // CRITICAL: Encryption Export Compliance Declaration
            // Set to false if app doesn't use encryption beyond HTTPS
            // Set to true if app uses custom encryption and requires export compliance
            rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);
            UnityEngine.Debug.Log("✅ Set ITSAppUsesNonExemptEncryption = false");
            
            // Privacy Descriptions (optional - uncomment only if your app uses these features)
            // Failaka Island doesn't appear to use camera/microphone/location from the assets
            
            // Example: Uncomment if you add camera functionality
            // rootDict.SetString("NSCameraUsageDescription", "Failaka Island needs camera access to take photos of your adventure.");
            
            // Example: Uncomment if you add microphone functionality
            // rootDict.SetString("NSMicrophoneUsageDescription", "Failaka Island needs microphone access for voice features.");
            
            // Example: Uncomment if you add location features
            // rootDict.SetString("NSLocationWhenInUseUsageDescription", "Failaka Island needs your location to enhance your adventure.");
            
            // Example: Uncomment if you add photo library access
            // rootDict.SetString("NSPhotoLibraryUsageDescription", "Failaka Island needs photo library access to save your adventure photos.");
            
            // Save the modified plist
            plist.WriteToFile(plistPath);
            
            UnityEngine.Debug.Log("========================================");
            UnityEngine.Debug.Log("✅ iOS Post-Process Build Complete");
            UnityEngine.Debug.Log("Added encryption compliance declaration");
            UnityEngine.Debug.Log("========================================");
        }
    }
}
#endif
