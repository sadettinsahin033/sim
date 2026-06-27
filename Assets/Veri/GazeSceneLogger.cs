using System;
using System.IO;
using System.Text;
using UnityEngine;

public class GazeSceneLogger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CombinedEyeGazeProvider gazeProvider;
    [SerializeField] private GazeSceneContext sceneContext;

    [Header("Driving Direction")]
    [Tooltip("Motorun/yolun ileri yönünü temsil eden obje. XR Camera verme.")]
    [SerializeField] private Transform drivingForwardReference;

    [Tooltip("Ray hiçbir şeye çarpmadığında bu açı içindeyse NoHitForward sayılır.")]
    [SerializeField] private float noHitForwardHorizontalAngle = 35f;

    [Header("Raycast")]
    [SerializeField] private float maxDistance = 25f;
    [SerializeField] private LayerMask hitLayers = ~0;
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;
    [SerializeField] private bool preferSpecificAOIOverForwardView = true;
    [SerializeField] private bool drawDebugRay = true;

    [Header("Filter")]
    [SerializeField] private float confidenceThreshold = 0.3f;

    [Header("Smoothing")]
    [SerializeField] private float originSmoothSpeed = 12f;
    [SerializeField] private float directionSmoothSpeed = 12f;

    [Header("Fixation")]
    [SerializeField] private float fixationThreshold = 0.2f;

    [Header("Warning")]
    [SerializeField] private float irrelevantWarningThreshold = 2.0f;

    [Header("File Logging")]
    [SerializeField] private bool startLoggingOnPlay = true;
    [SerializeField] private float sampleRateHz = 10f;
    [SerializeField] private bool logFilePath = true;

    private Vector3 smoothOrigin;
    private Vector3 smoothDirection;
    private bool smoothingInitialized;

    private StreamWriter writer;
    private string filePath;
    private bool isLogging;
    private float sampleTimer;
    private float sampleInterval;
    private int sampleIndex;

    private string currentGazeKey;
    private float currentFixationDuration;
    private float lastFixationDuration;
    private int fixationCount;

    private float irrelevantLookTimer;
    private bool gazeWarningShown;

    private GazeSceneFrame latestFrame = new GazeSceneFrame();

    private void Awake()
    {
        sampleInterval = 1f / Mathf.Max(1f, sampleRateHz);
    }

    private void Start()
    {
        if (sceneContext == null)
            sceneContext = FindObjectOfType<GazeSceneContext>();

        if (gazeProvider == null)
            gazeProvider = FindObjectOfType<CombinedEyeGazeProvider>();

        if (startLoggingOnPlay)
            StartLogging();
    }

    private void Update()
    {
        UpdateGazeFrame();

        if (!isLogging)
            return;

        sampleTimer += Time.deltaTime;

        if (sampleTimer >= sampleInterval)
        {
            sampleTimer -= sampleInterval;
            WriteLatestFrame();
        }
    }

    public void StartLogging()
    {
        if (isLogging)
            return;

        if (sceneContext == null)
            sceneContext = FindObjectOfType<GazeSceneContext>();

        string participantId = "P001";
        string sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        if (sceneContext != null)
        {
            participantId = sceneContext.participantId;
            sessionId = sceneContext.sessionId;
        }

        string fileName = "gaze_scene_" + participantId + "_" + sessionId + ".jsonl";
        filePath = Path.Combine(Application.persistentDataPath, fileName);

        writer = new StreamWriter(filePath, false, Encoding.UTF8);

        isLogging = true;
        sampleIndex = 0;
        sampleTimer = 0f;

        if (logFilePath)
            Debug.Log("[GAZE SCENE LOG STARTED] " + filePath);
    }

    public void StopLogging()
    {
        if (!isLogging)
            return;

        EndCurrentFixation();

        isLogging = false;

        if (writer != null)
        {
            writer.Flush();
            writer.Close();
            writer = null;
        }

        Debug.Log("[GAZE SCENE LOG STOPPED] " + filePath);
    }

    private void UpdateGazeFrame()
    {
        if (gazeProvider == null)
        {
            FillInvalidGazeFrame("NoGazeProvider", "No Gaze Provider", 0f);
            return;
        }

        if (!gazeProvider.HasValidGaze || gazeProvider.Confidence < confidenceThreshold)
        {
            EndCurrentFixation();
            smoothingInitialized = false;

            FillInvalidGazeFrame("LowConfidence", "Low Confidence / No Valid Gaze", gazeProvider.Confidence);
            return;
        }

        UpdateSmoothedRay(gazeProvider.Origin, gazeProvider.Direction);

        if (drawDebugRay)
            Debug.DrawRay(smoothOrigin, smoothDirection * maxDistance, Color.green);

        RaycastHit hit;

        if (TryGetBestHit(out hit))
        {
            HandleHit(hit, gazeProvider.Confidence);
        }
        else
        {
            HandleNoHit(gazeProvider.Confidence);
        }
    }

    private void UpdateSmoothedRay(Vector3 rawOrigin, Vector3 rawDirection)
    {
        if (!smoothingInitialized)
        {
            smoothOrigin = rawOrigin;
            smoothDirection = rawDirection.normalized;
            smoothingInitialized = true;
            return;
        }

        float originLerp = 1f - Mathf.Exp(-originSmoothSpeed * Time.deltaTime);
        float directionLerp = 1f - Mathf.Exp(-directionSmoothSpeed * Time.deltaTime);

        smoothOrigin = Vector3.Lerp(smoothOrigin, rawOrigin, originLerp);
        smoothDirection = Vector3.Lerp(smoothDirection, rawDirection.normalized, directionLerp).normalized;
    }

    private bool TryGetBestHit(out RaycastHit bestHit)
    {
        RaycastHit[] hits = Physics.RaycastAll(
            smoothOrigin,
            smoothDirection,
            maxDistance,
            hitLayers,
            triggerInteraction
        );

        if (hits == null || hits.Length == 0)
        {
            bestHit = default(RaycastHit);
            return false;
        }

        Array.Sort(hits, CompareHitsByDistance);

        if (!preferSpecificAOIOverForwardView)
        {
            bestHit = hits[0];
            return true;
        }

        for (int i = 0; i < hits.Length; i++)
        {
            GazeTarget target = hits[i].collider.GetComponentInParent<GazeTarget>();

            if (target != null && target.aoiType != AOIType.ForwardView)
            {
                bestHit = hits[i];
                return true;
            }
        }

        for (int i = 0; i < hits.Length; i++)
        {
            GazeTarget target = hits[i].collider.GetComponentInParent<GazeTarget>();

            if (target != null && target.aoiType == AOIType.ForwardView)
            {
                bestHit = hits[i];
                return true;
            }
        }

        bestHit = hits[0];
        return true;
    }

    private static int CompareHitsByDistance(RaycastHit a, RaycastHit b)
    {
        return a.distance.CompareTo(b.distance);
    }

    private void HandleHit(RaycastHit hit, float confidence)
    {
        GazeTarget target = hit.collider.GetComponentInParent<GazeTarget>();

        if (target == null)
        {
            ProcessGaze(
                true,
                confidence,
                AOIType.Unknown,
                "UnclassifiedHit",
                "Unclassified Hit - " + hit.collider.name,
                GazeRelevance.Neutral,
                hit.distance,
                IsLookingForward()
            );

            return;
        }

        ProcessGaze(
            true,
            confidence,
            target.aoiType,
            target.TargetId,
            target.DisplayName,
            target.GetRelevance(),
            hit.distance,
            IsLookingForward()
        );
    }

    private void HandleNoHit(float confidence)
    {
        bool lookingForward = IsLookingForward();

        if (lookingForward)
        {
            ProcessGaze(
                true,
                confidence,
                AOIType.NoHitForward,
                "NoHitForward",
                "No Hit - Forward View",
                GazeRelevance.Relevant,
                -1f,
                true
            );
        }
        else
        {
            ProcessGaze(
                true,
                confidence,
                AOIType.NoHitOffRoad,
                "NoHitOffRoad",
                "No Hit - Off Road",
                GazeRelevance.Irrelevant,
                -1f,
                false
            );
        }
    }

    private void ProcessGaze(
        bool hasValidGaze,
        float confidence,
        AOIType aoiType,
        string targetId,
        string displayName,
        GazeRelevance relevance,
        float hitDistance,
        bool isLookingForward)
    {
        string newKey = targetId + "_" + aoiType + "_" + relevance;

        if (newKey != currentGazeKey)
        {
            EndCurrentFixation();

            currentGazeKey = newKey;
            currentFixationDuration = 0f;
        }

        currentFixationDuration += Time.deltaTime;

        bool warningCandidate = aoiType == AOIType.TaskIrrelevant || aoiType == AOIType.NoHitOffRoad;

        if (warningCandidate)
        {
            irrelevantLookTimer += Time.deltaTime;

            if (irrelevantLookTimer >= irrelevantWarningThreshold)
                gazeWarningShown = true;
        }
        else
        {
            irrelevantLookTimer = 0f;
            gazeWarningShown = false;
        }

        FillCommonFrame();

        latestFrame.hasValidGaze = hasValidGaze;
        latestFrame.gazeConfidence = confidence;

        latestFrame.gazeOriginX = smoothOrigin.x;
        latestFrame.gazeOriginY = smoothOrigin.y;
        latestFrame.gazeOriginZ = smoothOrigin.z;

        latestFrame.gazeDirectionX = smoothDirection.x;
        latestFrame.gazeDirectionY = smoothDirection.y;
        latestFrame.gazeDirectionZ = smoothDirection.z;

        latestFrame.gazeAOIType = aoiType.ToString();
        latestFrame.gazeTargetId = targetId;
        latestFrame.gazeDisplayName = displayName;
        latestFrame.gazeRelevance = relevance.ToString();

        latestFrame.gazeHitDistance = hitDistance;
        latestFrame.isLookingForward = isLookingForward;

        latestFrame.currentFixationDuration = currentFixationDuration;
        latestFrame.lastFixationDuration = lastFixationDuration;
        latestFrame.fixationCount = fixationCount;

        latestFrame.gazeWarningCandidate = warningCandidate;
        latestFrame.gazeWarningShown = gazeWarningShown;
    }

    private void FillInvalidGazeFrame(string targetId, string displayName, float confidence)
    {
        FillCommonFrame();

        latestFrame.hasValidGaze = false;
        latestFrame.gazeConfidence = confidence;

        latestFrame.gazeOriginX = 0f;
        latestFrame.gazeOriginY = 0f;
        latestFrame.gazeOriginZ = 0f;

        latestFrame.gazeDirectionX = 0f;
        latestFrame.gazeDirectionY = 0f;
        latestFrame.gazeDirectionZ = 0f;

        latestFrame.gazeAOIType = AOIType.Unknown.ToString();
        latestFrame.gazeTargetId = targetId;
        latestFrame.gazeDisplayName = displayName;
        latestFrame.gazeRelevance = GazeRelevance.Neutral.ToString();

        latestFrame.gazeHitDistance = -1f;
        latestFrame.isLookingForward = false;

        latestFrame.currentFixationDuration = 0f;
        latestFrame.lastFixationDuration = lastFixationDuration;
        latestFrame.fixationCount = fixationCount;

        latestFrame.gazeWarningCandidate = false;
        latestFrame.gazeWarningShown = false;
    }

    private void FillCommonFrame()
    {
        latestFrame.recordType = "gaze_scene_sample";
        latestFrame.sampleIndex = sampleIndex;
        latestFrame.unityTime = Time.time;
        latestFrame.unixTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if (sceneContext == null)
            return;

        latestFrame.participantId = sceneContext.participantId;
        latestFrame.sessionId = sceneContext.sessionId;

        latestFrame.sceneName = sceneContext.SceneName;
        latestFrame.scenarioId = sceneContext.scenarioId;
        latestFrame.scenarioName = sceneContext.scenarioName;
        latestFrame.scenarioDifficulty = sceneContext.scenarioDifficulty;
        latestFrame.scenarioDifficultyLevel = sceneContext.scenarioDifficultyLevel;
        latestFrame.scenarioElapsedTime = sceneContext.GetScenarioElapsedTime();

        latestFrame.currentEventTag = sceneContext.currentEventTag;
        latestFrame.currentEventDescription = sceneContext.currentEventDescription;
        latestFrame.currentEventSeverity = sceneContext.currentEventSeverity;
        latestFrame.expectedReaction = sceneContext.expectedReaction;
    }

    private bool IsLookingForward()
    {
        Transform reference = drivingForwardReference != null
            ? drivingForwardReference
            : transform;

        Vector3 referenceForward = Vector3.ProjectOnPlane(reference.forward, Vector3.up).normalized;
        Vector3 gazeForward = Vector3.ProjectOnPlane(smoothDirection, Vector3.up).normalized;

        if (referenceForward.sqrMagnitude < 0.001f || gazeForward.sqrMagnitude < 0.001f)
            return false;

        float angle = Vector3.Angle(referenceForward, gazeForward);

        return angle <= noHitForwardHorizontalAngle;
    }

    private void EndCurrentFixation()
    {
        if (string.IsNullOrEmpty(currentGazeKey))
        {
            currentFixationDuration = 0f;
            return;
        }

        if (currentFixationDuration >= fixationThreshold)
        {
            fixationCount++;
            lastFixationDuration = currentFixationDuration;
        }

        currentGazeKey = null;
        currentFixationDuration = 0f;
    }

    private void WriteLatestFrame()
    {
        if (writer == null)
            return;

        latestFrame.sampleIndex = sampleIndex;
        latestFrame.unityTime = Time.time;
        latestFrame.unixTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        string json = JsonUtility.ToJson(latestFrame);
        writer.WriteLine(json);

        sampleIndex++;
    }

    private void OnDisable()
    {
        StopLogging();
    }

    private void OnApplicationQuit()
    {
        StopLogging();
    }

    [Serializable]
    private class GazeSceneFrame
    {
        public string recordType;
        public int sampleIndex;
        public float unityTime;
        public long unixTimeMs;

        public string participantId;
        public string sessionId;

        public string sceneName;
        public string scenarioId;
        public string scenarioName;
        public string scenarioDifficulty;
        public int scenarioDifficultyLevel;
        public float scenarioElapsedTime;

        public string currentEventTag;
        public string currentEventDescription;
        public string currentEventSeverity;
        public string expectedReaction;

        public bool hasValidGaze;
        public float gazeConfidence;

        public float gazeOriginX;
        public float gazeOriginY;
        public float gazeOriginZ;

        public float gazeDirectionX;
        public float gazeDirectionY;
        public float gazeDirectionZ;

        public string gazeAOIType;
        public string gazeTargetId;
        public string gazeDisplayName;
        public string gazeRelevance;

        public float gazeHitDistance;
        public bool isLookingForward;

        public float currentFixationDuration;
        public float lastFixationDuration;
        public int fixationCount;

        public bool gazeWarningCandidate;
        public bool gazeWarningShown;
    }
}