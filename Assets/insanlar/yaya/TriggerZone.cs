using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    [Header("Bağlantılar")]
    public WaypointPatrol hareketEdecekKarakter;
    public string tetikleyiciTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (HiyerarsideTagAra(other.transform, tetikleyiciTag))
        {
            if (hareketEdecekKarakter != null)
            {
                hareketEdecekKarakter.HareketiBaslat();
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