using System.Collections;
using UnityEngine;

public class CompoundTriggerArea : MonoBehaviour
{
    [Header("Alan adı")]
    public string areaName = "L Trigger Area";

    [Header("Hedef Araç")]
    public Transform targetVehicle;

    [Header("Bağlanacak Scenario Event Trigger")]
    public ScenarioEventTrigger scenarioEventTrigger;

    [Header("Gerçek çıkış gecikmesi")]
    public float exitDelay = 0.15f;

    [Header("Debug")]
    [InspectorName("Sistem Debug Logları")]
    public bool debugLog = true;

    [InspectorName("Area Giriş/Çıkış Loglarını Göster")]
    public bool showAreaLogs = true;

    private bool isInsideArea = false;
    private Coroutine exitCoroutine;

    private BoxCollider[] childBoxes;

    private void Awake()
    {
        if (scenarioEventTrigger == null)
            scenarioEventTrigger = GetComponent<ScenarioEventTrigger>();

        SetupChildColliders();
    }

    [ContextMenu("Setup Child Colliders")]
    public void SetupChildColliders()
    {
        childBoxes = GetComponentsInChildren<BoxCollider>(true);

        foreach (BoxCollider box in childBoxes)
        {
            if (box == null)
                continue;

            box.isTrigger = true;

            CompoundTriggerRelay relay = box.GetComponent<CompoundTriggerRelay>();

            if (relay == null)
                relay = box.gameObject.AddComponent<CompoundTriggerRelay>();

            relay.area = this;
        }

        if (debugLog)
            Debug.Log(areaName + " hazırlandı. Box sayısı: " + childBoxes.Length);
    }

    public void ChildEnter(Collider other)
    {
        if (!IsTargetVehicle(other))
            return;

        if (exitCoroutine != null)
        {
            StopCoroutine(exitCoroutine);
            exitCoroutine = null;
        }

        if (!isInsideArea)
        {
            isInsideArea = true;
            AreaEnter(other);
        }
    }

    public void ChildStay(Collider other)
    {
        if (!IsTargetVehicle(other))
            return;

        if (isInsideArea)
            AreaStay(other);
    }

    public void ChildExit(Collider other)
    {
        if (!IsTargetVehicle(other))
            return;

        if (exitCoroutine != null)
            StopCoroutine(exitCoroutine);

        exitCoroutine = StartCoroutine(DelayedExitCheck(other));
    }

    private IEnumerator DelayedExitCheck(Collider lastCollider)
    {
        yield return new WaitForSeconds(exitDelay);

        bool stillInside = IsTargetStillInsideAnyBox();

        if (!stillInside && isInsideArea)
        {
            isInsideArea = false;
            AreaExit(lastCollider);
        }

        exitCoroutine = null;
    }

    private bool IsTargetStillInsideAnyBox()
    {
        if (childBoxes == null || childBoxes.Length == 0)
            childBoxes = GetComponentsInChildren<BoxCollider>(true);

        foreach (BoxCollider box in childBoxes)
        {
            if (box == null || !box.enabled || !box.gameObject.activeInHierarchy)
                continue;

            Vector3 worldCenter = box.transform.TransformPoint(box.center);

            Vector3 worldHalfExtents = new Vector3(
                box.size.x * Mathf.Abs(box.transform.lossyScale.x) * 0.5f,
                box.size.y * Mathf.Abs(box.transform.lossyScale.y) * 0.5f,
                box.size.z * Mathf.Abs(box.transform.lossyScale.z) * 0.5f
            );

            Quaternion worldRotation = box.transform.rotation;

            Collider[] hits = Physics.OverlapBox(
                worldCenter,
                worldHalfExtents,
                worldRotation,
                ~0,
                QueryTriggerInteraction.Collide
            );

            foreach (Collider hit in hits)
            {
                if (hit == null)
                    continue;

                if (IsTargetVehicle(hit))
                    return true;
            }
        }

        return false;
    }

    private bool IsTargetVehicle(Collider other)
    {
        if (targetVehicle == null)
        {
            if (debugLog)
                Debug.LogWarning(areaName + " için Target Vehicle atanmamış.");

            return false;
        }

        if (other == null)
            return false;

        if (other.transform == targetVehicle)
            return true;

        if (other.transform.IsChildOf(targetVehicle))
            return true;

        Rigidbody rb = other.attachedRigidbody;

        if (rb != null)
        {
            if (rb.transform == targetVehicle)
                return true;

            if (rb.transform.IsChildOf(targetVehicle))
                return true;

            if (targetVehicle.IsChildOf(rb.transform))
                return true;
        }

        return false;
    }

    private void AreaEnter(Collider other)
    {
        if (showAreaLogs)
            Debug.Log(areaName + " → TEK ALAN GİRİŞ: " + targetVehicle.name);

        if (scenarioEventTrigger != null)
            scenarioEventTrigger.ManualTriggerEnter(other);
        else if (debugLog)
            Debug.LogWarning(areaName + " için ScenarioEventTrigger atanmamış.");
    }

    private void AreaStay(Collider other)
    {
        if (scenarioEventTrigger != null)
            scenarioEventTrigger.ManualTriggerStay(other);
    }

    private void AreaExit(Collider other)
    {
        if (showAreaLogs)
            Debug.Log(areaName + " → TEK ALAN ÇIKIŞ: " + targetVehicle.name);

        if (scenarioEventTrigger != null)
            scenarioEventTrigger.ManualTriggerExit(other);
        else if (debugLog)
            Debug.LogWarning(areaName + " için ScenarioEventTrigger atanmamış.");
    }
}