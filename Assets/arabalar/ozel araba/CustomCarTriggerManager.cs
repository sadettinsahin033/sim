using UnityEngine;

public class SimpleCarTrigger : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Tetikleyecek aracın etiketi")]
    public string playerTag = "Player";

    [Tooltip("Kapanacak olan engel (Küp) objesi")]
    public GameObject customCarCube;

    private void OnTriggerEnter(Collider other)
    {
        // Çarpışan objenin Player olup olmadığını kök objeye kadar kontrol et
        if (CheckForPlayerTag(other.transform))
        {
            // Player tetikleyiciye girdiğinde küpü kapat
            if (customCarCube != null)
            {
                customCarCube.SetActive(false);
            }
        }
    }

    // Çarpışan objenin kendisinden başlayıp en üst ana objesine (Root) kadar Tag kontrolü yapar
    private bool CheckForPlayerTag(Transform hitTransform)
    {
        Transform current = hitTransform;
        while (current != null)
        {
            if (current.CompareTag(playerTag))
            {
                return true; // Player tag'i bulundu!
            }
            current = current.parent; // Bir üst objeye geç
        }
        return false; // Hiçbir ebeveynde Player tag'i bulunamadı
    }
}