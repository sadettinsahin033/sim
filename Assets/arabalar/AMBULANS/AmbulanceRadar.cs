using UnityEngine;

public class AmbulanceRadar : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 1. Sadece "RTC_TrafficVehicle" layer'ına sahip olan nesneleri filtrele
        if (other.gameObject.layer == LayerMask.NameToLayer("RTC_TrafficVehicle"))
        {
            // 2. İstenen Log mesajını konsola yazdır (Kolaylık olsun diye çarpan arabanın adını da ekledim)
            Debug.Log("Araba tespit edildi: " + other.gameObject.name);

            // Çarptığımız objede veya üst objelerinde RTC_CarController scriptini arıyoruz
            RTC_CarController trafficCar = other.GetComponentInParent<RTC_CarController>();

            // Güvenlik kontrolü: Eğer script varsa ve bir yol noktası takip ediyorsa
            if (trafficCar != null && trafficCar.currentWaypoint != null)
            {
                // Eğer aracın şu anki yol noktasının yan şerit bağlantısı (interconnection) atanmışsa
                if (trafficCar.currentWaypoint.interConnectionWaypoint != null)
                {
                    // Arabayı yan şeritteki yeni yol noktasına geçir
                    trafficCar.currentWaypoint = trafficCar.currentWaypoint.interConnectionWaypoint   ;
                }
            }
        }
    }
}