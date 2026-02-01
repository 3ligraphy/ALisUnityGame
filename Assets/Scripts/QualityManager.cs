using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// Global Quality Manager - Adjusts quality settings across all scenes for better performance.
/// Attach this to a GameObject and it will persist across scenes.
/// </summary>
public class QualityManager : MonoBehaviour
{
    [Header("Quality Preset")]
    [Tooltip("Select a quality preset")]
    public QualityPreset qualityPreset = QualityPreset.Medium;
    
    [Header("Texture Settings")]
    [Tooltip("Texture quality (0=Full, 1=Half, 2=Quarter, 3=Eighth)")]
    [Range(0, 3)]
    public int textureQuality = 1;
    
    [Header("Shadow Settings")]
    [Tooltip("Enable shadows")]
    public bool enableShadows = true;
    [Tooltip("Shadow resolution")]
    public ShadowResolution shadowResolution = ShadowResolution.Medium;
    [Tooltip("Shadow distance")]
    public float shadowDistance = 50f;
    
    [Header("Rendering Settings")]
    [Tooltip("Anti-aliasing (0=Off, 2=2x, 4=4x, 8=8x)")]
    public int antiAliasing = 0;
    [Tooltip("Anisotropic filtering")]
    public AnisotropicFiltering anisotropicFiltering = AnisotropicFiltering.Disable;
    [Tooltip("VSync count (0=Off, 1=Every VBlank, 2=Every 2nd VBlank)")]
    [Range(0, 2)]
    public int vSyncCount = 0;
    
    [Header("LOD Settings")]
    [Tooltip("LOD bias (lower = use lower LOD earlier, better performance)")]
    [Range(0.3f, 2f)]
    public float lodBias = 0.7f;
    [Tooltip("Maximum LOD level (higher = lower quality but better performance)")]
    [Range(0, 3)]
    public int maximumLODLevel = 1;
    
    [Header("Water/Effects Settings")]
    [Tooltip("Reduce water quality (affects Suimono and similar systems)")]
    public bool reduceWaterQuality = true;
    [Tooltip("Reduce particle effects")]
    public bool reduceParticles = true;
    [Tooltip("Particle budget multiplier (0.5 = half particles)")]
    [Range(0.1f, 1f)]
    public float particleBudget = 0.5f;
    
    [Header("Performance")]
    [Tooltip("Target frame rate (0 = no limit, 30 or 60 recommended for mobile)")]
    public int targetFrameRate = 30;
    
    public enum QualityPreset
    {
        VeryLow,
        Low,
        Medium,
        High
    }
    
    public enum ShadowResolution
    {
        Low = 512,
        Medium = 1024,
        High = 2048,
        VeryHigh = 4096
    }
    
    private static QualityManager instance;
    
