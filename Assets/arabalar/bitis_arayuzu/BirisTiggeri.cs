using System.Collections; // Coroutine (Zamanlayıcı) için zorunlu
using UnityEngine;
using UnityEngine.SceneManagement;

public class BirisTiggeri : MonoBehaviour
{
    [Header("UI Ayarları")]
    public GameObject endScreenPanel; // Görünecek olan UI Paneli

    [Header("Zaman Ayarı")]
    public float beklemeSuresi = 3f; // Ekranda yazının kaç saniye kalacağı

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.CompareTag("Player"))
        {
            ActivateEndScreen();
        }
    }

    void ActivateEndScreen()
    {
        endScreenPanel.SetActive(true); // UI panelini görünür yap
        Time.timeScale = 0f; // Oyunu arkada durdur

        // Zamanlayıcıyı başlatıyoruz (Bu fonksiyon otomatik geçişi sağlayacak)
        StartCoroutine(ZamanlaVeGec());
    }

    IEnumerator ZamanlaVeGec()
    {
        // Oyun durduğu için (timeScale = 0) normal WaitForSeconds çalışmaz.
        // Bu yüzden gerçek dünya zamanını sayması için "Realtime" kullanıyoruz.
        yield return new WaitForSecondsRealtime(beklemeSuresi);

        Time.timeScale = 1f; // Sonraki sahne için zamanı normale döndür

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Eğer sonraki sahne Build Settings'te varsa yükle
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogError("Sonraki sahne Build Settings listesinde bulunamadı!");
        }
    }
}