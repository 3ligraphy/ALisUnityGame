using UnityEngine;

[RequireComponent(typeof(Light))]
public class HimalayanSaltLampFlicker : MonoBehaviour
{
    [Header("Base Light Settings")]
    public float baseIntensity = 1.8f;       // الشدة الأساسية
    public Color baseColor = new Color(1.0f, 0.55f, 0.35f); // لون دافئ

    [Header("Flicker")]
    public float flickerAmplitude = 0.35f;   // مدى الارتعاش
    public float flickerSpeed = 1.2f;        // سرعة الارتعاش الناعم
    public float flutterAmplitude = 0.12f;   // ارتعاش صغير سريع
    public float flutterSpeed = 9.0f;        // سرعة الارتعاش الصغير

    [Header("Color Warmth Shift")]
    public float colorShiftAmount = 0.05f;   // انزياح بسيط في اللون

    [Header("Material Emission (Optional)")]
    public Renderer targetRenderer;          // Renderer لمصباح الملح
    [ColorUsage(true, true)] public Color emissionColor = new Color(1.0f, 0.55f, 0.35f);
    public float emissionBase = 2.5f;
    public float emissionScale = 1.2f;

    private Light lampLight;
    private float timeOffset;

    void Awake()
    {
        lampLight = GetComponent<Light>();
        timeOffset = Random.Range(0f, 1000f); // توقيت مختلف لكل مصباح
        lampLight.color = baseColor;
    }

    void Update()
    {
        float t = Time.time + timeOffset;

        float softFlicker = Mathf.PerlinNoise(t * flickerSpeed, 0f) * 2f - 1f; // -1..1
        float flutter = Mathf.PerlinNoise(0f, t * flutterSpeed) * 2f - 1f;     // -1..1

        float intensityOffset = softFlicker * flickerAmplitude + flutter * flutterAmplitude;
        float currentIntensity = Mathf.Max(0.1f, baseIntensity + intensityOffset);

        lampLight.intensity = currentIntensity;

        // انزياح دافئ في اللون مع الشدة
        float shift = colorShiftAmount * intensityOffset;
        Color warmShift = new Color(shift * 0.3f, 0f, -shift * 0.2f);
        lampLight.color = baseColor + warmShift;

        // ربط الانبعاث بالمادة
        if (targetRenderer != null)
        {
            float emission = Mathf.Max(0.1f, emissionBase + intensityOffset * emissionScale);
            foreach (var mat in targetRenderer.materials)
            {
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", emissionColor * emission);
                }
            }
        }
    }
}
