using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class DayToggle : MonoBehaviour
{
    [Header("References")]
    public Light directionalLight;

    [Header("Skybox Materials")]
    public Material skyboxDay;
    public Material skyboxNight;

    [Header("Day Settings")]
    public Color dayLightColor = new Color(1f, 0.95f, 0.8f);
    public float dayLightIntensity = 1.2f;
    public Color daySkyColor = new Color(0.5f, 0.7f, 1f);
    public Color dayEquatorColor = new Color(0.4f, 0.5f, 0.6f);
    public Color dayGroundColor = new Color(0.2f, 0.2f, 0.1f);

    [Header("Night Settings")]
    public Color nightLightColor = new Color(0.1f, 0.1f, 0.3f);
    public float nightLightIntensity = 0.05f;
    public Color nightSkyColor = new Color(0.01f, 0.01f, 0.05f);
    public Color nightEquatorColor = new Color(0.01f, 0.01f, 0.03f);
    public Color nightGroundColor = new Color(0f, 0f, 0f);

    [Header("Settings")]
    public Key toggleKey = Key.F7;
    public float transitionDuration = 3f;

    // Untuk menyimpan property ID skybox agar bisa di-lerp
    private static readonly int SkyTintID = Shader.PropertyToID("_SkyTint");
    private static readonly int AtmosphereThicknessID = Shader.PropertyToID("_AtmosphereThickness");
    private static readonly int ExposureID = Shader.PropertyToID("_Exposure");
    private static readonly int SunSizeID = Shader.PropertyToID("_SunSize");

    private bool isNight = false;
    private bool isTransitioning = false;

    void Start()
    {
        // Set skybox awal ke siang
        if (skyboxDay != null)
            RenderSettings.skybox = skyboxDay;
    }

    void Update()
    {
        if (Keyboard.current[toggleKey].wasPressedThisFrame && !isTransitioning)
        {
            if (isNight)
                StartCoroutine(TransitionToDay());
            else
                StartCoroutine(TransitionToNight());
        }
    }

    IEnumerator TransitionToNight()
    {
        isTransitioning = true;
        isNight = true;
        Debug.Log("Beralih ke malam...");

        // Ganti skybox di awal transisi
        if (skyboxNight != null)
            RenderSettings.skybox = skyboxNight;

        // Simpan nilai awal dari skybox malam untuk di-lerp dari nilai siang
        Color startSkyTint = skyboxDay != null
            ? skyboxDay.GetColor(SkyTintID)
            : Color.white;
        float startAtmosphere = skyboxDay != null
            ? skyboxDay.GetFloat(AtmosphereThicknessID)
            : 1f;
        float startExposure = skyboxDay != null
            ? skyboxDay.GetFloat(ExposureID)
            : 1f;
        float startSunSize = skyboxDay != null
            ? skyboxDay.GetFloat(SunSizeID)
            : 0.04f;

        Color targetSkyTint = skyboxNight != null
            ? skyboxNight.GetColor(SkyTintID)
            : Color.black;
        float targetAtmosphere = skyboxNight != null
            ? skyboxNight.GetFloat(AtmosphereThicknessID)
            : 0.3f;
        float targetExposure = skyboxNight != null
            ? skyboxNight.GetFloat(ExposureID)
            : 0.3f;
        float targetSunSize = skyboxNight != null
            ? skyboxNight.GetFloat(SunSizeID)
            : 0f;

        float elapsed = 0f;
        Color startLightColor = directionalLight.color;
        float startLightIntensity = directionalLight.intensity;
        Color startAmbientSky = RenderSettings.ambientSkyColor;
        Color startAmbientEq = RenderSettings.ambientEquatorColor;
        Color startAmbientGround = RenderSettings.ambientGroundColor;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            float smooth = t * t * (3f - 2f * t);

            // Lerp Directional Light
            directionalLight.color = Color.Lerp(startLightColor, nightLightColor, smooth);
            directionalLight.intensity = Mathf.Lerp(startLightIntensity, nightLightIntensity, smooth);

            // Lerp Ambient
            RenderSettings.ambientSkyColor = Color.Lerp(startAmbientSky, nightSkyColor, smooth);
            RenderSettings.ambientEquatorColor = Color.Lerp(startAmbientEq, nightEquatorColor, smooth);
            RenderSettings.ambientGroundColor = Color.Lerp(startAmbientGround, nightGroundColor, smooth);

            // Lerp property skybox material malam secara bertahap
            if (skyboxNight != null)
            {
                skyboxNight.SetColor(SkyTintID,
                    Color.Lerp(startSkyTint, targetSkyTint, smooth));
                skyboxNight.SetFloat(AtmosphereThicknessID,
                    Mathf.Lerp(startAtmosphere, targetAtmosphere, smooth));
                skyboxNight.SetFloat(ExposureID,
                    Mathf.Lerp(startExposure, targetExposure, smooth));
                skyboxNight.SetFloat(SunSizeID,
                    Mathf.Lerp(startSunSize, targetSunSize, smooth));
            }

            DynamicGI.UpdateEnvironment();
            yield return null;
        }

        // Pastikan nilai akhir tepat
        directionalLight.color = nightLightColor;
        directionalLight.intensity = nightLightIntensity;
        RenderSettings.ambientSkyColor = nightSkyColor;
        RenderSettings.ambientEquatorColor = nightEquatorColor;
        RenderSettings.ambientGroundColor = nightGroundColor;

        isTransitioning = false;
        Debug.Log("Mode malam aktif");
    }

    IEnumerator TransitionToDay()
    {
        isTransitioning = true;
        isNight = false;
        Debug.Log("Beralih ke siang...");

        // Ganti skybox di awal transisi
        if (skyboxDay != null)
            RenderSettings.skybox = skyboxDay;

        Color startSkyTint = skyboxNight != null
            ? skyboxNight.GetColor(SkyTintID)
            : Color.black;
        float startAtmosphere = skyboxNight != null
            ? skyboxNight.GetFloat(AtmosphereThicknessID)
            : 0.3f;
        float startExposure = skyboxNight != null
            ? skyboxNight.GetFloat(ExposureID)
            : 0.3f;
        float startSunSize = skyboxNight != null
            ? skyboxNight.GetFloat(SunSizeID)
            : 0f;

        Color targetSkyTint = skyboxDay != null
            ? skyboxDay.GetColor(SkyTintID)
            : new Color(0.5f, 0.7f, 1f);
        float targetAtmosphere = skyboxDay != null
            ? skyboxDay.GetFloat(AtmosphereThicknessID)
            : 1f;
        float targetExposure = skyboxDay != null
            ? skyboxDay.GetFloat(ExposureID)
            : 1.3f;
        float targetSunSize = skyboxDay != null
            ? skyboxDay.GetFloat(SunSizeID)
            : 0.04f;

        float elapsed = 0f;
        Color startLightColor = directionalLight.color;
        float startLightIntensity = directionalLight.intensity;
        Color startAmbientSky = RenderSettings.ambientSkyColor;
        Color startAmbientEq = RenderSettings.ambientEquatorColor;
        Color startAmbientGround = RenderSettings.ambientGroundColor;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            float smooth = t * t * (3f - 2f * t);

            directionalLight.color = Color.Lerp(startLightColor, dayLightColor, smooth);
            directionalLight.intensity = Mathf.Lerp(startLightIntensity, dayLightIntensity, smooth);

            RenderSettings.ambientSkyColor = Color.Lerp(startAmbientSky, daySkyColor, smooth);
            RenderSettings.ambientEquatorColor = Color.Lerp(startAmbientEq, dayEquatorColor, smooth);
            RenderSettings.ambientGroundColor = Color.Lerp(startAmbientGround, dayGroundColor, smooth);

            if (skyboxDay != null)
            {
                skyboxDay.SetColor(SkyTintID,
                    Color.Lerp(startSkyTint, targetSkyTint, smooth));
                skyboxDay.SetFloat(AtmosphereThicknessID,
                    Mathf.Lerp(startAtmosphere, targetAtmosphere, smooth));
                skyboxDay.SetFloat(ExposureID,
                    Mathf.Lerp(startExposure, targetExposure, smooth));
                skyboxDay.SetFloat(SunSizeID,
                    Mathf.Lerp(startSunSize, targetSunSize, smooth));
            }

            DynamicGI.UpdateEnvironment();
            yield return null;
        }

        directionalLight.color = dayLightColor;
        directionalLight.intensity = dayLightIntensity;
        RenderSettings.ambientSkyColor = daySkyColor;
        RenderSettings.ambientEquatorColor = dayEquatorColor;
        RenderSettings.ambientGroundColor = dayGroundColor;

        isTransitioning = false;
        Debug.Log("Mode siang aktif");
    }
}