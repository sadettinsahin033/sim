using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BasitNpcKontrol : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator anim;

    public Transform ilkHedef;
    public Transform sonHedef;

    void Start()
    {
        // ==========================================
        // BAŞLANGIÇ: NPC HEYKEL GİBİ BEKLER
        // ==========================================
        agent.isStopped = true;
        anim.speed = 0f; // Animasyonu dondur ki yerinde yürümesin
    }

    // Kutu Trigger'ın dışarıdan çağıracağı özel fonksiyon (Public olmak zorunda)
    public void SenaryoyuBaslat()
    {
        StartCoroutine(NpcSenaryosu());
    }

    IEnumerator NpcSenaryosu()
    {
        // ==========================================
        // 1. AŞAMA: TETİKLENDİ! ANİMASYON 1 İLE İLK HEDEFE GİT
        // ==========================================

        anim.speed = 1f; // Animasyon hızını normale döndür
        anim.Play("yurume"); // Yürümeyi zorla başlat

        agent.isStopped = false;
        agent.updateRotation = true;

        agent.SetDestination(ilkHedef.position);

        yield return new WaitUntil(() => agent.remainingDistance <= 0.5f && !agent.pathPending);

        // ==========================================
        // 2. AŞAMA: HEDEFTE DUR, ANİMASYON 2 VE 3 ÇALIŞSIN
        // ==========================================
        agent.isStopped = true;
        agent.velocity = Vector3.zero; // 🌟 YENİ: Kalan tüm fiziksel hızı anında sıfırlar, kaymayı bıçak gibi keser!
        agent.updateRotation = false;

        anim.SetTrigger("New Trigger");

        // Senin bulduğun o mükemmel geçiş süresi:
        yield return new WaitForSeconds(2.9f);

        // ==========================================
        // 3. AŞAMA: SON HEDEFE DOĞRU ANİMASYON 1 İLE DEVAM ET
        // ==========================================
        agent.updateRotation = true;
        agent.isStopped = false;

        agent.SetDestination(sonHedef.position);
    }
}