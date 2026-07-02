using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // URP Post-Processing bileşenleri için gerekli

public class VRSteeringVignette : MonoBehaviour
{
    [Header("Bağlantılar")]
    [Tooltip("Sahnede kullandığınız RCCP araba kontrolcü scripti")]
    public RCCP_CarController carController;

    [Tooltip("Önceki adımda oluşturduğumuz Global Volume")]
    public Volume postProcessVolume;

    [Header("Vignette (Karartma) İnce Ayarları")]
    [Range(0f, 1f)]
    [Tooltip("Direksiyon tam kırıldığında kenarlar maksimum ne kadar kararsın? (Öneri: 0.5 - 0.6)")]
    public float maxVignetteIntensity = 0.55f;

    [Tooltip("Kararmanın ekrana gelme ve kaybolma hızı. Yüksek değer = Daha hızlı tepki")]
    public float changeSpeed = 6f;

    private Vignette vignette;

    void Start()
    {
        // Eğer editörden Global Volume sürüklenmediyse sahneden otomatik bulmaya çalışır
        if (postProcessVolume == null)
            postProcessVolume = FindObjectOfType<Volume>();

        // Volume içindeki Vignette efektine erişiyoruz
        if (postProcessVolume != null && postProcessVolume.profile.TryGet<Vignette>(out var tempVignette))
        {
            vignette = tempVignette;
            vignette.active = true; // Efektin açık olduğundan emin oluyoruz
        }
        else
        {
            Debug.LogError("VRSteeringVignette: Global Volume veya içerisindeki Vignette efekti bulunamadı! Lütfen kontrolleri yapın.");
        }
    }

    void Update()
    {
        // Gerekli bileşenler eksikse hata vermemesi için kodu durdurur
        if (carController == null || vignette == null) return;

        // RCCP'den anlık direksiyon girdisini alıyoruz (-1 ile +1 arasında bir değer döner)
        // Mathf.Abs kullanarak eksi değerleri artı yapıyoruz (Sağa da sola da dönse kenarlar kararsın diye)
        float steerInput = Mathf.Abs(carController.steerInput_V);

        // Direksiyon açısına göre hedef karartma oranını hesaplıyoruz
        float targetIntensity = steerInput * maxVignetteIntensity;

        // Ani göz kırpma hissi yaratmaması için Lerp ile yumuşak bir geçiş sağlıyoruz
        vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetIntensity, Time.deltaTime * changeSpeed);
    }
}