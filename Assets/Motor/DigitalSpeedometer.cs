using TMPro;
using UnityEngine;

public class DigitalSpeedometer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ArduinoMotorController motorController;
    [SerializeField] private TextMeshProUGUI speedText;

    [Header("Display Settings")]
    [SerializeField] private bool showKmh = true;
    [SerializeField] private string suffixKmh = " km/h";
    [SerializeField] private string suffixMs = " m/s";
    [SerializeField] private int decimalCount = 0;

    [Header("Smoothing")]
    [SerializeField] private bool smoothDisplay = true;
    [SerializeField] private float smoothSpeed = 8f;

    private float displayedSpeed;

    private void Awake()
    {
        if (motorController == null)
            motorController = FindAnyObjectByType<ArduinoMotorController>();

        if (speedText == null)
            speedText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (motorController == null || speedText == null)
            return;

        float rawSpeed = motorController.CurrentSpeed;

        float targetSpeed = showKmh
            ? rawSpeed * 3.6f
            : rawSpeed;

        if (smoothDisplay)
        {
            displayedSpeed = Mathf.Lerp(
                displayedSpeed,
                targetSpeed,
                smoothSpeed * Time.deltaTime
            );
        }
        else
        {
            displayedSpeed = targetSpeed;
        }

        string suffix = showKmh ? suffixKmh : suffixMs;

        speedText.text = displayedSpeed.ToString($"F{decimalCount}") + suffix;
    }
}