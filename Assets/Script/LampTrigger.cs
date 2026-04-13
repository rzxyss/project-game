using UnityEngine;
using System.Collections;

public class LampTrigger : MonoBehaviour
{
    [Header("References")]
    public Light pointLight;
    public Renderer lampHeadRenderer;

    [Header("Settings")]
    public float detectionRadius = 5f;
    public float fadeDuration = 0.5f;
    public string playerTag = "Player";

    // Simpan nilai intensity asli saat malam
    private float originalIntensity;
    private bool isOff = false;
    private bool isTransitioning = false;

    // Material emission
    private Material lampMaterial;
    private Color originalEmissionColor;

    void Start()
    {
        originalIntensity = pointLight.intensity;

        // Ambil material dari lamphead untuk matikan emissionnya
        if (lampHeadRenderer != null)
        {
            // Instance material agar tidak affect semua lampu sekaligus
            lampMaterial = lampHeadRenderer.material;
            originalEmissionColor = lampMaterial.GetColor("_EmissionColor");
        }

        // Buat SphereCollider sebagai trigger otomatis
        SphereCollider col = gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = detectionRadius;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !isOff && !isTransitioning)
        {
            StartCoroutine(TurnOff());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag) && isOff && !isTransitioning)
        {
            StartCoroutine(TurnOn());
        }
    }

    IEnumerator TurnOff()
    {
        isTransitioning = true;
        isOff = true;

        float elapsed = 0f;
        float startIntensity = pointLight.intensity;
        Color startEmission = lampMaterial != null
            ? lampMaterial.GetColor("_EmissionColor")
            : Color.black;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            float smooth = t * t * (3f - 2f * t);

            pointLight.intensity = Mathf.Lerp(startIntensity, 0f, smooth);

            if (lampMaterial != null)
                lampMaterial.SetColor("_EmissionColor",
                    Color.Lerp(startEmission, Color.black, smooth));

            yield return null;
        }

        pointLight.intensity = 0f;
        if (lampMaterial != null)
            lampMaterial.SetColor("_EmissionColor", Color.black);

        isTransitioning = false;
    }

    IEnumerator TurnOn()
    {
        isTransitioning = true;
        isOff = false;

        float elapsed = 0f;
        float startIntensity = pointLight.intensity;
        Color startEmission = lampMaterial != null
            ? lampMaterial.GetColor("_EmissionColor")
            : Color.black;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            float smooth = t * t * (3f - 2f * t);

            pointLight.intensity = Mathf.Lerp(startIntensity, originalIntensity, smooth);

            if (lampMaterial != null)
                lampMaterial.SetColor("_EmissionColor",
                    Color.Lerp(startEmission, originalEmissionColor, smooth));

            yield return null;
        }

        pointLight.intensity = originalIntensity;
        if (lampMaterial != null)
            lampMaterial.SetColor("_EmissionColor", originalEmissionColor);

        isTransitioning = false;
    }

    // Dipanggil dari StreetLampController saat mode malam/siang
    public void SetIntensity(float intensity)
    {
        originalIntensity = intensity;
        if (!isOff)
            pointLight.intensity = intensity;
    }

    // Visualisasi radius di Scene View
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}