using UnityEngine;
using UnityEngine.UI;

public class VR_Vignette : MonoBehaviour
{
    [Header("RCCP Reference")]
    [SerializeField] private RCCP_CarController carController;

    [Header("UI Settings")]
    [SerializeField] private Image vignetteImage;

    [Header("Comfort Settings")]
    [SerializeField] private float triggerAngle = 10f;

    [Range(0f, 1f)]
    [SerializeField] private float maxIntensity = 0.65f;

    [Header("Timing (Debounce) Settings")]
    [SerializeField] private float minDuration = 0.5f;
    [SerializeField] private float exitDelay = 0.2f;
    [SerializeField] private float fadeSpeed = 5f;

    private bool isVignetteActive = false;
    private float activeTimer = 0f;
    private float straightTimer = 0f;
    private float lastLoggedAlpha = -1f;

    private void Start()
    {
        if (vignetteImage != null)
        {
            Color c = vignetteImage.color;
            c.a = 0f;
            vignetteImage.color = c;
        }

        if (carController == null)
            carController = FindFirstObjectByType<RCCP_CarController>();
    }

    private void LateUpdate()
    {
        if (carController == null || vignetteImage == null) return;

        float currentSteer = Mathf.Abs(carController.steerAngle);

        if (currentSteer > triggerAngle)
        {
            isVignetteActive = true;
            activeTimer = minDuration;
            straightTimer = 0f;
        }
        else
        {
            if (activeTimer > 0) activeTimer -= Time.deltaTime;
            straightTimer += Time.deltaTime;

            if (activeTimer <= 0 && straightTimer >= exitDelay)
            {
                isVignetteActive = false;
            }
        }

        float targetAlpha = isVignetteActive ? maxIntensity : 0f;

        Color color = vignetteImage.color;
        color.a = Mathf.Lerp(color.a, targetAlpha, Time.deltaTime * fadeSpeed);

        if (color.a < 0.005f) color.a = 0f;
        if (color.a > maxIntensity - 0.005f) color.a = maxIntensity;

        vignetteImage.color = color;

        if (Mathf.Abs(color.a - lastLoggedAlpha) > 0.01f)
        {
            Debug.Log($"[Vignette Alpha]: {color.a:F2}");
            lastLoggedAlpha = color.a;
        }
    }
}