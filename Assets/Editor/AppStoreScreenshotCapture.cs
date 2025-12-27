using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Unity Editor tool to capture App Store screenshots from game scenes
/// Usage: In Unity Editor, go to Tools > Capture App Store Screenshots
/// </summary>
public class AppStoreScreenshotCapture : EditorWindow
{
    private string outputFolder = "Screenshots";
    private bool captureAllScenes = true;
    private List<string> selectedScenes = new List<string>();
    
    // Orientation
    private enum Orientation
    {
        Landscape,
        Portrait
    }
    private Orientation orientation = Orientation.Portrait;
    
    // iPhone screenshot sizes (required by App Store)
    private enum ScreenshotSize
    {
        iPhone67inch_1290x2796,     // iPhone 14 Pro Max, 15 Plus, 15 Pro Max
        iPhone65inch_1242x2688,     // iPhone XS Max, 11 Pro Max
        iPhone61inch_1170x2532,     // iPhone 12/13/14/15
        iPhone58inch_1125x2436,     // iPhone X/XS/11 Pro
        iPad129inch_2048x2732       // iPad Pro 12.9"
    }
    
    private ScreenshotSize selectedSize = ScreenshotSize.iPhone67inch_1290x2796;
    private int superSampleMultiplier = 2;
    
    [MenuItem("Tools/Capture App Store Screenshots")]
    public static void ShowWindow()
    {
        GetWindow<AppStoreScreenshotCapture>("App Store Screenshots");
    }
    
    void OnGUI()
    {
        GUILayout.Label("App Store Screenshot Capture", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Output folder
        EditorGUILayout.LabelField("Output Folder:");
        EditorGUILayout.BeginHorizontal();
        outputFolder = EditorGUILayout.TextField(outputFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Screenshot Folder", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                outputFolder = path;
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Screenshot size
        EditorGUILayout.LabelField("Screenshot Size:");
        selectedSize = (ScreenshotSize)EditorGUILayout.EnumPopup(selectedSize);
        
        EditorGUILayout.Space();
        
        // Orientation
        EditorGUILayout.LabelField("Orientation:");
        orientation = (Orientation)EditorGUILayout.EnumPopup(orientation);
        
        EditorGUILayout.Space();
        
        // Quality
        EditorGUILayout.LabelField("Super Sampling (higher = better quality, slower):");
        superSampleMultiplier = EditorGUILayout.IntSlider(superSampleMultiplier, 1, 4);
        
        EditorGUILayout.Space();
        
        // Scene selection
        captureAllScenes = EditorGUILayout.Toggle("Capture All Scenes", captureAllScenes);
        
        if (!captureAllScenes)
        {
            EditorGUILayout.HelpBox("Manual scene selection not yet implemented. Using all scenes.", MessageType.Info);
        }
        
        EditorGUILayout.Space();
        
        // Capture button
        if (GUILayout.Button("Capture Screenshots", GUILayout.Height(40)))
        {
            CaptureScreenshots();
        }
        
        EditorGUILayout.Space();
        
        // Info
        EditorGUILayout.HelpBox(
            "This tool will:\n" +
            "1. Load each scene in your build settings\n" +
            "2. Wait for scene to load\n" +
            "3. Capture a screenshot at the selected resolution\n" +
            "4. Save to the output folder\n\n" +
            "Screenshots will be saved as PNG files ready for App Store upload.",
            MessageType.Info
        );
    }
    
    private void CaptureScreenshots()
    {
        // Get resolution based on selected size
        Vector2Int resolution = GetResolution(selectedSize);
        
        // Create output folder
        string fullOutputPath = Path.GetFullPath(outputFolder);
        if (!Directory.Exists(fullOutputPath))
        {
            Directory.CreateDirectory(fullOutputPath);
        }
        
        // Get all scenes from build settings
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        
        if (scenes.Length == 0)
        {
            EditorUtility.DisplayDialog("No Scenes", "No scenes found in Build Settings!", "OK");
            return;
        }
        
        Debug.Log($"========================================");
        Debug.Log($"Starting screenshot capture for {scenes.Length} scenes");
        Debug.Log($"Resolution: {resolution.x}x{resolution.y}");
        Debug.Log($"Output folder: {fullOutputPath}");
        Debug.Log($"========================================");
        
        int captured = 0;
        
        foreach (EditorBuildSettingsScene scene in scenes)
        {
            if (!scene.enabled)
            {
                Debug.Log($"Skipping disabled scene: {scene.path}");
                continue;
            }
            
            // Load the scene
            Debug.Log($"Loading scene: {scene.path}");
            var loadedScene = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
            
            // Get scene name for filename
            string sceneName = Path.GetFileNameWithoutExtension(scene.path);
            string filename = $"{sceneName}_{resolution.x}x{resolution.y}.png";
            string fullPath = Path.Combine(fullOutputPath, filename);
            
            // Find camera in scene or use Scene view camera
            Camera camera = Camera.main;
            if (camera == null)
            {
                camera = GameObject.FindObjectOfType<Camera>();
            }
            
            if (camera != null)
            {
                // Use camera to render screenshot
                Debug.Log($"Capturing from Camera: {camera.name}");
                CaptureFromCamera(camera, fullPath, resolution);
            }
            else
            {
                // Use Scene view camera
                Debug.Log($"No camera found, using Scene View");
                SceneView sceneView = SceneView.lastActiveSceneView;
                if (sceneView != null)
                {
                    CaptureFromSceneView(sceneView, fullPath, resolution);
                }
                else
                {
                    Debug.LogWarning($"⚠️ No camera or Scene View available for {sceneName}");
                    continue;
                }
            }
            
            Debug.Log($"✅ Saved: {filename}");
            captured++;
        }
        
        Debug.Log($"========================================");
        Debug.Log($"✅ Captured {captured} screenshots!");
        Debug.Log($"📁 Saved to: {fullOutputPath}");
        Debug.Log($"========================================");
        
        // Show completion dialog
        bool openFolder = EditorUtility.DisplayDialog(
            "Screenshots Captured!",
            $"Successfully captured {captured} screenshots!\n\nLocation: {fullOutputPath}\n\nOpen folder?",
            "Open Folder",
            "Close"
        );
        
        if (openFolder)
        {
            System.Diagnostics.Process.Start(fullOutputPath);
        }
    }
    
    private void CaptureFromCamera(Camera camera, string filePath, Vector2Int resolution)
    {
        // Create render texture
        RenderTexture rt = new RenderTexture(resolution.x, resolution.y, 24);
        camera.targetTexture = rt;
        
        // Render
        camera.Render();
        
        // Read pixels
        RenderTexture.active = rt;
        Texture2D screenshot = new Texture2D(resolution.x, resolution.y, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, resolution.x, resolution.y), 0, 0);
        screenshot.Apply();
        
        // Save to file
        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
        
        // Clean up
        camera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);
        DestroyImmediate(screenshot);
    }
    
