using UnityEngine;

public class TirTetikleyici : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private RTC_CarController hedefTir; // Harekete geçecek tır
    [SerializeField] private string anaNesneTagi = "Player"; // En üstteki ana nesnenin tag'i

    private bool tetiklendiMi = false;

    void Start()
    {
        if (hedefTir != null)
        {
            // Tırın yapay zekasını oyun başında kapatıyoruz
            hedefTir.enabled = false;
        }
        else
        {
            Debug.LogError("Lütfen Script üzerindeki 'Hedef Tir' alanına tırınızı atayın!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Tetikleyiciye giren objenin hiyerarşideki EN ÜST (ana) nesnesini bulur
        Transform anaNesne = other.transform.root;

        // Ana nesnenin tag'i belirlediğimiz tag ("Player") ise ve henüz tetiklenmediyse
        if (anaNesne.CompareTag(anaNesneTagi) && !tetiklendiMi)
        {
            tetiklendiMi = true;
            TiriHareketeGecir();
        }
    }

    private void TiriHareketeGecir()
    {
        if (hedefTir != null)
        {
            // Yapay zekayı açıyoruz, tır hareket ediyor
            hedefTir.enabled = true;
            Debug.Log("Ana obje doğrulandı. Tır harekete geçti!");
        }
    }
}