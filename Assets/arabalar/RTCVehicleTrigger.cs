using System.Collections;
using UnityEngine;

public class RTCVehicleTrigger : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Etkilenmesini istediğiniz hedef RTC aracını buraya sürükleyin.")]
    public GameObject targetVehicle;

    [Tooltip("Scriptin kaç saniye kapalı kalacağını belirler.")]
    public float disableDuration = 2f;

    // Temasın sadece bir kez gerçekleşmesini sağlamak için kontrol değişkeni
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Eğer daha önce tetiklendiyse veya hedef araç belirlenmediyse işlem yapma
        if (hasTriggered || targetVehicle == null)
            return;

        // Temas eden objenin kökünü (root) veya kendisini kontrol et
        // RTC genelde parent (kök) objede yer aldığı için transform.root kontrolü güvenlidir
        if (other.gameObject == targetVehicle || other.transform.root.gameObject == targetVehicle)
        {
            // Hedef araç üzerindeki RTC_CarController bileşenini bul
            // (Dokümandaki script ismine göre RTC_CarController olarak varsayılmıştır)
            RTC_CarController carController = targetVehicle.GetComponentInChildren<RTC_CarController>();

            if (carController != null)
            {
                // İlk teması kaydet ve bir daha çalışmamasını sağla
                hasTriggered = true;

                // Zamanlayıcıyı (Coroutine) başlat
                StartCoroutine(ToggleTrafficController(carController));
            }
            else
            {
                Debug.LogWarning("Hedef araç üzerinde RTC_CarController bileşeni bulunamadı!");
            }
        }
    }

    private IEnumerator ToggleTrafficController(RTC_CarController controller)
    {
        // RTC Controller scriptini kapat
        controller.enabled = false;
        Debug.Log(targetVehicle.name + " üzerindeki RTC Controller 2 saniyeliğine kapatıldı.");

        // Belirtilen süre kadar (2 saniye) bekle
        yield return new WaitForSeconds(disableDuration);

        // RTC Controller scriptini tekrar aç
        controller.enabled = true;
        Debug.Log(targetVehicle.name + " üzerindeki RTC Controller tekrar açıldı.");
    }
}