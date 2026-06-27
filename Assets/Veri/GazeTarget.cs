using UnityEngine;

[DisallowMultipleComponent]
public class GazeTarget : MonoBehaviour
{
    [Header("AOI")]
    public AOIType aoiType = AOIType.Unknown;

    [Header("Names")]
    [SerializeField] private string targetId;
    [SerializeField] private string displayName;

    [Header("Relevance Override")]
    [SerializeField] private bool overrideRelevance = false;
    [SerializeField] private GazeRelevance relevanceOverride = GazeRelevance.Relevant;

    public string TargetId
    {
        get
        {
            if (string.IsNullOrEmpty(targetId))
                return gameObject.name;

            return targetId;
        }
    }

    public string DisplayName
    {
        get
        {
            if (string.IsNullOrEmpty(displayName))
                return gameObject.name;

            return displayName;
        }
    }

    public GazeRelevance GetRelevance()
    {
        if (overrideRelevance)
            return relevanceOverride;

        if (aoiType == AOIType.TaskIrrelevant)
            return GazeRelevance.Irrelevant;

        if (aoiType == AOIType.NoHitOffRoad)
            return GazeRelevance.Irrelevant;

        if (aoiType == AOIType.Unknown)
            return GazeRelevance.Neutral;

        return GazeRelevance.Relevant;
    }

    private void Reset()
    {
        targetId = gameObject.name;
        displayName = gameObject.name;
    }
}