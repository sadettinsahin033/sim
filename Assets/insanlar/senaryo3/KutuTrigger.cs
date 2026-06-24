using UnityEngine;

public class KutuTrigger : MonoBehaviour
{
    [Header("Hareket Edecek NPC'yi Buraya Sürükle")]
    public BasitNpcKontrol hedefNpc;

    // Arabanın birden fazla çarpıp sistemi bozmasını engeller
    private bool tetiklendiMi = false;

    void OnTriggerEnter(Collider other)
    {
        // Eğer zaten tetiklendiyse, alt objeler (arka tekerlek vb.) tekrar çalıştıramasın diye kodu burada kes
        if (tetiklendiMi) return;

        // Çarpan objeden başlayarak en üst objeye (root) kadar "Player" etiketini ara
        Transform geciciObje = other.transform;
        bool oyuncuBulundu = false;

        // geciciObje null olana kadar (yani hiyerarşinin en tepesine çıkana kadar) döngüyü çalıştır
        while (geciciObje != null)
        {
            if (geciciObje.CompareTag("Player"))
            {
                oyuncuBulundu = true;
                break; // Etiketi bulduk, daha fazla yukarı çıkıp aramaya gerek yok!
            }

            // Bulamadıysa, bir üst ebeveyne (parent) geç ve aramaya devam et
            geciciObje = geciciObje.parent;
        }

        // Eğer silsile içinde "Player" etiketine sahip bir parça bulunduysa senaryoyu başlat
        if (oyuncuBulundu)
        {
            tetiklendiMi = true; // Kilidi kapat
            hedefNpc.SenaryoyuBaslat();
        }
    }
}