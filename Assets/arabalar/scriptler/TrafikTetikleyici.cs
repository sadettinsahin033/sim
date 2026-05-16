using UnityEngine;

public class TrafikTetikleyici : MonoBehaviour
{
    [Header("Özel Araç Atamasý")]
    [Tooltip("Eðer buraya bir araç sürüklerseniz, sistem önce bu aracý çýkarmaya çalýþýr. Boþ býrakýrsanýz havuzdan rastgele çeker.")]
    public GameObject ozelArac;

    [Header("Hedef ve Rota")]
    public Transform spawnNoktasi;
    public RCCP_AIWaypointsContainer gidilecekRota;

    [Header("Ayarlar")]
    public float hizlanmaMesafesi = 150f;
    public float kaybolmaMesafesi = 500f;
    [Range(0f, 1f)] public float uzakMesafeGazi = 0.3f;
    [Range(0f, 1f)] public float yakinMesafeGazi = 0.5f;

    private bool tetiklendiMi = false;

    private void OnTriggerEnter(Collider other)
    {
        if (tetiklendiMi) return;

        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            tetiklendiMi = true;
            AraciYolaCikar();
        }
    }

    void AraciYolaCikar()
    {
        GameObject secilenArac = null;

        // MANTIK: Önce özel araç var mý ve müsait mi kontrol et
        if (ozelArac != null && !ozelArac.activeInHierarchy)
        {
            secilenArac = ozelArac;
        }
        else
        {
            // Özel araç yoksa veya o araç zaten þu an yoldaysa (aktifse), havuzdan rastgele al
            secilenArac = TrafikHavuzu.Instance.RastgeleMusaitAracVer();
        }

        if (secilenArac != null)
        {
            // Pozisyon ve Rotasyon ayarla
            secilenArac.transform.position = spawnNoktasi.position;
            secilenArac.transform.rotation = spawnNoktasi.rotation;

            // AI Ayarlarýný yap
            RCCP_AI aracAI = TrafikHavuzu.Instance.AIBul(secilenArac);
            if (aracAI != null)
            {
                aracAI.waypointsContainer = gidilecekRota;
                aracAI.currentWaypointIndex = 0;
            }

            // TrafikArabasi script ayarlarýný aktar
            TrafikArabasi trafikArabasi = secilenArac.GetComponent<TrafikArabasi>();
            if (trafikArabasi != null)
            {
                trafikArabasi.AyarlariKur(hizlanmaMesafesi, kaybolmaMesafesi, uzakMesafeGazi, yakinMesafeGazi);
            }

            secilenArac.SetActive(true);
            gameObject.SetActive(false); // Tetikleyiciyi kapat
        }
        else
        {
            Debug.LogWarning(gameObject.name + " için uygun araç bulunamadý!");
        }
    }
}