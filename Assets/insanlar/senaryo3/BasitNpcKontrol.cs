using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BasitNpcKontrol : MonoBehaviour
{
    [Header("Bileşenler")]
    public NavMeshAgent agent;
    public Animator anim;

    [Header("Rotalar")]
    public Transform ilkHedef;
    public Transform sonHedef;

    void Start()
    {
        // Başlangıçta agent ve animatör yoksa hata vermemesi için koruma (isteğe bağlı)
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (anim == null) anim = GetComponent<Animator>();

        // BAŞLANGIÇ: NPC HEYKEL GİBİ BEKLER
        agent.isStopped = true;
        anim.speed = 0f;
    }

    public void SenaryoyuBaslat()
    {
        // Eğer script zaten çalışıyorsa üst üste binmesin diye önlem
        StopAllCoroutines();
        StartCoroutine(NpcSenaryosu());
    }

    IEnumerator NpcSenaryosu()
    {
        // ==========================================
        // 1. AŞAMA: TETİKLENDİ! İLK HEDEFE GİT
        // ==========================================
        anim.speed = 1f;
        anim.Play("yurume");

        agent.isStopped = false;
        agent.updateRotation = true;
        agent.SetDestination(ilkHedef.position);

        // 🌟 KRİTİK: Rota hesaplanana kadar 1 kare bekle
        yield return null;
        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= 0.5f);

        // ==========================================
        // 2. AŞAMA: HEDEFTE DUR, ARA ANİMASYON
        // ==========================================
        agent.isStopped = true;
        agent.velocity = Vector3.zero; // O efsanevi kayma engelleyici satırın :)
        agent.updateRotation = false;

        anim.SetTrigger("New Trigger");

        yield return new WaitForSeconds(2f);

        // ==========================================
        // 3. AŞAMA: SON HEDEFE DOĞRU DEVAM ET
        // ==========================================
        agent.updateRotation = true;
        agent.isStopped = false;
        agent.SetDestination(sonHedef.position);

        // 🌟 KRİTİK: Rota hesaplanana kadar yine 1 kare bekle
        yield return null;
        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= 0.5f);

        // ==========================================
        // 4. AŞAMA: SON HEDEFTE TAMAMEN DURMA
        // ==========================================
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        anim.Play("idle"); // Varsa NPC'nin normal duruş animasyonu, yoksa anim.speed = 0f yapabilirsin.
    }
}