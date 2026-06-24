using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    [Header("Bağlantılar")]
    public WaypointPatrol hareketEdecekKarakter;
    public string tetikleyiciTag = "Player";

    // YENİ EKLEDİĞİMİZ ALAN: NPC'ye ait olan Cube objesini buraya sürükleyeceksin
    [Header("NPC Bileşen Ayarları")]
    public GameObject cubeObjesi;

    private void OnTriggerEnter(Collider other)
    {
        if (HiyerarsideTagAra(other.transform, tetikleyiciTag))
        {
            // Karakter atanmışsa hareketi başlat
            if (hareketEdecekKarakter != null)
            {
                hareketEdecekKarakter.HareketiBaslat();
            }

            // YENİ EKLEDİĞİMİZ KONTROL: Eğer Cube objesi atanmışsa onu aktif et
            if (cubeObjesi != null)
            {
                cubeObjesi.SetActive(true);
            }
        }
    }

    private bool HiyerarsideTagAra(Transform baslangicNoktasi, string arananTag)
    {
        Transform anlikObje = baslangicNoktasi;

        while (anlikObje != null)
        {
            if (anlikObje.CompareTag(arananTag))
            {
                return true;
            }

            anlikObje = anlikObje.parent;
        }

        return false;
    }
}