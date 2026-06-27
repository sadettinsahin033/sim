using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class MotorcycleTelemetryLogger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ArduinoMotorController motorController;
    [SerializeField] private Rigidbody rb;

    [Header("Logging Settings")]
    [SerializeField] private bool logOnStart = true;
    [SerializeField] private float sampleRate = 20f;
    [SerializeField] private string fileNamePrefix = "motorcycle_telemetry";

    [Header("Save Location")]
    [SerializeField] private bool saveInsideAssetsFolder = true;
    [SerializeField] private string assetsSubFolderName = "TelemetryLogs";

    [Header("Controls")]
    [SerializeField] private bool toggleWithLKey = true;

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

        if (motorController == null)
            Debug.LogError("TelemetryLogger: ArduinoMotorController bulunamadý. Logger MotorRoot üzerinde olmalý veya referans atanmalý.");

        if (rb == null)
            Debug.LogError("TelemetryLogger: Rigidbody bulunamadý. Logger MotorRoot üzerinde olmalý veya RB referansý atanmalý.");
    }

    private void Start()
    {
        if (logOnStart)
            StartLogging();
    }

    private void Update()
    {
        if (toggleWithLKey && Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame)
        {
            if (isLogging)
                StopLogging();
            else
                StartLogging();
        }

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
        {
            Debug.LogWarning("TelemetryLogger: Zaten kayýt yapýyor.");
            return;
        }

        if (motorController == null || rb == null)
        {
            Debug.LogError("TelemetryLogger: Kayýt baþlatýlamadý. MotorController veya Rigidbody eksik.");
            return;
        }

        try
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"{fileNamePrefix}_{timestamp}.csv";

            string folderPath;

            if (saveInsideAssetsFolder)
            {
                folderPath = Path.Combine(Application.dataPath, assetsSubFolderName);
            }
            else
            {
                folderPath = Path.Combine(Application.persistentDataPath, assetsSubFolderName);
            }

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            filePath = Path.Combine(folderPath, fileName);

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

            writer.Flush();

            float now = Time.time;

            lastTime = now;
            lastSpeed = GetSpeed();
            lastSteerAngle = motorController.VisualSteerAngle;
            lastYaw = GetYaw();

            isLogging = true;

            Debug.Log("========================================");
            Debug.Log($"TelemetryLogger BAÞLADI");
            Debug.Log($"CSV yolu: {filePath}");
            Debug.Log("L tuþu ile kaydý durdurup baþlatabilirsin.");
            Debug.Log("========================================");
        }
        catch (Exception exception)
        {
            Debug.LogError($"TelemetryLogger: Dosya oluþturulamadý. Hata: {exception.Message}");
        }
    }

    public void StopLogging()
    {
        if (!isLogging && writer == null)
            return;

        isLogging = false;

        if (writer != null)
        {
            writer.Flush();
            writer.Close();
            writer = null;
        }

        Debug.Log("========================================");
        Debug.Log($"TelemetryLogger DURDU");
        Debug.Log($"CSV yolu: {filePath}");
        Debug.Log("========================================");
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

        bool grounded = motorController.IsGrounded;

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
            BoolToInt(grounded) + "," +
            BoolToInt(harshBrake) + "," +
            BoolToInt(harshSteer) + "," +
            BoolToInt(highSideSlip) + "," +
            BoolToInt(highYawRate) + "," +
            Format(riskScore)
        );

        writer.Flush();

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