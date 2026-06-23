using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public class MotorcycleTelemetryLogger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ArduinoMotorController motorController;
    [SerializeField] private Rigidbody rb;

    [Header("Logging Settings")]
    [SerializeField] private bool logOnStart = true;
    [SerializeField] private float sampleRate = 20f;
    [SerializeField] private string fileNamePrefix = "motorcycle_telemetry";

    [Header("Risk Thresholds")]
    [SerializeField] private float harshBrakeThreshold = 0.75f;
    [SerializeField] private float harshSteerRateThreshold = 120f;
    [SerializeField] private float highSideSlipThreshold = 1.2f;
    [SerializeField] private float highYawRateThreshold = 90f;

    private bool isLogging;
    private float timer;
    private float sampleInterval;

    private string filePath;
    private StreamWriter writer;

    private float lastSpeed;
    private float lastSteerAngle;
    private float lastYaw;
    private float lastTime;

    private readonly CultureInfo culture = CultureInfo.InvariantCulture;

    private void Awake()
    {
        if (motorController == null)
            motorController = GetComponent<ArduinoMotorController>();

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        sampleInterval = 1f / Mathf.Max(1f, sampleRate);
    }

    private void Start()
    {
        if (logOnStart)
            StartLogging();
    }

    private void Update()
    {
        if (!isLogging)
            return;

        timer += Time.deltaTime;

        if (timer < sampleInterval)
            return;

        timer = 0f;
        WriteSample();
    }

    private void OnDestroy()
    {
        StopLogging();
    }

    private void OnApplicationQuit()
    {
        StopLogging();
    }

    public void StartLogging()
    {
        if (isLogging)
            return;

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{fileNamePrefix}_{timestamp}.csv";

        filePath = Path.Combine(Application.persistentDataPath, fileName);

        writer = new StreamWriter(filePath, false, Encoding.UTF8);

        writer.WriteLine(
            "time," +
            "position_x,position_y,position_z," +
            "speed_ms,speed_kmh,forward_speed,sideways_speed," +
            "acceleration_ms2," +
            "yaw,yaw_rate," +
            "raw_steer_x,raw_throttle_y,raw_brake_z," +
            "steer_input,handlebar_angle,steer_rate," +
            "throttle,brake," +
            "is_grounded," +
            "harsh_brake,harsh_steer,high_side_slip,high_yaw_rate," +
            "risk_score"
        );

        float now = Time.time;

        lastTime = now;
        lastSpeed = GetSpeed();
        lastSteerAngle = motorController != null ? motorController.VisualSteerAngle : 0f;
        lastYaw = GetYaw();

        isLogging = true;

        Debug.Log($"Telemetry logging started: {filePath}");
    }

    public void StopLogging()
    {
        if (!isLogging)
            return;

        isLogging = false;

        if (writer != null)
        {
            writer.Flush();
            writer.Close();
            writer = null;
        }

        Debug.Log($"Telemetry logging stopped: {filePath}");
    }

    private void WriteSample()
    {
        if (writer == null || motorController == null || rb == null)
            return;

        float now = Time.time;
        float deltaTime = Mathf.Max(0.0001f, now - lastTime);

        Vector3 position = transform.position;

        Vector3 horizontalVelocity = motorController.CurrentHorizontalVelocity;
        Vector3 moveDirection = motorController.CurrentMoveDirection.normalized;

        float speed = horizontalVelocity.magnitude;
        float speedKmh = speed * 3.6f;

        float forwardSpeed = Vector3.Dot(horizontalVelocity, moveDirection);

        Vector3 forwardVelocity = moveDirection * forwardSpeed;
        Vector3 sidewaysVelocity = horizontalVelocity - forwardVelocity;
        float sidewaysSpeed = sidewaysVelocity.magnitude;

        float acceleration = (speed - lastSpeed) / deltaTime;

        float yaw = GetYaw();
        float yawDelta = Mathf.DeltaAngle(lastYaw, yaw);
        float yawRate = yawDelta / deltaTime;

        float handlebarAngle = motorController.VisualSteerAngle;
        float steerRate = Mathf.Abs(handlebarAngle - lastSteerAngle) / deltaTime;

        float rawX = motorController.RawX;
        float rawY = motorController.RawY;
        float rawZ = motorController.RawZ;

        float steerInput = motorController.SteerInput;
        float throttle = motorController.ThrottleInput;
        float brake = motorController.BrakeInput;

        bool isGrounded = motorController.IsGrounded;

        bool harshBrake = brake >= harshBrakeThreshold && speed > 1f;
        bool harshSteer = steerRate >= harshSteerRateThreshold && speed > 1f;
        bool highSideSlip = sidewaysSpeed >= highSideSlipThreshold && speed > 1f;
        bool highYawRate = Mathf.Abs(yawRate) >= highYawRateThreshold && speed > 1f;

        float riskScore = CalculateRiskScore(
            harshBrake,
            harshSteer,
            highSideSlip,
            highYawRate,
            sidewaysSpeed,
            speed,
            steerRate,
            yawRate,
            brake
        );

        writer.WriteLine(
            Format(now) + "," +
            Format(position.x) + "," +
            Format(position.y) + "," +
            Format(position.z) + "," +
            Format(speed) + "," +
            Format(speedKmh) + "," +
            Format(forwardSpeed) + "," +
            Format(sidewaysSpeed) + "," +
            Format(acceleration) + "," +
            Format(yaw) + "," +
            Format(yawRate) + "," +
            Format(rawX) + "," +
            Format(rawY) + "," +
            Format(rawZ) + "," +
            Format(steerInput) + "," +
            Format(handlebarAngle) + "," +
            Format(steerRate) + "," +
            Format(throttle) + "," +
            Format(brake) + "," +
            BoolToInt(isGrounded) + "," +
            BoolToInt(harshBrake) + "," +
            BoolToInt(harshSteer) + "," +
            BoolToInt(highSideSlip) + "," +
            BoolToInt(highYawRate) + "," +
            Format(riskScore)
        );

        lastTime = now;
        lastSpeed = speed;
        lastSteerAngle = handlebarAngle;
        lastYaw = yaw;
    }

    private float CalculateRiskScore(
        bool harshBrake,
        bool harshSteer,
        bool highSideSlip,
        bool highYawRate,
        float sidewaysSpeed,
        float speed,
        float steerRate,
        float yawRate,
        float brake
    )
    {
        float score = 0f;

        if (harshBrake)
            score += 20f;

        if (harshSteer)
            score += 20f;

        if (highSideSlip)
            score += 25f;

        if (highYawRate)
            score += 20f;

        if (speed > 0.5f)
        {
            float slipRatio = sidewaysSpeed / speed;
            score += Mathf.Clamp01(slipRatio) * 15f;
        }

        score += Mathf.Clamp01(steerRate / 180f) * 10f;
        score += Mathf.Clamp01(Mathf.Abs(yawRate) / 180f) * 10f;
        score += Mathf.Clamp01(brake) * 5f;

        return Mathf.Clamp(score, 0f, 100f);
    }

    private float GetSpeed()
    {
        if (motorController == null)
            return 0f;

        return motorController.CurrentSpeed;
    }

    private float GetYaw()
    {
        if (rb != null)
            return rb.rotation.eulerAngles.y;

        return transform.eulerAngles.y;
    }

    private string Format(float value)
    {
        return value.ToString("0.0000", culture);
    }

    private int BoolToInt(bool value)
    {
        return value ? 1 : 0;
    }
}