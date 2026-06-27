using UnityEngine;

public class CombinedEyeGazeProvider : MonoBehaviour
{
    [Header("Left Eye")]
    [SerializeField] private Transform leftEyeTransform;
    [SerializeField] private OVREyeGaze leftEyeGaze;

    [Header("Right Eye")]
    [SerializeField] private Transform rightEyeTransform;
    [SerializeField] private OVREyeGaze rightEyeGaze;

    [Header("Settings")]
    [SerializeField] private float minimumConfidence = 0.3f;
    [SerializeField] private bool useSingleEyeIfOtherInvalid = true;

    public bool HasValidGaze { get; private set; }
    public float Confidence { get; private set; }
    public Vector3 Origin { get; private set; }
    public Vector3 Direction { get; private set; }

    private void Update()
    {
        UpdateCombinedGaze();
    }

    private void UpdateCombinedGaze()
    {
        HasValidGaze = false;
        Confidence = 0f;
        Origin = Vector3.zero;
        Direction = Vector3.forward;

        bool leftAvailable = leftEyeTransform != null && leftEyeGaze != null;
        bool rightAvailable = rightEyeTransform != null && rightEyeGaze != null;

        bool leftValid = leftAvailable && leftEyeGaze.Confidence >= minimumConfidence;
        bool rightValid = rightAvailable && rightEyeGaze.Confidence >= minimumConfidence;

        if (leftValid && rightValid)
        {
            Vector3 leftDirection = leftEyeTransform.forward.normalized;
            Vector3 rightDirection = rightEyeTransform.forward.normalized;
            Vector3 combinedDirection = leftDirection + rightDirection;

            if (combinedDirection.sqrMagnitude < 0.001f)
                return;

            Origin = (leftEyeTransform.position + rightEyeTransform.position) * 0.5f;
            Direction = combinedDirection.normalized;
            Confidence = Mathf.Min(leftEyeGaze.Confidence, rightEyeGaze.Confidence);
            HasValidGaze = true;

            return;
        }

        if (useSingleEyeIfOtherInvalid && leftValid)
        {
            Origin = leftEyeTransform.position;
            Direction = leftEyeTransform.forward.normalized;
            Confidence = leftEyeGaze.Confidence;
            HasValidGaze = true;

            return;
        }

        if (useSingleEyeIfOtherInvalid && rightValid)
        {
            Origin = rightEyeTransform.position;
            Direction = rightEyeTransform.forward.normalized;
            Confidence = rightEyeGaze.Confidence;
            HasValidGaze = true;
        }
    }
}