using UnityEngine;

public class RainTrigger : MonoBehaviour
{
    [Header("Yağmur Ayarları")]
    [Tooltip("Sahnede bulunan RainSystem partikül objesini buraya sürükleyin")]
    public ParticleSystem rainParticleSystem;

    [Tooltip("Bu alana girildiğinde yağmur sıklığı (Rate over Time) kaç olsun?")]
    public float targetRainIntensity = 200f;

    private void OnTriggerEnter(Collider other)
    {
        // other.transform.root ile çarpan objenin en üst (ana) ebeveynine gidip tag kontrolü yapıyoruz
        if (other.transform.root.CompareTag("Player"))
        {
            if (rainParticleSystem != null)
            {
                // Partikül sisteminin emission modülüne ulaş
                var emission = rainParticleSystem.emission;

                // Rate over time değerini senin yazdığın sayıya eşitle
                emission.rateOverTime = targetRainIntensity;

                // İsteğe bağlı: Obje bir kere tetiklendikten sonra kendini kapatsın
                gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("RainTrigger scriptinde Particle System eksik!");
            }
        }
    }
}