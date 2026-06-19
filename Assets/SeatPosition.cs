using UnityEngine;

public class VRMotorCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform seatPoint;

    [Header("Follow Settings")]
    [SerializeField] private bool followRotation = true;
    [SerializeField] private bool followPosition = true;

    private void LateUpdate()
    {
        if (seatPoint == null)
            return;

        if (followPosition)
        {
            transform.position = seatPoint.position;
        }

        if (followRotation)
        {
            transform.rotation = seatPoint.rotation;
        }
    }
}