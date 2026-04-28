using UnityEngine;

public class TrafikTetikleyici : MonoBehaviour
{
    [Header("Hedef ve Rota")]
    public Transform spawnNoktasi;
    public RCCP_AIWaypointsContainer gidilecekRota;

    [Header("Bu Tetikleyiciye Özel Araç Ayarlarý")]
    public float hizlanmaMesafesi = 150f;
    public float kaybolmaMesafesi = 500f;

    [Range(0f, 1f)]
    public float uzakMesafeGazi = 0.3f;

    [Range(0f, 1f)]
    public float yakinMesafeGazi = 0.5f;

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
        GameObject yeniArac = TrafikHavuzu.Instance.MüsaitAracVer();

        if (yeniArac != null)
        {
            yeniArac.transform.position = spawnNoktasi.position;
            yeniArac.transform.rotation = spawnNoktasi.rotation;

            RCCP_AI aracAI = TrafikHavuzu.Instance.AIBul(yeniArac);
            if (aracAI != null)
            {
                aracAI.waypointsContainer = gidilecekRota;
                aracAI.currentWaypointIndex = 0;
            }

            // YENÝ: Aracýn TrafikArabasi scriptini bul ve tetikleyicideki ayarlarý ona gönder
            TrafikArabasi trafikArabasi = yeniArac.GetComponent<TrafikArabasi>();
            if (trafikArabasi != null)
            {
                trafikArabasi.AyarlariKur(hizlanmaMesafesi, kaybolmaMesafesi, uzakMesafeGazi, yakinMesafeGazi);
            }

            yeniArac.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Havuzda boþ araç kalmadý!");
        }
    }
}