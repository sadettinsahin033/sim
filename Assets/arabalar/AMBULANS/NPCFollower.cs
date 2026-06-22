using UnityEngine;

public class NPCFollower : MonoBehaviour
{
    private RTC_CarController rtcController;
    public Transform playerVehicle; // Ana arabanızı buraya sürükleyin

    // NPC'nin kovalama moduna geçeceği mesafe
    public float followRange = 40f;

    void Start()
    {
        // RTC Car Controller bileşenini otomatik olarak alıyoruz
        rtcController = GetComponent<RTC_CarController>();
    }

    void Update()
    {
        if (playerVehicle == null || rtcController == null) return;

        float distance = Vector3.Distance(transform.position, playerVehicle.position);

        // NPC'nin oyuncuya olan yönünü hesaplıyoruz
        Vector3 directionToPlayer = (playerVehicle.position - transform.position).normalized;

        // Ana araba NPC'nin önünde mi yoksa arkasında mı?
        float dot = Vector3.Dot(transform.forward, directionToPlayer);

        // EĞER ana araba NPC'nin ÖNÜNDEYSE (dot > 0) ve takip mesafesindeyse:
        if (distance < followRange && dot > 0)
        {
            ApplyChaseMode();
        }
        else
        {
            ResetToNormalMode();
        }
    }

    void ApplyChaseMode()
    {
        // NPC hızı artırıp bize yetişmeye çalışıyor
        rtcController.maximumSpeed = 120f;

        // --- ŞERİT VE HEDEF DEĞİŞTİRME MANTIĞI BURAYA GELECEK ---
    }

    void ResetToNormalMode()
    {
        // Bizi geçtiğinde veya çok uzaklaştığımızda normal trafik hızına dönüyor
        rtcController.maximumSpeed = 50f;
    }
}