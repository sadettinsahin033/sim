using UnityEngine;
using UnityEngine.SceneManagement;

public class GazeSceneContext : MonoBehaviour
{
    [Header("Session")]
    public string participantId = "P001";
    public string sessionId = "";

    [Header("Scenario")]
    public string scenarioId = "Scenario_01";
    public string scenarioName = "Basic Ride";
    public string scenarioDifficulty = "Easy";

    [Tooltip("Senaryo 1 kolay, senaryo 3 zor gibi düşünebilirsin.")]
    public int scenarioDifficultyLevel = 1;

    [Header("Runtime Event")]
    public string currentEventTag = "None";
    public string currentEventDescription = "Normal driving";
    public string currentEventSeverity = "Low";
    public string expectedReaction = "KeepForwardAttention";

    public string SceneName { get; private set; }
    public float ScenarioStartTime { get; private set; }

    private void Awake()
    {
        SceneName = SceneManager.GetActiveScene().name;
        ScenarioStartTime = Time.time;

        if (string.IsNullOrEmpty(sessionId))
        {
            sessionId = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }
    }

    public float GetScenarioElapsedTime()
    {
        return Time.time - ScenarioStartTime;
    }

    public void SetScenario(
        string newScenarioId,
        string newScenarioName,
        string newDifficulty,
        int newDifficultyLevel)
    {
        scenarioId = newScenarioId;
        scenarioName = newScenarioName;
        scenarioDifficulty = newDifficulty;
        scenarioDifficultyLevel = newDifficultyLevel;
        ScenarioStartTime = Time.time;
    }

    public void SetEvent(
        string eventTag,
        string eventDescription,
        string eventSeverity,
        string reaction)
    {
        currentEventTag = eventTag;
        currentEventDescription = eventDescription;
        currentEventSeverity = eventSeverity;
        expectedReaction = reaction;
    }

    public void ClearEvent()
    {
        currentEventTag = "None";
        currentEventDescription = "Normal driving";
        currentEventSeverity = "Low";
        expectedReaction = "KeepForwardAttention";
    }
}