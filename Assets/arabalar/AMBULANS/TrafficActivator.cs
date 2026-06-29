using UnityEngine;

public class TrafficActivator : MonoBehaviour
{
    [Header("Aktif Edilecek RTC Arabası")]
    [Tooltip("Oyun başında kapalı olan ve tetiklendiğinde açılacak olan RTC aracını buraya sürükleyin.")]
    public GameObject rtcVehicle;

    [Header("Tetikleyici Filtresi")]
    [Tooltip("Sadece bu Tag'e sahip nesne kutuya girerse tetiklenir (Örn: Player).")]
    public string targetTag = "Player";

    [Tooltip("True ise yukarıdaki Tag'i tüm hiyerarşide arar. False ise kutuya giren her şey arabayı aktif eder.")]
    public bool useTagFiltering = true;

    private void Start()
    {
        // Oyun başladığında RTC arabasının kapalı olduğundan emin oluyoruz.
        if (rtcVehicle != null)
        {
            rtcVehicle.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (rtcVehicle == null) return;

        // Tag filtresi aktifse hiyerarşi kontrolünü başlatıyoruz
        if (useTagFiltering)
        {
            // Çarpan objeden ana objeye kadar yukarısını kontrol et, tag bulunamazsa kodu durdur
            if (!CheckTagInHierarchy(other.transform, targetTag))
            {
                return;
            }
        }

        // RTC Arabasını Aktif Et
        rtcVehicle.SetActive(true);
        Debug.Log("RTC Arabası başarıyla aktif edildi: " + rtcVehicle.name);

        // Bu tetikleyicinin işi bittiği için kendini kapatır.
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Çarpan objeden başlayıp en üstteki ana objeye (Root) kadar hiyerarşiyi tırmanarak Tag kontrolü yapar.
    /// </summary>
    private bool CheckTagInHierarchy(Transform currentTransform, string tagToFind)
    {
        // currentTransform null olana kadar (yani hiyerarşinin en tepesine ulaşana kadar) dön
        while (currentTransform != null)
        {
            // Şu anki objenin tag'i aradığımız tag mi?
            if (currentTransform.CompareTag(tagToFind))
            {
                // Konsolda hangi alt veya ana nesne sayesinde tetiklendiğini görmek istersen log (İsteğe bağlı):
                // Debug.Log("Tag hiyerarşide şurada bulundu: " + currentTransform.name);
                return true;
            }

            // Eğer bu objede yoksa, bir üstteki parent (anne/baba) objeye geçiş yap
            currentTransform = currentTransform.parent;
        }

        // Eğer en tepeye kadar çıkıp hala bulamadıysa false dön
        return false;
    }
}