using UnityEngine;
using System.Collections.Generic;

public class TrafikHavuzu : MonoBehaviour
{
    public static TrafikHavuzu Instance;

    [Header("Havuz Ayarları")]
    [Tooltip("Sahnede kapalı (tiki kaldırılmış) halde duran tüm araçları buraya sürükle.")]
    public List<GameObject> aracHavuzu;

    private Dictionary<GameObject, RCCP_AI> aiBilesenleri = new Dictionary<GameObject, RCCP_AI>();

    void Awake()
    {
        Instance = this;

        // Oyun başlarken tüm araçların AI'larını sözlüğe ekle
        foreach (GameObject arac in aracHavuzu)
        {
            if (arac != null)
            {
                aiBilesenleri.Add(arac, arac.GetComponent<RCCP_AI>());
                // DİKKAT: arac.SetActive(false); satırını SİLDİK! 
                // Çünkü araçları Inspector'dan zaten kapattık.
            }
        }
    }

    public GameObject MüsaitAracVer()
    {
        foreach (GameObject arac in aracHavuzu)
        {
            if (!arac.activeInHierarchy) return arac;
        }
        return null;
    }

    public RCCP_AI AIBul(GameObject arac)
    {
        aiBilesenleri.TryGetValue(arac, out RCCP_AI ai);
        return ai;
    }
}