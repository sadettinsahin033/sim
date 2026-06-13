using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class WaypointPatrol : MonoBehaviour
{
    [Header("Yol Ayarları")]
    public Transform[] yolNoktalari;

    private NavMeshAgent agent;
    private Animator animator;
    private int hedefNoktaIndeksi = 0;
    private bool hareketBasladi = false;

    // KİLİT NOKTA: Üst üste tetiklenmeyi engelleyecek zamanlayıcı
    private float hedefDegistirmeZamani = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.isStopped = true;
        agent.autoBraking = false;
    }

    void Update()
    {
        if (hareketBasladi && yolNoktalari.Length > 0)
        {
            // EĞER son hedef değiştirmenin üzerinden en az 1 saniye geçmediyse mesafe ölçme!
            // Bu satır karakterin kendi etrafında saçmalamasını engeller.
            if (Time.time < hedefDegistirmeZamani) return;

            Vector3 karakterPozisyonu = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 hedefPozisyonu = new Vector3(yolNoktalari[hedefNoktaIndeksi].position.x, 0, yolNoktalari[hedefNoktaIndeksi].position.z);

            float hedefeOlanMesafe = Vector3.Distance(karakterPozisyonu, hedefPozisyonu);

            // Hedefe 1 metre kala sıradaki noktaya geç
            if (hedefeOlanMesafe < 1.0f)
            {
                SonrakiNoktayaGit();
            }
        }
    }

    public void HareketiBaslat()
    {
        if (hareketBasladi) return;

        hareketBasladi = true;
        agent.isStopped = false;

        animator.SetTrigger("StartMoving");

        agent.SetDestination(yolNoktalari[hedefNoktaIndeksi].position);

        // İlk hedef atandığı an kodu 1 saniyeliğine kilitler
        hedefDegistirmeZamani = Time.time + 1.0f;
    }

    void SonrakiNoktayaGit()
    {
        if (yolNoktalari.Length == 0) return;

        hedefNoktaIndeksi = (hedefNoktaIndeksi + 1) % yolNoktalari.Length;
        agent.SetDestination(yolNoktalari[hedefNoktaIndeksi].position);

        // YENİ HEDEF ATANDI: Karakterin saçmalamaması için hedef kontrolünü 1 saniye dondur!
        hedefDegistirmeZamani = Time.time + 1.0f;
    }
}