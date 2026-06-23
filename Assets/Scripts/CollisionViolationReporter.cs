using System.Collections.Generic;
using UnityEngine;

public class CollisionViolationReporter : MonoBehaviour
{
    [System.Serializable]
    public class CollisionGroup
    {
        [Tooltip("Bu grubun görünen adýdýr. Örneðin: Yayalar, Binalar, Aðaçlar, Araçlar.")]
        [InspectorName("Grup Adý")]
        public string groupName = "Yayalar";

        [Tooltip("Bu gruba ait parent objeleri buraya sürükle. Örneðin Pedestrians parent objesini eklersen altýndaki tüm yayalar bu gruba dahil olur.")]
        [InspectorName("Grup Parent Objeleri")]
        public List<Transform> groupParents = new List<Transform>();

        [Tooltip("Ýstersen tek tek objeleri de buraya ekleyebilirsin. Parent kullanýyorsan burayý boþ býrakabilirsin.")]
        [InspectorName("Tekil Grup Objeleri")]
        public List<Transform> groupObjects = new List<Transform>();

        [Tooltip("Araç bu gruptaki bir objeye çarparsa log açýklamasýna eklenecek mesajdýr.")]
        [TextArea(1, 3)]
        [InspectorName("Çarpýþma Hata Mesajý")]
        public string collisionMessage = "Yayaya çarptý.";
    }

    [System.Serializable]
    public class CollisionGeneralSettings
    {
        [Tooltip("Açýksa araç fiziksel bir çarpýþma yaptýðýnda sistem bunu ihlal olarak bildirir.")]
        [InspectorName("Çarpýþma Kontrolü Aktif")]
        public bool collisionCheckActive = true;

        [Tooltip("Çarpýþmanýn hata sayýlmasý için aracýn minimum hýzý. Hafif temaslarý engellemek için kullanýlýr.")]
        [InspectorName("Minimum Çarpýþma Hýzý km/h")]
        public float minimumCollisionSpeedKmh = 5f;

        [Tooltip("Açýksa ayný objeye tekrar tekrar çarpýnca sürekli log üretmez.")]
        [InspectorName("Ayný Objeyi Tekrar Loglama")]
        public bool ignoreSameObjectAgain = true;

        [Tooltip("Bir çarpýþmadan sonra yeni çarpýþma bildirimi yapýlmasý için geçmesi gereken süre.")]
        [InspectorName("Çarpýþma Bekleme Süresi")]
        public float collisionCooldown = 1.0f;

        [Tooltip("Hiçbir gruba girmeyen bir objeye çarpýlýrsa kullanýlacak genel hata mesajýdýr.")]
        [TextArea(1, 3)]
        [InspectorName("Varsayýlan Çarpýþma Mesajý")]
        public string defaultCollisionMessage = "Araç bir nesneyle çarpýþtý.";

        [Tooltip("Açýksa aktif ana event yoksa çarpýþma kendi baþýna CSV/Console logu yazar.")]
        [InspectorName("Aktif Event Yoksa Kendi Baþýna Log Yaz")]
        public bool logSelfWhenNoActiveEvent = true;

        [Tooltip("Aktif ana event yokken kendi baþýna log yazarsa CSV'de action alanýna yazýlacak olay adýdýr.")]
        [InspectorName("Tek Baþýna Log Olay Adý")]
        public string selfLogActionName = "Çarpýþma";
    }

    [Tooltip("Çarpýþma algýlama için genel ayarlar.")]
    [InspectorName("Çarpýþma Genel Ayarlarý")]
    public CollisionGeneralSettings generalSettings = new CollisionGeneralSettings();

    [Tooltip("Yaya, bina, aðaç, araç gibi çarpýþma gruplarýný burada tanýmlayabilirsin.")]
    [InspectorName("Çarpýþma Gruplarý")]
    public List<CollisionGroup> collisionGroups = new List<CollisionGroup>();

    private DrivingLogManager logger;
    private float lastCollisionTime = -999f;
    private readonly HashSet<GameObject> alreadyReportedObjects = new HashSet<GameObject>();

    private void Awake()
    {
        logger = GetComponentInParent<DrivingLogManager>();

        if (logger == null)
            logger = GetComponent<DrivingLogManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!generalSettings.collisionCheckActive)
            return;

        if (logger == null)
            return;

        if (Time.time - lastCollisionTime < generalSettings.collisionCooldown)
            return;

        float speedKmh = logger.GetSpeedKmh();

        if (speedKmh < generalSettings.minimumCollisionSpeedKmh)
            return;

        GameObject hitObject = collision.gameObject;

        if (hitObject == null)
            return;

        if (generalSettings.ignoreSameObjectAgain && alreadyReportedObjects.Contains(hitObject))
            return;

        string message = GetCollisionMessage(hitObject.transform);

        alreadyReportedObjects.Add(hitObject);
        lastCollisionTime = Time.time;

        bool acceptedByActiveEvent = ScenarioEventTrigger.ReportCollisionViolation(logger, message);

        if (!acceptedByActiveEvent && generalSettings.logSelfWhenNoActiveEvent)
            logger.WriteEventLog(generalSettings.selfLogActionName, true, message);
    }

    private string GetCollisionMessage(Transform hitTransform)
    {
        if (hitTransform == null)
            return generalSettings.defaultCollisionMessage;

        for (int i = 0; i < collisionGroups.Count; i++)
        {
            CollisionGroup group = collisionGroups[i];

            if (group == null)
                continue;

            if (IsInGroup(hitTransform, group))
            {
                if (!IsBlank(group.collisionMessage))
                    return group.collisionMessage;

                if (!IsBlank(group.groupName))
                    return group.groupName + " grubuna çarptý.";
            }
        }

        return generalSettings.defaultCollisionMessage;
    }

    private bool IsInGroup(Transform hitTransform, CollisionGroup group)
    {
        for (int i = 0; i < group.groupParents.Count; i++)
        {
            Transform parent = group.groupParents[i];

            if (parent == null)
                continue;

            if (hitTransform == parent || hitTransform.IsChildOf(parent))
                return true;
        }

        for (int i = 0; i < group.groupObjects.Count; i++)
        {
            Transform obj = group.groupObjects[i];

            if (obj == null)
                continue;

            if (hitTransform == obj || hitTransform.IsChildOf(obj))
                return true;
        }

        return false;
    }

    private bool IsBlank(string value)
    {
        return string.IsNullOrEmpty(value) || value.Trim().Length == 0;
    }

    [ContextMenu("Tekrar Loglanan Objeleri Temizle")]
    public void ClearReportedObjects()
    {
        alreadyReportedObjects.Clear();
    }
}