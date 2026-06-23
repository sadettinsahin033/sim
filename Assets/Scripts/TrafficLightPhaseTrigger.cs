using UnityEngine;
using FCG;

public class TrafficLightPhaseTrigger : MonoBehaviour
{
    [Header("Debug")]
    public bool debugLogs = true;

    [Header("Algýlanacak Oyuncu Aracý")]
    public GameObject playerCar;

    [Header("Kontrol Edilecek FCG Trafik Iþýðý Sistemi")]
    public TrafficLights2 trafficLightsController;

    [Header("Oyuncu Hangi Yönden Geliyor?")]
    public TrafficLights2.TrafficDirection playerDirection = TrafficLights2.TrafficDirection.North;

    [Header("Oyuncunun Iþýðý Hangi Renge Resetlensin?")]
    public TrafficLights2.LightColor targetColor = TrafficLights2.LightColor.Green;

    [Header("Trigger'dan Kaç Saniye Sonra Resetlensin?")]
    public float delayBeforeReset = 0f;

    [Header("Sadece Bir Kez Çalýþsýn")]
    public bool triggerOnce = true;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && triggered)
        {
            if (debugLogs)
            {
                Debug.Log("[TrafficLightPhaseTrigger] Trigger daha önce çalýþtýðý için tekrar çalýþmadý.");
            }

            return;
        }

        if (playerCar == null)
        {
            Debug.LogWarning("[TrafficLightPhaseTrigger] Player Car atanmadý.");
            return;
        }

        bool isPlayerCar =
            other.gameObject == playerCar ||
            other.transform.root.gameObject == playerCar ||
            other.transform.IsChildOf(playerCar.transform);

        if (!isPlayerCar)
        {
            if (debugLogs)
            {
                Debug.Log("[TrafficLightPhaseTrigger] Trigger'a giren obje seçilen araç deðil: " + other.name);
            }

            return;
        }

        triggered = true;

        if (debugLogs)
        {
            Debug.Log(
                "[TrafficLightPhaseTrigger] Seçilen araç algýlandý: " + playerCar.name +
                " | Yön: " + playerDirection +
                " | Hedef renk: " + targetColor +
                " | Delay: " + delayBeforeReset + " sn"
            );
        }

        if (trafficLightsController != null)
        {
            trafficLightsController.ResetPhaseAfterDelay(playerDirection, targetColor, delayBeforeReset);
        }
        else
        {
            Debug.LogWarning("[TrafficLightPhaseTrigger] Traffic Lights Controller atanmadý.");
        }
    }
}