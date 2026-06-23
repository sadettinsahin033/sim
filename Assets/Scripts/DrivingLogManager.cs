using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public class DrivingLogManager : MonoBehaviour
{
    [System.Serializable]
    public class GeneralSettings
    {
        [Tooltip("Log sistemini tamamen açar veya kapatýr.")]
        [InspectorName("Loglama Aktif")]
        public bool loggingActive = true;

        [Tooltip("CSV dosyasýna kayýt yazýlsýn mý? Kapalýysa sadece Console log kullanýlabilir.")]
        [InspectorName("CSV Dosyasýna Yaz")]
        public bool writeToCsv = true;

        [Tooltip("Unity Console'a log yazýlsýn mý?")]
        [InspectorName("Console'da Göster")]
        public bool showConsoleLog = true;

        [Tooltip("Açýksa sürekli sürüþ verisi Console'a yazýlmaz, sadece event loglarý görünür. CSV kaydý etkilenmez.")]
        [InspectorName("Console'da Sadece Olay Loglarýný Göster")]
        public bool showOnlyEventLogsInConsole = true;

        [Tooltip("Açýksa her Play baþlatýldýðýnda yeni CSV dosyasý oluþturulur.")]
        [InspectorName("Her Çalýþtýrmada Yeni Dosya Oluþtur")]
        public bool createNewFileEachRun = true;

        [Tooltip("CSV dosyasýnýn adý. Her çalýþtýrmada yeni dosya açýksa sonuna tarih/saat eklenir.")]
        [InspectorName("CSV Dosya Adý")]
        public string csvFileName = "driving_log.csv";

        [Tooltip("CSV dosyalarýnýn kaydedileceði klasör adý. Application.persistentDataPath içine oluþturulur.")]
        [InspectorName("CSV Klasör Adý")]
        public string csvFolderName = "DrivingLogs";

        [Tooltip("Bu oturumun/senaryonun ID bilgisidir. CSV'de session_id alanýna yazýlýr.")]
        [InspectorName("Session / Senaryo ID")]
        public string sessionId = "Senaryo_2_1";
    }

    [System.Serializable]
    public class VehicleReferenceSettings
    {
        [Tooltip("Aracýn Rigidbody bileþeni. Boþ býrakýlýrsa bu objede veya parent objede aranýr.")]
        [InspectorName("Araç Rigidbody")]
        public Rigidbody carRigidbody;

        [Tooltip("G29SteeringOverride scripti. Boþ býrakýlýrsa bu objede veya parent objede aranýr.")]
        [InspectorName("G29 Input Script")]
        public G29SteeringOverride g29Input;

        [Tooltip("Açýksa G29 scriptinden gaz, fren ve direksiyon deðerleri alýnýr. Kapalýysa klavye inputlarý kullanýlýr.")]
        [InspectorName("G29 Deðerlerini Kullan")]
        public bool useG29Input = true;
    }

    [System.Serializable]
    public class ContinuousLogSettings
    {
        [Tooltip("Açýksa belirli aralýklarla sürekli sürüþ verisi CSV'ye yazýlýr.")]
        [InspectorName("Sürekli Kayýt Al")]
        public bool continuousLogging = false;

        [Tooltip("Sürekli kayýt açýksa kaç saniyede bir kayýt alýnacaðýný belirler.")]
        [InspectorName("Sürekli Kayýt Aralýðý")]
        public float continuousLogInterval = 0.25f;
    }

    [System.Serializable]
    public class KeyboardFallbackSettings
    {
        [Tooltip("G29 yoksa veya kullanýlmýyorsa klavyeden gaz/fren için Vertical axis okunur.")]
        [InspectorName("Klavye Gaz Fren Kullan")]
        public bool useKeyboardGasBrake = true;

        [Tooltip("G29 yoksa veya kullanýlmýyorsa klavyeden direksiyon için Horizontal axis okunur.")]
        [InspectorName("Klavye Direksiyon Kullan")]
        public bool useKeyboardSteering = true;
    }

    [System.Serializable]
    public class SignalSettings
    {
        [Tooltip("Açýksa sinyal durumu G29 scriptinin baðlý olduðu RCCP araç ýþýklarýndan okunur. CSV'ye yazýlmaz, sadece ScenarioEventTrigger sinyal kontrolü için kullanýlýr.")]
        [InspectorName("RCCP Iþýklardan Sinyal Oku")]
        public bool readSignalFromRccpLights = true;

        [Tooltip("Test için klavyeden sol sinyal açma/kapama tuþu.")]
        [InspectorName("Sol Sinyal Tuþu")]
        public KeyCode leftSignalKey = KeyCode.Q;

        [Tooltip("Test için klavyeden sað sinyal açma/kapama tuþu.")]
        [InspectorName("Sað Sinyal Tuþu")]
        public KeyCode rightSignalKey = KeyCode.E;

        [Tooltip("Test için klavyeden dörtlü sinyal açma/kapama tuþu.")]
        [InspectorName("Dörtlü Sinyal Tuþu")]
        public KeyCode hazardSignalKey = KeyCode.R;
    }

    [Tooltip("Genel loglama ve CSV ayarlarý.")]
    [InspectorName("Genel Log Ayarlarý")]
    public GeneralSettings generalSettings = new GeneralSettings();

    [Tooltip("Araç Rigidbody ve G29 referans ayarlarý.")]
    [InspectorName("Araç Referans Ayarlarý")]
    public VehicleReferenceSettings vehicleSettings = new VehicleReferenceSettings();

    [Tooltip("Sürekli veri kaydý ayarlarý.")]
    [InspectorName("Sürekli Kayýt Ayarlarý")]
    public ContinuousLogSettings continuousSettings = new ContinuousLogSettings();

    [Tooltip("G29 yoksa kullanýlacak klavye yedek input ayarlarý.")]
    [InspectorName("Klavye Yedek Input Ayarlarý")]
    public KeyboardFallbackSettings keyboardSettings = new KeyboardFallbackSettings();

    [Tooltip("Sinyal durumu sadece event deðerlendirmesi için okunur. CSV'ye sinyal kolonu yazýlmaz.")]
    [InspectorName("Sinyal Okuma Ayarlarý")]
    public SignalSettings signalSettings = new SignalSettings();

    [Header("ANLIK SÜRÜÞ DEÐERLERÝ")]
    [InspectorName("Gaz Deðeri")]
    public float gasValue;

    [InspectorName("Fren Deðeri")]
    public float brakeValue;

    [InspectorName("Direksiyon Deðeri")]
    public float steeringAngle;

    [InspectorName("Anlýk Hýz km/h")]
    public float speedKmh;

    [Header("ANLIK SÝNYAL DURUMU - CSV'YE YAZILMAZ")]
    [InspectorName("Sol Sinyal Açýk")]
    public bool leftSignalOn;

    [InspectorName("Sað Sinyal Açýk")]
    public bool rightSignalOn;

    [InspectorName("Dörtlü Sinyal Açýk")]
    public bool hazardSignalOn;

    private string csvFilePath;
    private float continuousTimer;

    private const string ContinuousActionName = "Sürekli Sürüþ Verisi";

    private void Awake()
    {
        FindReferencesIfMissing();
        PrepareCsvFile();
    }

    private void Update()
    {
        ReadDrivingInputs();
        ReadSignalState();

        if (generalSettings.loggingActive && continuousSettings.continuousLogging)
        {
            continuousTimer += Time.deltaTime;

            if (continuousTimer >= continuousSettings.continuousLogInterval)
            {
                continuousTimer = 0f;
                WriteEventLog(ContinuousActionName, false, "Sürekli sürüþ verisi kaydý.");
            }
        }
    }

    private void FindReferencesIfMissing()
    {
        if (vehicleSettings.carRigidbody == null)
            vehicleSettings.carRigidbody = GetComponentInParent<Rigidbody>();

        if (vehicleSettings.carRigidbody == null)
            vehicleSettings.carRigidbody = GetComponent<Rigidbody>();

        if (vehicleSettings.g29Input == null)
            vehicleSettings.g29Input = GetComponentInParent<G29SteeringOverride>();

        if (vehicleSettings.g29Input == null)
            vehicleSettings.g29Input = GetComponent<G29SteeringOverride>();
    }

    private void PrepareCsvFile()
    {
        if (!generalSettings.writeToCsv)
            return;

        string folderPath = Path.Combine(Application.persistentDataPath, generalSettings.csvFolderName);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string fileName = generalSettings.csvFileName;

        if (string.IsNullOrWhiteSpace(fileName))
            fileName = "driving_log.csv";

        if (!fileName.EndsWith(".csv"))
            fileName += ".csv";

        if (generalSettings.createNewFileEachRun)
        {
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            fileName = nameWithoutExtension + "_" + timeStamp + extension;
        }

        csvFilePath = Path.Combine(folderPath, fileName);

        if (!File.Exists(csvFilePath))
            WriteCsvHeader();
    }

    private void WriteCsvHeader()
    {
        string header =
            "session_id;" +
            "timestamp;" +
            "delta_time;" +
            "pos_x;" +
            "pos_y;" +
            "pos_z;" +
            "rot_x;" +
            "rot_y;" +
            "rot_z;" +
            "speed_kmh;" +
            "gas_value;" +
            "brake_value;" +
            "steering_angle;" +
            "action;" +
            "is_error;" +
            "explanation";

        File.WriteAllText(csvFilePath, header + Environment.NewLine, new UTF8Encoding(true));
    }

    private void ReadDrivingInputs()
    {
        speedKmh = GetSpeedKmh();

        bool canUseG29 =
            vehicleSettings.useG29Input &&
            vehicleSettings.g29Input != null;

        if (canUseG29)
        {
            gasValue = Mathf.Clamp01(vehicleSettings.g29Input.throttleOutput);
            brakeValue = Mathf.Clamp01(vehicleSettings.g29Input.brakeOutput);
            steeringAngle = Mathf.Clamp(vehicleSettings.g29Input.steerOutput, -1f, 1f);
            return;
        }

        float keyboardGas = 0f;
        float keyboardBrake = 0f;
        float horizontal = 0f;

        if (keyboardSettings.useKeyboardGasBrake)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                keyboardGas = 1f;

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                keyboardBrake = 1f;
        }

        if (keyboardSettings.useKeyboardSteering)
        {
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                horizontal += 1f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                horizontal -= 1f;
        }

        gasValue = keyboardGas;
        brakeValue = keyboardBrake;
        steeringAngle = Mathf.Clamp(horizontal, -1f, 1f);
    }

    private void ReadSignalState()
    {
        bool readFromRccp = false;

        if (signalSettings.readSignalFromRccpLights &&
            vehicleSettings.g29Input != null &&
            vehicleSettings.g29Input.rccpInput != null &&
            vehicleSettings.g29Input.rccpInput.CarController != null &&
            vehicleSettings.g29Input.rccpInput.CarController.Lights != null)
        {
            var lights = vehicleSettings.g29Input.rccpInput.CarController.Lights;

            leftSignalOn = lights.indicatorsLeft;
            rightSignalOn = lights.indicatorsRight;
            hazardSignalOn = lights.indicatorsAll;

            readFromRccp = true;
        }

        if (!readFromRccp)
            ReadSignalFromKeyboardForTest();
    }

    private void ReadSignalFromKeyboardForTest()
    {
        if (Input.GetKeyDown(signalSettings.leftSignalKey))
        {
            leftSignalOn = !leftSignalOn;

            if (leftSignalOn)
            {
                rightSignalOn = false;
                hazardSignalOn = false;
            }
        }

        if (Input.GetKeyDown(signalSettings.rightSignalKey))
        {
            rightSignalOn = !rightSignalOn;

            if (rightSignalOn)
            {
                leftSignalOn = false;
                hazardSignalOn = false;
            }
        }

        if (Input.GetKeyDown(signalSettings.hazardSignalKey))
        {
            hazardSignalOn = !hazardSignalOn;

            if (hazardSignalOn)
            {
                leftSignalOn = false;
                rightSignalOn = false;
            }
        }
    }

    public float GetSpeedKmh()
    {
        if (vehicleSettings.carRigidbody == null)
            return 0f;

#if UNITY_6000_0_OR_NEWER
        return vehicleSettings.carRigidbody.linearVelocity.magnitude * 3.6f;
#else
        return vehicleSettings.carRigidbody.velocity.magnitude * 3.6f;
#endif
    }

    public void WriteEventLog(string action, bool isError, string explanation)
    {
        if (!generalSettings.loggingActive)
            return;

        if (string.IsNullOrWhiteSpace(action))
            action = "Bilinmeyen Olay";

        if (string.IsNullOrWhiteSpace(explanation))
            explanation = "";

        speedKmh = GetSpeedKmh();

        if (generalSettings.writeToCsv)
            AppendCsvLine(action, isError, explanation);

        if (generalSettings.showConsoleLog)
            WriteConsoleLog(action, isError, explanation);
    }

    private void AppendCsvLine(string action, bool isError, string explanation)
    {
        if (string.IsNullOrWhiteSpace(csvFilePath))
            PrepareCsvFile();

        if (string.IsNullOrWhiteSpace(csvFilePath))
            return;

        Vector3 position = transform.position;
        Vector3 rotation = transform.eulerAngles;

        string line =
            CleanCsv(generalSettings.sessionId) + ";" +
            CleanCsv(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")) + ";" +
            FloatToCsv(Time.time) + ";" +
            FloatToCsv(position.x) + ";" +
            FloatToCsv(position.y) + ";" +
            FloatToCsv(position.z) + ";" +
            FloatToCsv(rotation.x) + ";" +
            FloatToCsv(rotation.y) + ";" +
            FloatToCsv(rotation.z) + ";" +
            FloatToCsv(speedKmh) + ";" +
            FloatToCsv(gasValue) + ";" +
            FloatToCsv(brakeValue) + ";" +
            FloatToCsv(steeringAngle) + ";" +
            CleanCsv(action) + ";" +
            BoolToCsv(isError) + ";" +
            CleanCsv(explanation);

        File.AppendAllText(csvFilePath, line + Environment.NewLine, new UTF8Encoding(true));
    }

    private void WriteConsoleLog(string action, bool isError, string explanation)
    {
        if (generalSettings.showOnlyEventLogsInConsole && action == ContinuousActionName)
            return;

        string message =
            "[" + generalSettings.sessionId + "] " +
            action +
            " | Hata: " + isError +
            " | Hýz: " + speedKmh.ToString("F1", CultureInfo.InvariantCulture) + " km/h" +
            " | Gaz: " + gasValue.ToString("F2", CultureInfo.InvariantCulture) +
            " | Fren: " + brakeValue.ToString("F2", CultureInfo.InvariantCulture) +
            " | Direksiyon: " + steeringAngle.ToString("F2", CultureInfo.InvariantCulture) +
            " | Açýklama: " + explanation;

        if (isError)
            Debug.LogWarning(message);
        else
            Debug.Log(message);
    }

    public bool IsLeftSignalOn()
    {
        return leftSignalOn || hazardSignalOn;
    }

    public bool IsRightSignalOn()
    {
        return rightSignalOn || hazardSignalOn;
    }

    public bool IsAnySignalOn()
    {
        return leftSignalOn || rightSignalOn || hazardSignalOn;
    }

    public string GetSignalText()
    {
        if (hazardSignalOn)
            return "Dörtlü";

        if (leftSignalOn)
            return "Sol";

        if (rightSignalOn)
            return "Sað";

        return "Yok";
    }

    public string GetCsvFilePath()
    {
        return csvFilePath;
    }

    private string FloatToCsv(float value)
    {
        return value.ToString("F3", CultureInfo.InvariantCulture);
    }

    private string BoolToCsv(bool value)
    {
        return value ? "1" : "0";
    }

    private string CleanCsv(string value)
    {
        if (value == null)
            return "";

        value = value.Replace("\r", " ");
        value = value.Replace("\n", " ");
        value = value.Replace(";", ",");

        return value;
    }
}