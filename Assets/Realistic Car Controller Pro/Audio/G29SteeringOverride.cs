using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class G29SteeringOverride : MonoBehaviour
{
    [Header("RCCP")]
    public RCCP_Input rccpInput;

    [Header("Wheel (auto bulunur)")]
    public Joystick wheel;

    [Header("Steering Tuning")]
    [Range(0f, 0.05f)] public float deadzone = 0.0f;
    [Range(0.1f, 5f)] public float steerSensitivity = 2.0f;
    [Range(0.1f, 2f)] public float lowEndBoost = 0.65f;
    public bool invert = false;

    [Header("Steering Debug")]
    public float steerRaw;
    public float steerOutput;

    [Header("Pedals")]
    public string throttleControl = "z";
    public string brakeControl = "rz";

    public float throttleReleased = 1f;
    public float throttlePressed = 0f;

    public float brakeReleased = 1f;
    public float brakePressed = 0f;

    [Header("Pedal Tuning")]
    [Range(0f, 0.2f)] public float throttleDeadzone = 0f;
    [Range(0.1f, 5f)] public float throttleSensitivity = 1f;

    [Range(0f, 0.2f)] public float brakeDeadzone = 0f;
    [Range(0.1f, 5f)] public float brakeSensitivity = 1f;

    [Header("Pedal Debug")]
    public float throttleRaw;
    public float throttleOutput;
    public float brakeRaw;
    public float brakeOutput;

    [Header("Center Calibration")]
    public bool calibrateOnStart = true;
    public float centerOffset = 0f;

    private RCCP_Inputs inputs = new RCCP_Inputs();

    void Start()
    {
        if (rccpInput == null)
            rccpInput = GetComponent<RCCP_Input>();

        if (wheel == null)
        {
            foreach (var j in Joystick.all)
            {
                if (j == null) continue;

                string display = j.displayName != null ? j.displayName.ToLower() : "";
                string devName = j.name != null ? j.name.ToLower() : "";

                if (display.Contains("g29") || display.Contains("logitech") ||
                    devName.Contains("g29") || devName.Contains("logitech"))
                {
                    wheel = j;
                    Debug.Log("[G29Override] Wheel bulundu: " + j.displayName);
                    break;
                }
            }
        }

        if (wheel == null)
        {
            Debug.LogError("[G29Override] G29 bulunamadý!");
            return;
        }

        if (calibrateOnStart)
        {
            centerOffset = ReadRawSteering();
            Debug.Log("[G29Override] Center offset calibrated: " + centerOffset);
        }
    }

    void Update()
    {
        if (rccpInput == null || wheel == null)
            return;

        float steer = ReadRawSteering();

        steerRaw = steer;

        steer -= centerOffset;

        if (invert)
            steer = -steer;

        steer = Mathf.Clamp(steer, -1f, 1f);

        if (Mathf.Abs(steer) < deadzone)
        {
            steer = 0f;
        }
        else
        {
            float sign = Mathf.Sign(steer);
            float magnitude = Mathf.InverseLerp(deadzone, 1f, Mathf.Abs(steer));

            magnitude = Mathf.Pow(magnitude, lowEndBoost);

            steer = sign * magnitude;
        }

        steer *= steerSensitivity;
        steer = Mathf.Clamp(steer, -1f, 1f);

        steerOutput = steer;

        inputs.throttleInput = ReadPedal(
            throttleControl,
            throttleReleased,
            throttlePressed,
            throttleDeadzone,
            throttleSensitivity,
            out throttleRaw,
            out throttleOutput
        );

        inputs.brakeInput = ReadPedal(
            brakeControl,
            brakeReleased,
            brakePressed,
            brakeDeadzone,
            brakeSensitivity,
            out brakeRaw,
            out brakeOutput
        );

        inputs.handbrakeInput = 0f;
        inputs.clutchInput = 0f;

        inputs.steerInput = steer;

        rccpInput.OverrideInputs(inputs);
    }

    float ReadRawSteering()
    {
        float value = wheel.stick.x.ReadValue();

        InputControl control = wheel["stick/x"];
        if (control is AxisControl axis)
        {
            float raw = axis.ReadUnprocessedValue();
            if (Mathf.Abs(raw) > Mathf.Abs(value))
                value = raw;
        }

        return value;
    }

    float ReadPedal(
        string controlName,
        float releasedValue,
        float pressedValue,
        float pedalDeadzone,
        float pedalSensitivity,
        out float rawValue,
        out float outputValue
    )
    {
        rawValue = 0f;
        outputValue = 0f;

        InputControl control = wheel[controlName];

        if (control is AxisControl axis)
        {
            rawValue = axis.ReadValue();

            float t = Mathf.InverseLerp(releasedValue, pressedValue, rawValue);
            t = Mathf.Clamp01(t);

            if (t < pedalDeadzone)
            {
                t = 0f;
            }
            else
            {
                t = Mathf.InverseLerp(pedalDeadzone, 1f, t);
            }

            t *= pedalSensitivity;
            outputValue = Mathf.Clamp01(t);

            return outputValue;
        }

        return 0f;
    }
}