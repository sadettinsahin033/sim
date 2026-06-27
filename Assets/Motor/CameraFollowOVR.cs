using UnityEngine;

[DefaultExecutionOrder(10000)]
public class OVRMotorSeatFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform seatPoint;
    [SerializeField] private Transform centerEyeAnchor;

    [Header("Follow")]
    [SerializeField] private bool followPosition = true;
    [SerializeField] private bool followYawOnly = true;
    [SerializeField] private bool followFullRotation = false;

    [Header("Height Fix")]
    [SerializeField] private bool forceEyeToSeatHeight = true;

    private void Awake()
    {
        FindCenterEyeIfNeeded();
    }

    private void LateUpdate()
    {
        if (seatPoint == null)
            return;

        FindCenterEyeIfNeeded();

        if (followYawOnly)
        {
            Vector3 seatEuler = seatPoint.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, seatEuler.y, 0f);
        }
        else if (followFullRotation)
        {
            transform.rotation = seatPoint.rotation;
        }

        if (!followPosition)
            return;

        if (centerEyeAnchor != null)
        {
            Vector3 delta = seatPoint.position - centerEyeAnchor.position;

            if (!forceEyeToSeatHeight)
                delta.y = seatPoint.position.y - transform.position.y;

            transform.position += delta;
        }
        else
        {
            transform.position = seatPoint.position;
        }
    }

    private void FindCenterEyeIfNeeded()
    {
        if (centerEyeAnchor != null)
            return;

        Transform found = transform.Find("TrackingSpace/CenterEyeAnchor");

        if (found != null)
            centerEyeAnchor = found;
    }
}