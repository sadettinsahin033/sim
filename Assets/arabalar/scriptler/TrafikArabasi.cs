using UnityEngine;



public class TrafikArabasi : MonoBehaviour

{

    public float hizlanmaMesafesi;

    public float kaybolmaMesafesi;

    public float uzakMesafeGazi;

    public float yakinMesafeGazi;



    [Header("Göz (Sensör) Ayarları")]

    public float engelAlgilamaMesafesi = 15f; // Lazerin boyu uzun kalsın ki araba kör olmasın

    public AudioSource kornaSesi;

    public string trafikAraciTagi = "RCCP_Vehicle";



    [Header("Akıllı Fren Ayarları")]

    public float frenBaslamaMesafesi = 5f; // Kaç metre kala yavaşlamaya başlasın

    public float tamDurmaMesafesi = 1.5f;  // Çarpmaya kaç metre kala ZIMBA gibi dursun (Arabanın burnunu kurtarmak için 1.5 idealdir)



    [Header("Trafik Işığı Ayarları")]

    public string kirmiziIsikTagi = "Stop";



    private Transform oyuncuAraci;

    private RCCP_AI yapayZeka;

    private bool sistemHazirMi = false;

    private Rigidbody benimRb;



    public void AyarlariKur(float hizlanma, float kaybolma, float uzakGaz, float yakinGaz)

    {

        hizlanmaMesafesi = hizlanma;

        kaybolmaMesafesi = kaybolma;

        uzakMesafeGazi = uzakGaz;

        yakinMesafeGazi = yakinGaz;

    }



    void Awake()

    {

        yapayZeka = GetComponent<RCCP_AI>();

        benimRb = GetComponent<Rigidbody>();

    }



    void OnEnable()

    {

        sistemHazirMi = false;

        if (oyuncuAraci == null)

        {

            GameObject oyuncuObjesi = GameObject.FindGameObjectWithTag("Player");

            if (oyuncuObjesi != null) oyuncuAraci = oyuncuObjesi.transform;

        }

        if (yapayZeka != null) yapayZeka.maxThrottle = uzakMesafeGazi;

        Invoke("SistemiHazirla", 2f);

    }



    void SistemiHazirla() => sistemHazirMi = true;



    void Update()

    {

        if (!sistemHazirMi || oyuncuAraci == null || yapayZeka == null) return;



        Vector3 baslangicNoktasi = transform.position + (Vector3.up * 0.7f) + (transform.forward * 2.5f);

        Vector3 kutuYarimBoyutu = new Vector3(1.2f, 0.5f, 0.5f);

        bool engelVarMi = false;



        RaycastHit[] hits = Physics.BoxCastAll(baslangicNoktasi, kutuYarimBoyutu, transform.forward, transform.rotation, engelAlgilamaMesafesi, ~0, QueryTriggerInteraction.Collide);



        foreach (RaycastHit hit in hits)

        {

            Rigidbody hedefRb = hit.collider.attachedRigidbody;

            if (hedefRb != null && hedefRb == benimRb) continue;



            bool oyuncuMu = hit.collider.CompareTag("Player") || (hedefRb != null && hedefRb.CompareTag("Player"));

            bool digerArabaMi = hit.collider.CompareTag(trafikAraciTagi) || (hedefRb != null && hedefRb.CompareTag(trafikAraciTagi));

            bool kirmiziIsikMi = hit.collider.CompareTag(kirmiziIsikTagi);



            if (oyuncuMu || digerArabaMi || kirmiziIsikMi)

            {

                // Engeli gördük! Peki ne kadar uzağımızda?

                float mesafe = hit.distance;



                // Eğer engel 5 metrenin (frenBaslamaMesafesi) içindeyse tepki ver!

                if (mesafe <= frenBaslamaMesafesi)

                {

                    engelVarMi = true;

                    yapayZeka.maxThrottle = 0f; // Kesinlikle gaz verme



                    // DURUM 1: Engele 1.5 metre (tamDurmaMesafesi) veya daha az kaldıysa (Çarpışma anı)

                    if (mesafe <= tamDurmaMesafesi)

                    {

                        if (benimRb != null)

                        {

                            benimRb.linearDamping = 100f;  // Duvar gibi dur

                            benimRb.angularDamping = 100f;

                        }

                    }

                    // DURUM 2: Engele 5 metre ile 1.5 metre arasındaysak (Güvenli Yavaşlama Bölgesi)

                    else

                    {

                        if (benimRb != null)

                        {

                            // Arabanın hedefe yaklaştıkça artan bir fren gücü uygulamasını sağlıyoruz (0.05'ten 10'a kadar)

                            float frenOrani = 1f - ((mesafe - tamDurmaMesafesi) / (frenBaslamaMesafesi - tamDurmaMesafesi));

                            benimRb.linearDamping = Mathf.Lerp(0.05f, 15f, frenOrani);



                            benimRb.angularDamping = 0.05f; // Direksiyonu kilitleme ki AI yoldan çıkmasın

                        }

                    }



                    // Korna çal

                    if (!kirmiziIsikMi && (oyuncuMu || digerArabaMi))

                    {

                        if (kornaSesi != null && !kornaSesi.isPlaying) kornaSesi.Play();

                    }



                    break; // İlk tehlikeli engelde işlemi tamamla

                }

            }

        }



        // --- GÖRÜŞ ALANINDA TEHLİKELİ MESAFEDE (5 Metre) ENGEL YOKSA ---

        if (!engelVarMi)

        {

            if (benimRb != null)

            {

                benimRb.linearDamping = 0.05f;

                benimRb.angularDamping = 0.05f;

            }



            float mesafe = Vector3.Distance(oyuncuAraci.position, transform.position);

            if (mesafe > kaybolmaMesafesi) gameObject.SetActive(false);

            else if (mesafe <= hizlanmaMesafesi) yapayZeka.maxThrottle = yakinMesafeGazi;

            else yapayZeka.maxThrottle = uzakMesafeGazi;

        }

    }



    void OnDrawGizmos()

    {

        // Gelişmiş Gizmo Çizimi: 15 metrelik tarama alanı KIRMIZI, 5 metrelik tehlike alanı SARI çizilecek

        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);

        Vector3 baslangicNoktasi = transform.position + (Vector3.up * 0.7f) + (transform.forward * 2.5f);

        Vector3 kutuYarimBoyutu = new Vector3(1.2f, 0.5f, 0.5f);



        Gizmos.matrix = Matrix4x4.TRS(baslangicNoktasi, transform.rotation, Vector3.one);

        Gizmos.DrawCube(Vector3.forward * (engelAlgilamaMesafesi / 2), new Vector3(kutuYarimBoyutu.x * 2, kutuYarimBoyutu.y * 2, engelAlgilamaMesafesi));



        // Sarı Tehlike/Fren Alanı Çizgisi

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(Vector3.forward * (frenBaslamaMesafesi / 2), new Vector3(kutuYarimBoyutu.x * 2, kutuYarimBoyutu.y * 2, frenBaslamaMesafesi));

    }

}