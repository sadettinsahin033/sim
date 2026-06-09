using UnityEngine;

public class NpcTetikleyici : MonoBehaviour
{
    // Inspector'dan bizim NPC'yi buraya sürükleyeceğiz
    public OzelNpcKontrol npcScripti;

    void OnTriggerEnter(Collider other)
    {
        // Trigger alanına giren obje oyuncu ise (Tag'inin "Player" olduğunu varsayıyoruz)
        if (other.CompareTag("Player"))
        {
            npcScripti.HareketeGec();

            // Senaryo sadece bir kez tetiklensin, sürekli tekrarlamasın istiyorsan bu objeyi kapat
            gameObject.SetActive(false);
        }
    }
}
