using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class OzelNpcKontrol : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator anim;

    public Transform ilkHedef;
    public Transform sonHedef;

    void Start()
    {
        HareketeGec();
    }

    public void HareketeGec()
    {
        StartCoroutine(NpcSenaryosu());
    }

    IEnumerator NpcSenaryosu()
    {
        // YENİ EKLENTİ: NavMesh'in sahnede kendini bulması için 0.1 saniye bekle
        yield return new WaitForSeconds(0.1f);

        // ==========================================
        // 1. ADIM: BAŞTA 7 SANİYE YÜRÜME
        // ==========================================
        agent.isStopped = false;
        agent.updateRotation = true;

        // İlk hedefe git emrini şimdi veriyoruz
        agent.SetDestination(ilkHedef.position);

        anim.ResetTrigger("TurnRightTrigger");
        anim.SetTrigger("WalkTrigger");

        yield return new WaitForSeconds(7f);

        // ==========================================
        // 2. ADIM: DUR VE SOLA DÖN
        // ==========================================
        agent.isStopped = true;
        agent.updateRotation = false;

        anim.ResetTrigger("WalkTrigger");
        anim.SetTrigger("TurnLeftTrigger");

        yield return new WaitForSeconds(1.5f);

        // ==========================================
        // 3. ADIM: DURMADAN SAĞA DÖN
        // ==========================================
        anim.ResetTrigger("TurnLeftTrigger");
        anim.SetTrigger("TurnRightTrigger");

        yield return new WaitForSeconds(1.5f);

        // ==========================================
        // 4. ADIM: DURMADAN YÜRÜMEYE DEVAM ET
        // ==========================================
        agent.updateRotation = true;
        agent.isStopped = false;

        // Son hedefe git emri
        agent.SetDestination(sonHedef.position);

        anim.ResetTrigger("TurnRightTrigger");
        anim.SetTrigger("WalkTrigger");
    }
}