    void Awake()
    {
        // Singleton pattern - persist across scenes
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Apply quality settings on start
        ApplyQualitySettings();
    }
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }
    
    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Wait a frame then apply quality settings to new scene
        StartCoroutine(ApplyQualityAfterSceneLoad());
    }
    
    System.Collections.IEnumerator ApplyQualityAfterSceneLoad()
    {
        yield return null; // Wait one frame for scene to fully load
        OnSceneLoaded();
    }
    
    void Start()
    {
        // Apply preset and then individual settings
        ApplyPreset(qualityPreset);
        ApplyQualitySettings();
        
        // Find and reduce water quality
        if (reduceWaterQuality)
        {
            ReduceWaterQuality();
        }
        
        // Reduce particle effects
        if (reduceParticles)
        {
            ReduceParticleEffects();
        }
    }
    
    /// <summary>
    /// Apply a quality preset
    /// </summary>
    public void ApplyPreset(QualityPreset preset)
    {
        switch (preset)
        {
            case QualityPreset.VeryLow:
                textureQuality = 3;          // Eighth resolution
                enableShadows = false;
                shadowDistance = 20f;
                antiAliasing = 0;
                anisotropicFiltering = AnisotropicFiltering.Disable;
                lodBias = 0.3f;
                maximumLODLevel = 2;
                reduceWaterQuality = true;
                reduceParticles = true;
                particleBudget = 0.25f;
                targetFrameRate = 30;
                break;
                
            case QualityPreset.Low:
                textureQuality = 2;          // Quarter resolution
                enableShadows = false;
                shadowDistance = 30f;
                antiAliasing = 0;
                anisotropicFiltering = AnisotropicFiltering.Disable;
                lodBias = 0.5f;
                maximumLODLevel = 1;
                reduceWaterQuality = true;
                reduceParticles = true;
                particleBudget = 0.5f;
                targetFrameRate = 30;
                break;
                
            case QualityPreset.Medium:
                textureQuality = 1;          // Half resolution
                enableShadows = true;
                shadowResolution = ShadowResolution.Low;
                shadowDistance = 50f;
                antiAliasing = 0;
                anisotropicFiltering = AnisotropicFiltering.Disable;
                lodBias = 0.7f;
                maximumLODLevel = 0;
                reduceWaterQuality = true;
                reduceParticles = true;
                particleBudget = 0.7f;
                targetFrameRate = 30;
                break;
                
            case QualityPreset.High:
                textureQuality = 0;          // Full resolution
                enableShadows = true;
                shadowResolution = ShadowResolution.Medium;
                shadowDistance = 100f;
                antiAliasing = 2;
                anisotropicFiltering = AnisotropicFiltering.Enable;
                lodBias = 1f;
                maximumLODLevel = 0;
                reduceWaterQuality = false;
                reduceParticles = false;
                particleBudget = 1f;
                targetFrameRate = 60;
                break;
        }
        
        ApplyQualitySettings();
        Debug.Log($"QualityManager: Applied {preset} preset");
    }
    
    /// <summary>
    /// Apply all quality settings
    /// </summary>
    public void ApplyQualitySettings()
    {
        // Texture quality
        QualitySettings.globalTextureMipmapLimit = textureQuality;
        
        // Shadows
        if (enableShadows)
        {
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = (UnityEngine.ShadowResolution)GetShadowResolutionIndex();
            QualitySettings.shadowDistance = shadowDistance;
        }
        else
        {
            QualitySettings.shadows = ShadowQuality.Disable;
        }
        
        // Anti-aliasing
        QualitySettings.antiAliasing = antiAliasing;
        
        // Anisotropic filtering
        QualitySettings.anisotropicFiltering = anisotropicFiltering;
        
        // VSync
        QualitySettings.vSyncCount = vSyncCount;
        
        // LOD
        QualitySettings.lodBias = lodBias;
        QualitySettings.maximumLODLevel = maximumLODLevel;
        
        // Frame rate
        Application.targetFrameRate = targetFrameRate;
        
        Debug.Log($"QualityManager: Settings applied - Textures:{textureQuality}, Shadows:{enableShadows}, LOD:{lodBias}");
    }
    
    int GetShadowResolutionIndex()
    {
        switch (shadowResolution)
        {
            case ShadowResolution.Low: return 0;
            case ShadowResolution.Medium: return 1;
            case ShadowResolution.High: return 2;
            case ShadowResolution.VeryHigh: return 3;
            default: return 1;
        }
    }
    
    /// <summary>
    /// Reduce water quality for Suimono and similar water systems
    /// </summary>
    void ReduceWaterQuality()
    {
        // Find Suimono water objects
        var suimonoObjects = FindObjectsOfType<MonoBehaviour>();
        foreach (var obj in suimonoObjects)
        {
            string typeName = obj.GetType().Name.ToLower();
            
            // Suimono water system
            if (typeName.Contains("suimono") || typeName.Contains("water"))
            {
                // Try to reduce reflection quality
                TrySetProperty(obj, "reflectionResolution", 128);
                TrySetProperty(obj, "refractionResolution", 128);
                TrySetProperty(obj, "enableReflections", false);
                TrySetProperty(obj, "enableRefraction", false);
                TrySetProperty(obj, "enableCaustics", false);
                TrySetProperty(obj, "waveScale", 0.5f);
                TrySetProperty(obj, "enableFoam", false);
                TrySetProperty(obj, "enableUnderwaterFX", false);
                
                Debug.Log($"QualityManager: Reduced quality for {obj.GetType().Name}");
            }
        }
        
        // Disable reflection probes or reduce their quality
        var reflectionProbes = FindObjectsOfType<ReflectionProbe>();
        foreach (var probe in reflectionProbes)
        {
            probe.resolution = 64;
            probe.intensity = 0.5f;
        }
        
        Debug.Log($"QualityManager: Reduced {reflectionProbes.Length} reflection probes");
    }
    
    /// <summary>
    /// Reduce particle system quality
    /// </summary>
    void ReduceParticleEffects()
    {
        var particleSystems = FindObjectsOfType<ParticleSystem>();
        foreach (var ps in particleSystems)
        {
            var main = ps.main;
            
            // Reduce max particles
            main.maxParticles = Mathf.RoundToInt(main.maxParticles * particleBudget);
            
            // Reduce emission rate
            var emission = ps.emission;
            if (emission.enabled)
            {
                var rateOverTime = emission.rateOverTime;
                if (rateOverTime.mode == ParticleSystemCurveMode.Constant)
                {
                    emission.rateOverTime = rateOverTime.constant * particleBudget;
                }
            }
            
            // Disable sub-emitters for performance
            var subEmitters = ps.subEmitters;
            if (subEmitters.subEmittersCount > 0 && particleBudget < 0.5f)
            {
                subEmitters.enabled = false;
            }
        }
        
        Debug.Log($"QualityManager: Reduced {particleSystems.Length} particle systems");
    }
    
    /// <summary>
    /// Helper to try setting a property on an object via reflection
    /// </summary>
    void TrySetProperty(object obj, string propertyName, object value)
    {
        try
        {
            var property = obj.GetType().GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(obj, value);
            }
            else
            {
                var field = obj.GetType().GetField(propertyName);
                if (field != null)
                {
                    field.SetValue(obj, value);
                }
            }
        }
        catch
        {
            // Silently fail - property might not exist
        }
    }
    
    /// <summary>
    /// Call this when entering a new scene to re-apply quality reductions
    /// </summary>
    public void OnSceneLoaded()
    {
        ApplyQualitySettings();
        
        if (reduceWaterQuality)
        {
            ReduceWaterQuality();
        }
        
        if (reduceParticles)
        {
            ReduceParticleEffects();
        }
    }
    
    /// <summary>
    /// Public static method to set quality from anywhere
    /// </summary>
    public static void SetQuality(QualityPreset preset)
    {
        if (instance != null)
        {
            instance.ApplyPreset(preset);
        }
    }
    
    /// <summary>
    /// Get current instance
    /// </summary>
    public static QualityManager Instance => instance;
}
