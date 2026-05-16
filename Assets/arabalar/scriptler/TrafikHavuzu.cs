using UnityEngine;
using System.Collections.Generic;

public class TrafikHavuzu : MonoBehaviour
{
    public static TrafikHavuzu Instance;

    [Header("Havuz Ayarları")]
    public List<GameObject> aracHavuzu;

    private Dictionary<GameObject, RCCP_AI> aiBilesenleri = new Dictionary<GameObject, RCCP_AI>();

    // Havuzda hangi sırada kaldığımızı aklında tutan değişken
    private int siradakiIndeks = 0;

    void Awake()
    {
        Instance = this;
        foreach (GameObject arac in aracHavuzu)
        {
            if (arac != null)
            {
                aiBilesenleri.Add(arac, arac.GetComponent<RCCP_AI>());
            }
        }
    }

    // DİKKAT: İsim diğer scriptler bozulmasın diye aynı bırakıldı, 
    // ama artık rastgele değil, SIRAYLA (Round-Robin) araç veriyor!
    public GameObject RastgeleMusaitAracVer()
    {
        // Havuzdaki araç sayısı kadar döngü yap
        for (int i = 0; i < aracHavuzu.Count; i++)
        {
            // Sıradaki indeksi hesapla
            int kontrolIndeksi = (siradakiIndeks + i) % aracHavuzu.Count;
            GameObject arac = aracHavuzu[kontrolIndeksi];

            // Eğer bu sıradaki araç kapalıysa (müsaitse) onu ver
            if (arac != null && !arac.activeInHierarchy)
            {
                // Bir sonraki araç çağırma işlemi için sırayı bir ileri kaydır
                siradakiIndeks = (kontrolIndeksi + 1) % aracHavuzu.Count;
                return arac;
            }
        }

        // Eğer döngü biter ve boşta hiç araç bulamazsa null döner
        return null;
    }

    public RCCP_AI AIBul(GameObject arac)
    {
        aiBilesenleri.TryGetValue(arac, out RCCP_AI ai);
        return ai;
    }
}