    private void CaptureFromSceneView(SceneView sceneView, string filePath, Vector2Int resolution)
    {
        // Create render texture
        RenderTexture rt = new RenderTexture(resolution.x, resolution.y, 24);
        
        // Render scene view
        sceneView.camera.targetTexture = rt;
        sceneView.camera.Render();
        
        // Read pixels
        RenderTexture.active = rt;
        Texture2D screenshot = new Texture2D(resolution.x, resolution.y, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, resolution.x, resolution.y), 0, 0);
        screenshot.Apply();
        
        // Save to file
        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
        
        // Clean up
        sceneView.camera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);
        DestroyImmediate(screenshot);
    }
    
    private Vector2Int GetResolution(ScreenshotSize size)
    {
        Vector2Int resolution;
        
        switch (size)
        {
            case ScreenshotSize.iPhone67inch_1290x2796:
                resolution = new Vector2Int(1290, 2796);
                break;
            case ScreenshotSize.iPhone65inch_1242x2688:
                resolution = new Vector2Int(1242, 2688);
                break;
            case ScreenshotSize.iPhone61inch_1170x2532:
                resolution = new Vector2Int(1170, 2532);
                break;
            case ScreenshotSize.iPhone58inch_1125x2436:
                resolution = new Vector2Int(1125, 2436);
                break;
            case ScreenshotSize.iPad129inch_2048x2732:
                resolution = new Vector2Int(2048, 2732);
                break;
            default:
                resolution = new Vector2Int(1290, 2796);
                break;
        }
        
        // Swap width/height for landscape orientation
        if (orientation == Orientation.Landscape)
        {
            return new Vector2Int(resolution.y, resolution.x);
        }
        
        return resolution;
    }
}
