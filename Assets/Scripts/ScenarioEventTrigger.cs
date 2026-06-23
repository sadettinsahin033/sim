using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FCG;

public class ScenarioEventTrigger : MonoBehaviour
{
    public enum EventActionType
    {
        SabitSurus,
        Hizlanma,
        YavaslamaFrenleme,
        SolSeritDegistirme,
        SagSeritDegistirme,
        SolaDonus,
        SagaDonus,
        Sollama,
        Durma,
        YayayaYolVerme,
        HizSiniriKontrolu,
        AniFrenTepkisi,
        NavigasyonTakibi,
        AmbulansaYolVerme,
        KarsiSeritIhlali,
        YolDisi,
        CizgiIhlali,
        KoridorGecisIhlali,
        Carpisma
    }

    public enum SignalRequirement
    {
        Farketmez,
        Yok,
        Sol,
        Sag
    }

    public enum SteeringDirection
    {
        Sol,
        Sag
    }

    public enum ViolationType
    {
        YolDisi,
        KarsiSerit,
        CizgiIhlali,
        KoridorGecisi,
        Carpisma
    }

    public enum DistanceMeasureMode
    {
        Kapali,
        IlkFren,
        IlkDurma,
        IlkFrenVeIlkDurma
    }

    [System.Serializable]
    public class GeneralSettings
    {
        [Tooltip("Bu trigger'ın hangi sürüş olayını temsil ettiğini seçer. CSV'de action alanına bu olay adı yazılır.")]
        [InspectorName("Olay Türü")]
        public EventActionType actionType = EventActionType.SabitSurus;

        [Tooltip("Açıksa bu trigger kendi sonucunu CSV ve Console'a yazar. Ana event trigger'larında açık olur. Sensör trigger'larında genelde kapalı tutulur.")]
        [InspectorName("Bu Trigger Sonuç Logu Yazsın")]
        public bool writeResultLog = true;

        [Tooltip("Açıksa bu trigger sadece hata oluştuğunda log yazar. Hata yoksa başarı mesajı yazmaz. Yol dışı, karşı şerit, çizgi ihlali gibi sensörlerde kullanışlıdır.")]
        [InspectorName("Sadece Hata Varsa Log Yaz")]
        public bool logOnlyIfError = false;

        [Tooltip("Açıksa bu trigger bir ihlal ölçtüğünde aktif bir ana event yoksa kendi başına hata logu yazar. Örneğin aktif sabit sürüş yokken yol dışı trigger'ı kendi logunu basabilir.")]
        [InspectorName("Aktif Event Yoksa Kendi Başına Log Yaz")]
        public bool logSelfWhenNoActiveEvent = false;

        [Tooltip("Açıksa değerlendirme araç trigger'dan çıkınca yapılır. Süreye bağlı kontrollerde genelde açık kalmalıdır.")]
        [InspectorName("Trigger Çıkışında Değerlendir")]
        public bool evaluateOnExit = true;

        [Tooltip("Açıksa bu trigger oyun çalışması boyunca yalnızca bir kez log yazar. Test sırasında tekrar tekrar log almak için kapatılabilir.")]
        [InspectorName("Bir Kez Logla")]
        public bool logOnlyOnce = true;

        [Tooltip("Tüm seçili şartlar doğruysa yazılacak başarı mesajıdır. Ana event trigger'larında kullanılır.")]
        [TextArea(2, 4)]
        [InspectorName("Başarı Mesajı")]
        public string successMessage = "Olay başarıyla tamamlandı.";

        [Tooltip("Herhangi bir şart bozulursa yazılacak ana hata mesajıdır. Alt hata sebepleri bunun sonuna eklenebilir.")]
        [TextArea(2, 4)]
        [InspectorName("Genel Hata Mesajı")]
        public string generalErrorMessage = "Olay başarısız gerçekleşti.";

        [Tooltip("Açıksa hata açıklamasına her kontrolden gelen özel hata mesajları eklenir. Örneğin: Hız sınırı aşıldı. Araç yol dışına çıktı.")]
        [InspectorName("Hata Sebeplerini Mesaja Ekle")]
        public bool appendErrorReasons = true;
    }

    [System.Serializable]
    public class DistanceMeasureSettings
    {
        [InspectorName("Mesafe Ölçümü Yap")]
        [Tooltip("Açıksa seçilen hedef nesneye göre fren ve/veya durma mesafesi explanation içine eklenir.")]
        public bool measureDistance = false;

        [InspectorName("Mesafe Hedef Nesnesi")]
        [Tooltip("Mesafe bu objeye göre ölçülür. Örneğin kırmızı ışık dur çizgisi için StopLinePoint.")]
        public Transform targetObject;

        [InspectorName("Mesafe Ölçüm Modu")]
        [Tooltip("İlk fren, ilk durma veya ikisini birden ölçer.")]
        public DistanceMeasureMode distanceMeasureMode = DistanceMeasureMode.Kapali;

        [InspectorName("İlk Fren Mesafesi Etiketi")]
        public string firstBrakeDistanceLabel = "İlk fren mesafesi";

        [InspectorName("Durduğu Mesafe Etiketi")]
        public string stoppedDistanceLabel = "Durduğu mesafe";

        [InspectorName("Fren Anındaki Hızı Yaz")]
        public bool includeBrakeSpeed = true;

        [InspectorName("Durma Anındaki Hızı Yaz")]
        public bool includeStopSpeed = false;
    }

    [System.Serializable]
    public class SpeedSettings
    {
        [Tooltip("Açıksa araç hızı minimum ve maksimum hız aralığına göre kontrol edilir.")]
        [InspectorName("Hız Kontrolü Yap")]
        public bool checkSpeed = false;

        [Tooltip("Araç bu hızın altına belirlenen süre boyunca düşerse hız hatası sayılır.")]
        [InspectorName("Minimum Hız km/h")]
        public float minSpeedKmh = 0f;

        [Tooltip("Araç bu hızın üstüne belirlenen süre boyunca çıkarsa hız hatası sayılır.")]
        [InspectorName("Maksimum Hız km/h")]
        public float maxSpeedKmh = 50f;

        [Tooltip("Hız sınırı kaç saniye boyunca ihlal edilirse hata sayılacağını belirler. Anlık hız aşımını yok saymak için kullanılır.")]
        [InspectorName("Hız İhlal Süresi")]
        public float speedViolationDuration = 0.5f;

        [Tooltip("Hız şartı bozulursa açıklamaya eklenecek özel hata mesajıdır.")]
        [TextArea(1, 3)]
        [InspectorName("Hız Hata Mesajı")]
        public string speedErrorMessage = "Hız sınırı aşıldı.";
    }

    [System.Serializable]
    public class StopSettings
    {
        [Tooltip("Açıksa araç bu trigger içinde belirlenen süre kadar durmak zorundadır.")]
        [InspectorName("Durma Gerekli")]
        public bool requireStop = false;

        [Tooltip("Araç bu hızın altındaysa durmuş kabul edilir.")]
        [InspectorName("Duruyor Sayılacak Hız km/h")]
        public float stopSpeedThresholdKmh = 3f;

        [Tooltip("Araç kaç saniye boyunca durmuş kalırsa durma şartı başarılı sayılır.")]
        [InspectorName("Gerekli Durma Süresi")]
        public float requiredStopDuration = 1.5f;

        [Tooltip("Durma şartı gerçekleşmezse açıklamaya eklenecek özel hata mesajıdır.")]
        [TextArea(1, 3)]
        [InspectorName("Durma Hata Mesajı")]
        public string stopErrorMessage = "Gerekli durma davranışı gerçekleşmedi.";
    }

    [System.Serializable]
    public class TrafficLightStopSettings
    {
        [InspectorName("Durma Kontrolünde Trafik Işığını Kullan")]
        [Tooltip("Açıksa durma zorunluluğu seçilen TrafficLight objesinin mevcut rengine göre belirlenir.")]
        public bool useTrafficLightForStop = false;

        [InspectorName("Kontrol Edilecek Trafik Işığı")]
        [Tooltip("Oyuncunun baktığı TrafficLight objesini buraya sürükle. Örneğin trafficLight_N, trafficLight_S, trafficLight_E veya trafficLight_W.")]
        public TrafficLight targetTrafficLight;

        [InspectorName("Kırmızıda Durma Gerekli")]
        public bool requireStopOnRed = true;

        [InspectorName("Sarıda Durma Gerekli")]
        public bool requireStopOnYellow = true;

        [InspectorName("Yaya / Ara Fazda Durma Gerekli")]
        public bool requireStopOnPedestrianPhase = true;

        [InspectorName("Yeşilde Durursa Hata Say")]
        public bool errorIfStoppedOnGreen = true;

        [TextArea(1, 3)]
        [InspectorName("Yeşilde Durma Hata Mesajı")]
        public string greenStopErrorMessage = "Yeşil ışıkta gereksiz durdu.";

        [InspectorName("Kırmızıda Durduktan Sonra Geçerse Hata Say")]
        public bool errorIfPassedBeforeGreenAfterStop = true;

        [TextArea(1, 3)]
        [InspectorName("Kırmızıda Geçiş Hata Mesajı")]
        public string passedBeforeGreenErrorMessage = "Araç kırmızı ışıkta durduktan sonra ışık yeşile dönmeden geçti.";
    }

    [System.Serializable]
    public class BrakeSettings
    {
        [Tooltip("Açıksa fren pedalının belirlenen değerin üstünde ve belirlenen süre boyunca kullanılıp kullanılmadığı kontrol edilir.")]
        [InspectorName("Fren Kontrolü Yap")]
        public bool checkBrake = false;

        [Tooltip("Frenin basılmış sayılması için gereken minimum fren değeri.")]
        [Range(0f, 1f)]
        [InspectorName("Minimum Fren Değeri")]
        public float requiredBrakeValue = 0.5f;

        [Tooltip("Fren şartının geçerli sayılması için minimum basılı kalma süresi.")]
        [InspectorName("Minimum Fren Süresi")]
        public float requiredBrakeDuration = 0.3f;

        [Tooltip("Açıksa fren yapılması başarı değil hata kabul edilir. Yersiz fren ölçmek için kullanılır.")]
        [InspectorName("Fren Koşulunu Tersine Çevir")]
        public bool reverseBrakeCondition = false;

        [Tooltip("Fren şartı bozulursa açıklamaya eklenecek özel hata mesajıdır.")]
        [TextArea(1, 3)]
        [InspectorName("Fren Hata Mesajı")]
        public string brakeErrorMessage = "Beklenen fren davranışı gerçekleşmedi.";
    }

    [System.Serializable]
    public class GasSettings
    {
        [Tooltip("Açıksa gaz pedalının belirlenen değerin üstünde ve belirlenen süre boyunca kullanılıp kullanılmadığı kontrol edilir.")]
        [InspectorName("Gaz Kontrolü Yap")]
        public bool checkGas = false;

        [Tooltip("Gazın basılmış sayılması için gereken minimum gaz değeri.")]
        [Range(0f, 1f)]
        [InspectorName("Minimum Gaz Değeri")]
        public float requiredGasValue = 0.4f;

        [Tooltip("Gaz şartının geçerli sayılması için minimum basılı kalma süresi.")]
        [InspectorName("Minimum Gaz Süresi")]
        public float requiredGasDuration = 0.5f;

        [Tooltip("Açıksa gaz verilmesi başarı değil hata kabul edilir. Gereksiz gaz verme hatalarını ölçmek için kullanılır.")]
        [InspectorName("Gaz Koşulunu Tersine Çevir")]
        public bool reverseGasCondition = false;

        [Tooltip("Gaz şartı bozulursa açıklamaya eklenecek özel hata mesajıdır.")]
        [TextArea(1, 3)]
        [InspectorName("Gaz Hata Mesajı")]
        public string gasErrorMessage = "Beklenen gaz davranışı gerçekleşmedi.";
    }

    [System.Serializable]
    public class GasBrakeTogetherSettings
    {
        [Tooltip("Açıksa gaz ve fren pedallarına aynı anda basılması hata olarak ölçülür.")]
        [InspectorName("Gaz ve Fren Aynı Anda Hata Say")]
        public bool checkGasBrakeTogether = false;

        [Tooltip("Gaz pedalının aynı anda basılmış kabul edilmesi için gereken eşik.")]
        [Range(0f, 1f)]
        [InspectorName("Gaz Eşiği")]
        public float gasTogetherThreshold = 0.3f;

        [Tooltip("Fren pedalının aynı anda basılmış kabul edilmesi için gereken eşik.")]
        [Range(0f, 1f)]
        [InspectorName("Fren Eşiği")]
        public float brakeTogetherThreshold = 0.3f;

        [Tooltip("Gaz ve frenin aynı anda kaç saniye basılı kalırsa hata sayılacağını belirler.")]
        [InspectorName("Gaz Fren Aynı Anda Süresi")]
        public float gasBrakeTogetherDuration = 0.3f;

        [Tooltip("Gaz ve fren aynı anda kullanılırsa açıklamaya eklenecek özel hata mesajıdır.")]
        [TextArea(1, 3)]
        [InspectorName("Gaz Fren Hata Mesajı")]
        public string gasBrakeTogetherErrorMessage = "Gaz ve fren pedalına aynı anda basıldı.";
    }

    [System.Serializable]
    public class SteeringSettings
    {
        [Tooltip("Açıksa direksiyonun beklenen yöne yeterli miktarda ve yeterli süre çevrilip çevrilmediği kontrol edilir.")]
        [InspectorName("Direksiyon Yönü Kontrol Et")]
        public bool checkSteeringDirection = false;

        [Tooltip("Beklenen direksiyon yönüdür. Sol için negatif, sağ için pozitif direksiyon değeri kullanılır.")]
        [InspectorName("Beklenen Direksiyon Yönü")]
        public SteeringDirection expectedSteeringDirection = SteeringDirection.Sol;

        [Tooltip("Direksiyon hareketinin geçerli sayılması için aşılması gereken eşik değeri.")]
        [Range(0f, 1f)]
        [InspectorName("Direksiyon Eşiği")]
        public float steeringThreshold = 0.3f;

        [Tooltip("Direksiyon hareketinin doğru sayılması için beklenen yönde minimum kalma süresi.")]
        [InspectorName("Minimum Direksiyon Süresi")]
        public float requiredSteeringDuration = 0.4f;

        [Tooltip("Açıksa beklenen direksiyon hareketini yapmak başarı değil hata kabul edilir. Gereksiz direksiyon kırma gibi durumlarda kullanılır.")]
        [InspectorName("Direksiyon Koşulunu Tersine Çevir")]
        public bool reverseSteeringCondition = false;

        [Tooltip("Direksiyon şartı bozulursa açıklamaya eklenecek özel hata mesajıdır.")]
        [TextArea(1, 3)]
        [InspectorName("Direksiyon Hata Mesajı")]
        public string steeringErrorMessage = "Beklenen direksiyon hareketi gerçekleşmedi.";
    }

    [System.Serializable]
    public class SignalSettings
    {
        [Tooltip("Beklenen sinyal davranışıdır. Farketmez seçilirse sinyal kontrolü yapılmaz. Yok seçilirse sinyal verilmesi hata kabul edilir.")]
        [InspectorName("Beklenen Sinyal")]
        public SignalRequirement requiredSignal = SignalRequirement.Farketmez;

        [Tooltip("Beklenen sinyalin doğru sayılması veya istenmeyen sinyalin hata sayılması için minimum yanma süresi.")]
        [InspectorName("Minimum Sinyal Süresi")]
        public float requiredSignalDuration = 0.5f;

        [Tooltip("Sinyal şartı bozulursa açıklamaya eklenecek özel hata mesajıdır.")]
        [TextArea(1, 3)]
        [InspectorName("Sinyal Hata Mesajı")]
        public string signalErrorMessage = "Beklenen sinyal davranışı gerçekleşmedi.";
    }

    [System.Serializable]
    public class OffRoadSettings
    {
        [Tooltip("Açıksa bu trigger yol dışı sensörü gibi davranır. Araç bu trigger içinde belirtilen süre kalırsa yol dışı ihlali ölçülür.")]
        [InspectorName("Yol Dışı Ölçümü Yap")]
        public bool measureOffRoad = false;

        [Tooltip("Araç bu trigger içinde kaç saniye kalırsa yol dışı ihlali sayılacağını belirler.")]
        [InspectorName("Yol Dışı İhlal Süresi")]
        public float offRoadViolationDuration = 0.5f;

        [Tooltip("Açıksa başka yol dışı sensörlerinden gelen ihlalleri bu trigger kendi sonucuna dahil eder. Ana event trigger'larında kullanılır.")]
        [InspectorName("Yol Dışı İhlalini Dinle")]
        public bool listenOffRoadViolation = false;

        [Tooltip("Yol dışı ihlali oluşursa açıklamaya eklenecek özel hata mesajıdır.")]
        [TextArea(1, 3)]
        [InspectorName("Yol Dışı Hata Mesajı")]
        public string offRoadErrorMessage = "Araç yol dışına çıktı.";
    }

    [System.Serializable]
    public class WrongLaneSettings
    {
        [Tooltip("Açıksa bu trigger karşı şerit sensörü gibi davranır. Araç bu trigger içinde belirtilen süre kalırsa karşı şerit ihlali ölçülür.")]
        [InspectorName("Karşı Şerit Ölçümü Yap")]
        public bool measureWrongLane = false;

        [Tooltip("Araç bu trigger içinde kaç saniye kalırsa karşı şerit ihlali sayılacağını belirler.")]
        [InspectorName("Karşı Şerit İhlal Süresi")]
        public float wrongLaneViolationDuration = 0.5f;

        [Tooltip("Açıksa başka karşı şerit sensörlerinden gelen ihlalleri bu trigger kendi sonucuna dahil eder. Ana event trigger'larında kullanılır.")]
        [InspectorName("Karşı Şerit İhlalini Dinle")]
        public bool listenWrongLaneViolation = false;

        [Tooltip("Karşı şerit ihlali oluşursa açıklamaya eklenecek özel hata mesajıdır.")]
        [TextArea(1, 3)]
        [InspectorName("Karşı Şerit Hata Mesajı")]
        public string wrongLaneErrorMessage = "Araç karşı şeride geçti.";
    }

    [System.Serializable]
    public class LineViolationSettings
    {
        [Tooltip("Açıksa bu trigger çizgi ihlali sensörü gibi davranır. Araç bu trigger içinde belirtilen süre kalırsa çizgi ihlali ölçülür.")]
        [InspectorName("Çizgi İhlali Ölçümü Yap")]
        public bool measureLineViolation = false;

        [Tooltip("Araç bu trigger içinde kaç saniye kalırsa çizgi ihlali sayılacağını belirler.")]
        [InspectorName("Çizgi İhlal Süresi")]
        public float lineViolationDuration = 0.5f;

        [Tooltip("Açıksa başka çizgi ihlali sensörlerinden gelen ihlalleri bu trigger kendi sonucuna dahil eder. Ana event trigger'larında kullanılır.")]
        [InspectorName("Çizgi İhlalini Dinle")]
        public bool listenLineViolation = false;

        [Tooltip("Çizgi ihlali oluşursa açıklamaya eklenecek özel hata mesajıdır.")]
        [TextArea(1, 3)]
        [InspectorName("Çizgi İhlali Hata Mesajı")]
        public string lineViolationErrorMessage = "Araç şerit çizgisini ihlal etti.";
    }

    [System.Serializable]
    public class CorridorSettings
    {
        [Tooltip("Açıksa bu trigger dönüş koridoru kontrolüne dahil olur. Aynı grup ID'ye sahip farklı koridor numaraları arasında geçiş hata olarak ölçülür.")]
        [InspectorName("Koridor Geçiş Ölçümü Yap")]
        public bool measureCorridorTransition = false;

        [Tooltip("Açıksa başka koridor trigger'larından gelen geçiş ihlalini bu trigger kendi sonucuna dahil eder.")]
        [InspectorName("Koridor Geçişini Dinle")]
        public bool listenCorridorTransition = false;

        [Tooltip("Aynı dönüşe ait koridor trigger'larının grup adı aynı olmalıdır. Örneğin: Kavsak_1_SolaDonus.")]
        [InspectorName("Koridor Grup ID")]
        public string corridorGroupId = "Kavsak_1_Donus";

        [Tooltip("Bu trigger'ın temsil ettiği koridor numarasıdır. Aynı dönüşteki iki trigger'dan biri 1, diğeri 2 olmalıdır.")]
        [InspectorName("Bu Trigger Koridor No")]
        public int corridorNo = 1;

        [Tooltip("Araç başladığı koridordan diğer koridora geçip bu süre kadar kalırsa koridor geçiş ihlali sayılır.")]
        [InspectorName("Koridor Geçiş İhlal Süresi")]
        public float corridorTransitionDuration = 0.4f;

        [Tooltip("Açıksa araç koridor trigger'larından çıktıktan sonra belirlenen gecikme sonunda başlangıç koridoru sıfırlanır.")]
        [InspectorName("Koridor Başlangıcını Çıkışta Sıfırla")]
        public bool resetCorridorStartOnExit = true;

        [Tooltip("Araç koridor trigger'larından çıktıktan kaç saniye sonra başlangıç koridoru sıfırlansın.")]
        [InspectorName("Koridor Sıfırlama Gecikmesi")]
        public float corridorResetDelay = 1.0f;

        [Tooltip("Koridor değişimi oluşursa açıklamaya eklenecek özel hata mesajıdır.")]
        [TextArea(1, 3)]
        [InspectorName("Koridor Geçiş Hata Mesajı")]
        public string corridorTransitionErrorMessage = "Sürücü dönüş sırasında başladığı şerit koridorunu değiştirerek hatalı dönüş yaptı.";
    }

    [System.Serializable]
    public class CollisionSettings
    {
        [Tooltip("Açıksa dışarıdan bildirilen çarpışma ihlallerini bu trigger kendi sonucuna dahil eder. Çarpışmayı algılayan script arabada olmalıdır.")]
        [InspectorName("Çarpışma İhlalini Dinle")]
        public bool listenCollisionViolation = false;

        [Tooltip("Çarpışma özel mesaj göndermediyse açıklamaya eklenecek genel hata mesajıdır.")]
        [TextArea(1, 3)]
        [InspectorName("Çarpışma Hata Mesajı")]
        public string collisionErrorMessage = "Araç çarpışma gerçekleştirdi.";
    }

    [System.Serializable]
    public class ViolationSummarySettings
    {
        [Tooltip("Açıksa ihlal sayısı ve toplam ihlal süresi explanation içine eklenir. CSV kolonu değişmez.")]
        [InspectorName("İhlal Özeti Yaz")]
        public bool appendViolationSummary = true;

        [Tooltip("Açıksa her ihlal için kaç kez yapıldığı yazılır.")]
        [InspectorName("İhlal Sayısını Yaz")]
        public bool appendViolationCount = true;

        [Tooltip("Açıksa süreli ihlaller için toplam süre yazılır.")]
        [InspectorName("Toplam İhlal Süresini Yaz")]
        public bool appendViolationDuration = true;

        [Tooltip("Açıksa durma gerçekleştiyse durma süresi explanation içine eklenir.")]
        [InspectorName("Durma Süresini Yaz")]
        public bool appendStopDuration = true;
    }

    [Tooltip("Genel event ayarları. Log yazma, başarı mesajı ve genel hata mesajı burada bulunur.")]
    [InspectorName("Genel Olay Ayarları")]
    public GeneralSettings generalSettings = new GeneralSettings();

    [InspectorName("Mesafe Ölçüm Ayarları")]
    public DistanceMeasureSettings distanceMeasureSettings = new DistanceMeasureSettings();

    [InspectorName("İhlal Özeti Ayarları")]
    public ViolationSummarySettings violationSummarySettings = new ViolationSummarySettings();

    [Tooltip("Hız sınırı ve hız ihlali ayarları.")]
    [InspectorName("Hız Kontrol Ayarları")]
    public SpeedSettings speedSettings = new SpeedSettings();

    [Tooltip("Durma şartı ve durma süresi ayarları.")]
    [InspectorName("Durma Kontrol Ayarları")]
    public StopSettings stopSettings = new StopSettings();

    [InspectorName("Trafik Işığı Durma Ayarları")]
    public TrafficLightStopSettings trafficLightStopSettings = new TrafficLightStopSettings();

    [Tooltip("Fren davranışı ölçüm ayarları.")]
    [InspectorName("Fren Kontrol Ayarları")]
    public BrakeSettings brakeSettings = new BrakeSettings();

    [Tooltip("Gaz davranışı ölçüm ayarları.")]
    [InspectorName("Gaz Kontrol Ayarları")]
    public GasSettings gasSettings = new GasSettings();

    [Tooltip("Gaz ve fren pedalına aynı anda basma hatası ayarları.")]
    [InspectorName("Gaz Fren Birlikte Ayarları")]
    public GasBrakeTogetherSettings gasBrakeTogetherSettings = new GasBrakeTogetherSettings();

    [Tooltip("Direksiyon yönü ve direksiyon süresi ayarları.")]
    [InspectorName("Direksiyon Kontrol Ayarları")]
    public SteeringSettings steeringSettings = new SteeringSettings();

    [Tooltip("Sinyal yönü ve sinyal süresi ayarları.")]
    [InspectorName("Sinyal Kontrol Ayarları")]
    public SignalSettings signalSettings = new SignalSettings();

    [Tooltip("Yol dışı ölçümü ve yol dışı ihlalini dinleme ayarları.")]
    [InspectorName("Yol Dışı Kontrol Ayarları")]
    public OffRoadSettings offRoadSettings = new OffRoadSettings();

    [Tooltip("Karşı şerit ölçümü ve karşı şerit ihlalini dinleme ayarları.")]
    [InspectorName("Karşı Şerit Kontrol Ayarları")]
    public WrongLaneSettings wrongLaneSettings = new WrongLaneSettings();

    [Tooltip("Çizgi ihlali ölçümü ve çizgi ihlalini dinleme ayarları.")]
    [InspectorName("Çizgi İhlali Kontrol Ayarları")]
    public LineViolationSettings lineViolationSettings = new LineViolationSettings();

    [Tooltip("Dönüşte 1 ve 2 numaralı koridorlar arası geçiş ihlali ayarları.")]
    [InspectorName("Koridor Geçiş Ayarları")]
    public CorridorSettings corridorSettings = new CorridorSettings();

    [Tooltip("Çarpışma ihlalini dinleme ayarları.")]
    [InspectorName("Çarpışma Kontrol Ayarları")]
    public CollisionSettings collisionSettings = new CollisionSettings();

    private bool hasLogged = false;

    private class ViolationCounter
    {
        public int count;
        public float totalDuration;
        public float currentDuration;
        public bool currentCounted;
    }

    private class LocalState
    {
        public bool inside;
        public int triggerContactCount;
        public int lastUpdateFrame = -1;

        public bool hasTrafficLightColor;
        public TrafficLight.CurrentLightColor lastTrafficLightColor;

        public bool firstBrakeDistanceMeasured;
        public float firstBrakeDistance = -1f;
        public float firstBrakeSpeed = -1f;

        public bool stoppedDistanceMeasured;
        public float stoppedDistance = -1f;
        public float stoppedSpeed = -1f;

        public float speedBadTimer;
        public bool speedError;
        public ViolationCounter speedViolation = new ViolationCounter();

        public float stopTimer;
        public bool stopSuccess;
        public float longestStopDuration;

        public float brakeTimer;
        public bool brakeSuccess;
        public bool brakeError;
        public ViolationCounter brakeViolation = new ViolationCounter();

        public float gasTimer;
        public bool gasSuccess;
        public bool gasError;
        public ViolationCounter gasViolation = new ViolationCounter();

        public float gasBrakeTogetherTimer;
        public bool gasBrakeTogetherError;
        public ViolationCounter gasBrakeTogetherViolation = new ViolationCounter();

        public float steeringTimer;
        public bool steeringSuccess;
        public bool steeringError;
        public ViolationCounter steeringViolation = new ViolationCounter();

        public float signalTimer;
        public bool signalSuccess;
        public bool signalError;
        public ViolationCounter signalViolation = new ViolationCounter();

        public float offRoadTimer;
        public bool offRoadMeasured;
        public ViolationCounter offRoadViolation = new ViolationCounter();
        public bool offRoadReportedToActiveEvent;

        public float wrongLaneTimer;
        public bool wrongLaneMeasured;
        public ViolationCounter wrongLaneViolation = new ViolationCounter();
        public bool wrongLaneReportedToActiveEvent;

        public float lineViolationTimer;
        public bool lineViolationMeasured;
        public ViolationCounter lineViolation = new ViolationCounter();
        public bool lineViolationReportedToActiveEvent;

        public bool externalOffRoad;
        public bool externalWrongLane;
        public bool externalLineViolation;
        public bool externalCorridorTransition;
        public bool externalCollision;

        public int collisionCount;
        public int corridorViolationCount;

        public bool selfLogBecauseNoActiveEvent;

        public List<string> errorReasons = new List<string>();
    }

    private class CorridorState
    {
        public int startCorridorNo;
        public float otherCorridorTimer;
        public bool violationSent;
        public int activeContactCount;
    }

    private readonly Dictionary<DrivingLogManager, LocalState> localStates =
        new Dictionary<DrivingLogManager, LocalState>();

    private static readonly Dictionary<DrivingLogManager, List<ScenarioEventTrigger>> activeEventTriggers =
        new Dictionary<DrivingLogManager, List<ScenarioEventTrigger>>();

    private static readonly Dictionary<string, CorridorState> corridorStates =
        new Dictionary<string, CorridorState>();

    private void Reset()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        DrivingLogManager logger = GetLogger(other);
        if (logger == null)
            return;

        LocalState state = GetOrCreateState(logger);
        state.inside = true;
        state.triggerContactCount++;

        if (state.triggerContactCount == 1)
        {
            if (ShouldRegisterAsActiveEvent())
                RegisterActiveEvent(logger, this);

            RegisterCorridorEnter(logger);
        }

        UpdateAllChecksOncePerFrame(logger, state);
    }

    private void OnTriggerStay(Collider other)
    {
        DrivingLogManager logger = GetLogger(other);
        if (logger == null)
            return;

        LocalState state = GetOrCreateState(logger);
        state.inside = true;

        UpdateAllChecksOncePerFrame(logger, state);
    }

    private void OnTriggerExit(Collider other)
    {
        DrivingLogManager logger = GetLogger(other);
        if (logger == null)
            return;

        if (localStates.TryGetValue(logger, out LocalState state))
        {
            state.triggerContactCount--;

            if (state.triggerContactCount > 0)
                return;

            if (state.triggerContactCount < 0)
                state.triggerContactCount = 0;

            UpdateAllChecksOncePerFrame(logger, state);
            FinalizeMeasuredSensorViolations(logger, state);
            FinalizeAllViolationCounters(state);

            bool shouldEvaluate = generalSettings.writeResultLog || state.selfLogBecauseNoActiveEvent;

            if (shouldEvaluate && generalSettings.evaluateOnExit)
                EvaluateAndLog(logger, state);

            state.inside = false;
            localStates.Remove(logger);
        }

        RegisterCorridorExit(logger);

        if (ShouldRegisterAsActiveEvent())
            UnregisterActiveEvent(logger, this);
    }

    private void UpdateAllChecksOncePerFrame(DrivingLogManager logger, LocalState state)
    {
        if (state.lastUpdateFrame == Time.frameCount)
            return;

        state.lastUpdateFrame = Time.frameCount;
        UpdateAllChecks(logger, state);
    }

    private void UpdateAllChecks(DrivingLogManager logger, LocalState state)
    {
        UpdateTrafficLightColorState(state);

        if (generalSettings.writeResultLog || generalSettings.logSelfWhenNoActiveEvent)
        {
            UpdateSpeedCheck(logger, state);
            UpdateStopCheck(logger, state);
            UpdateBrakeCheck(logger, state);
            UpdateGasCheck(logger, state);
            UpdateGasBrakeTogetherCheck(logger, state);
            UpdateSteeringCheck(logger, state);
            UpdateSignalCheck(logger, state);
        }

        UpdateViolationMeasurements(logger, state);
        UpdateCorridorMeasurement(logger, state);
    }

    private void UpdateSpeedCheck(DrivingLogManager logger, LocalState state)
    {
        if (!speedSettings.checkSpeed)
            return;

        float speed = logger.GetSpeedKmh();
        bool speedBad = speed < speedSettings.minSpeedKmh || speed > speedSettings.maxSpeedKmh;

        UpdateViolationCounter(state.speedViolation, speedBad, speedSettings.speedViolationDuration);

        if (state.speedViolation.count > 0)
        {
            state.speedError = true;
            AddReason(state, speedSettings.speedErrorMessage);
        }

        state.speedBadTimer = state.speedViolation.currentDuration;
    }

    private void UpdateStopCheck(DrivingLogManager logger, LocalState state)
    {
        if (!stopSettings.requireStop)
            return;

        float speed = logger.GetSpeedKmh();

        if (speed <= stopSettings.stopSpeedThresholdKmh)
        {
            state.stopTimer += Time.deltaTime;

            if (state.stopTimer > state.longestStopDuration)
                state.longestStopDuration = state.stopTimer;

            if (state.stopTimer >= stopSettings.requiredStopDuration)
            {
                state.stopSuccess = true;
                TryMeasureStoppedDistance(logger, state);
            }
        }
        else
        {
            state.stopTimer = 0f;
        }
    }

    private bool TryGetCurrentTrafficLightColor(out TrafficLight.CurrentLightColor currentColor)
    {
        currentColor = TrafficLight.CurrentLightColor.Red;

        if (!trafficLightStopSettings.useTrafficLightForStop)
            return false;

        if (trafficLightStopSettings.targetTrafficLight == null)
            return false;

        currentColor = trafficLightStopSettings.targetTrafficLight.GetCurrentColor();
        return true;
    }

    private void UpdateTrafficLightColorState(LocalState state)
    {
        TrafficLight.CurrentLightColor currentColor;

        if (TryGetCurrentTrafficLightColor(out currentColor))
        {
            state.hasTrafficLightColor = true;
            state.lastTrafficLightColor = currentColor;
        }
    }

    private bool ShouldRequireStopAccordingToTrafficLight()
    {
        if (!trafficLightStopSettings.useTrafficLightForStop)
            return true;

        if (trafficLightStopSettings.targetTrafficLight == null)
            return true;

        TrafficLight.CurrentLightColor currentColor =
            trafficLightStopSettings.targetTrafficLight.GetCurrentColor();

        if (currentColor == TrafficLight.CurrentLightColor.Red)
            return trafficLightStopSettings.requireStopOnRed;

        if (currentColor == TrafficLight.CurrentLightColor.Yellow)
            return trafficLightStopSettings.requireStopOnYellow;

        if (currentColor == TrafficLight.CurrentLightColor.Pedestrian)
            return trafficLightStopSettings.requireStopOnPedestrianPhase;

        if (currentColor == TrafficLight.CurrentLightColor.Green)
            return false;

        return true;
    }


    private void UpdateBrakeCheck(DrivingLogManager logger, LocalState state)
    {
        if (!brakeSettings.checkBrake)
            return;

        bool brakePressed = logger.brakeValue >= brakeSettings.requiredBrakeValue;

        if (brakeSettings.reverseBrakeCondition)
        {
            UpdateViolationCounter(state.brakeViolation, brakePressed, brakeSettings.requiredBrakeDuration);

            if (state.brakeViolation.count > 0)
            {
                state.brakeError = true;
                AddReason(state, brakeSettings.brakeErrorMessage);
            }

            state.brakeTimer = state.brakeViolation.currentDuration;
            return;
        }

        if (brakePressed)
        {
            state.brakeTimer += Time.deltaTime;

            if (state.brakeTimer >= brakeSettings.requiredBrakeDuration)
            {
                state.brakeSuccess = true;
                TryMeasureFirstBrakeDistance(logger, state);
            }
        }
        else
        {
            state.brakeTimer = 0f;
        }
    }

    private bool CanMeasureFirstBrakeDistance()
    {
        if (!distanceMeasureSettings.measureDistance)
            return false;

        if (distanceMeasureSettings.targetObject == null)
            return false;

        return distanceMeasureSettings.distanceMeasureMode == DistanceMeasureMode.IlkFren ||
               distanceMeasureSettings.distanceMeasureMode == DistanceMeasureMode.IlkFrenVeIlkDurma;
    }

    private bool CanMeasureStoppedDistance()
    {
        if (!distanceMeasureSettings.measureDistance)
            return false;

        if (distanceMeasureSettings.targetObject == null)
            return false;

        return distanceMeasureSettings.distanceMeasureMode == DistanceMeasureMode.IlkDurma ||
               distanceMeasureSettings.distanceMeasureMode == DistanceMeasureMode.IlkFrenVeIlkDurma;
    }

    private void TryMeasureFirstBrakeDistance(DrivingLogManager logger, LocalState state)
    {
        if (!CanMeasureFirstBrakeDistance())
            return;

        if (state.firstBrakeDistanceMeasured)
            return;

        state.firstBrakeDistanceMeasured = true;

        state.firstBrakeDistance = Vector3.Distance(
            logger.transform.position,
            distanceMeasureSettings.targetObject.position
        );

        state.firstBrakeSpeed = logger.speedKmh;
    }

    private void TryMeasureStoppedDistance(DrivingLogManager logger, LocalState state)
    {
        if (!CanMeasureStoppedDistance())
            return;

        if (state.stoppedDistanceMeasured)
            return;

        state.stoppedDistanceMeasured = true;

        state.stoppedDistance = Vector3.Distance(
            logger.transform.position,
            distanceMeasureSettings.targetObject.position
        );

        state.stoppedSpeed = logger.speedKmh;
    }

    private string AppendDistanceExplanation(string explanation, LocalState state)
    {
        if (state.firstBrakeDistanceMeasured)
        {
            explanation += " " +
                           distanceMeasureSettings.firstBrakeDistanceLabel +
                           ": " +
                           state.firstBrakeDistance.ToString("F2") +
                           " m.";

            if (distanceMeasureSettings.includeBrakeSpeed)
            {
                explanation += " Fren anındaki hız: " +
                               state.firstBrakeSpeed.ToString("F1") +
                               " km/h.";
            }
        }

        if (state.stoppedDistanceMeasured)
        {
            explanation += " " +
                           distanceMeasureSettings.stoppedDistanceLabel +
                           ": " +
                           state.stoppedDistance.ToString("F2") +
                           " m.";

            if (distanceMeasureSettings.includeStopSpeed)
            {
                explanation += " Durma anındaki hız: " +
                               state.stoppedSpeed.ToString("F1") +
                               " km/h.";
            }
        }

        return explanation;
    }

    private void UpdateGasCheck(DrivingLogManager logger, LocalState state)
    {
        if (!gasSettings.checkGas)
            return;

        bool gasPressed = logger.gasValue >= gasSettings.requiredGasValue;

        if (gasSettings.reverseGasCondition)
        {
            UpdateViolationCounter(state.gasViolation, gasPressed, gasSettings.requiredGasDuration);

            if (state.gasViolation.count > 0)
            {
                state.gasError = true;
                AddReason(state, gasSettings.gasErrorMessage);
            }

            state.gasTimer = state.gasViolation.currentDuration;
            return;
        }

        if (gasPressed)
        {
            state.gasTimer += Time.deltaTime;

            if (state.gasTimer >= gasSettings.requiredGasDuration)
                state.gasSuccess = true;
        }
        else
        {
            state.gasTimer = 0f;
        }
    }

    private void UpdateGasBrakeTogetherCheck(DrivingLogManager logger, LocalState state)
    {
        if (!gasBrakeTogetherSettings.checkGasBrakeTogether)
            return;

        bool gasAndBrake =
            logger.gasValue >= gasBrakeTogetherSettings.gasTogetherThreshold &&
            logger.brakeValue >= gasBrakeTogetherSettings.brakeTogetherThreshold;

        UpdateViolationCounter(state.gasBrakeTogetherViolation, gasAndBrake, gasBrakeTogetherSettings.gasBrakeTogetherDuration);

        if (state.gasBrakeTogetherViolation.count > 0)
        {
            state.gasBrakeTogetherError = true;
            AddReason(state, gasBrakeTogetherSettings.gasBrakeTogetherErrorMessage);
        }

        state.gasBrakeTogetherTimer = state.gasBrakeTogetherViolation.currentDuration;
    }

    private void UpdateSteeringCheck(DrivingLogManager logger, LocalState state)
    {
        if (!steeringSettings.checkSteeringDirection)
            return;

        bool steeringMatched = false;

        if (steeringSettings.expectedSteeringDirection == SteeringDirection.Sol)
            steeringMatched = logger.steeringAngle <= -steeringSettings.steeringThreshold;

        if (steeringSettings.expectedSteeringDirection == SteeringDirection.Sag)
            steeringMatched = logger.steeringAngle >= steeringSettings.steeringThreshold;

        if (steeringSettings.reverseSteeringCondition)
        {
            UpdateViolationCounter(state.steeringViolation, steeringMatched, steeringSettings.requiredSteeringDuration);

            if (state.steeringViolation.count > 0)
            {
                state.steeringError = true;
                AddReason(state, steeringSettings.steeringErrorMessage);
            }

            state.steeringTimer = state.steeringViolation.currentDuration;
            return;
        }

        if (steeringMatched)
        {
            state.steeringTimer += Time.deltaTime;

            if (state.steeringTimer >= steeringSettings.requiredSteeringDuration)
                state.steeringSuccess = true;
        }
        else
        {
            state.steeringTimer = 0f;
        }
    }

    private void UpdateSignalCheck(DrivingLogManager logger, LocalState state)
    {
        if (signalSettings.requiredSignal == SignalRequirement.Farketmez)
            return;

        bool signalCondition = false;

        if (signalSettings.requiredSignal == SignalRequirement.Yok)
            signalCondition = logger.IsAnySignalOn();

        if (signalSettings.requiredSignal == SignalRequirement.Sol)
            signalCondition = logger.IsLeftSignalOn();

        if (signalSettings.requiredSignal == SignalRequirement.Sag)
            signalCondition = logger.IsRightSignalOn();

        if (signalSettings.requiredSignal == SignalRequirement.Yok)
        {
            UpdateViolationCounter(state.signalViolation, signalCondition, signalSettings.requiredSignalDuration);

            if (state.signalViolation.count > 0)
            {
                state.signalError = true;
                AddReason(state, signalSettings.signalErrorMessage);
            }

            state.signalTimer = state.signalViolation.currentDuration;
            return;
        }

        if (signalCondition)
        {
            state.signalTimer += Time.deltaTime;

            if (state.signalTimer >= signalSettings.requiredSignalDuration)
                state.signalSuccess = true;
        }
        else
        {
            state.signalTimer = 0f;
        }
    }

    private void UpdateViolationMeasurements(DrivingLogManager logger, LocalState state)
    {
        if (offRoadSettings.measureOffRoad)
        {
            UpdateViolationCounter(state.offRoadViolation, true, offRoadSettings.offRoadViolationDuration);

            if (state.offRoadViolation.count > 0)
                state.offRoadMeasured = true;

            state.offRoadTimer = state.offRoadViolation.currentDuration;
        }

        if (wrongLaneSettings.measureWrongLane)
        {
            UpdateViolationCounter(state.wrongLaneViolation, true, wrongLaneSettings.wrongLaneViolationDuration);

            if (state.wrongLaneViolation.count > 0)
                state.wrongLaneMeasured = true;

            state.wrongLaneTimer = state.wrongLaneViolation.currentDuration;
        }

        if (lineViolationSettings.measureLineViolation)
        {
            UpdateViolationCounter(state.lineViolation, true, lineViolationSettings.lineViolationDuration);

            if (state.lineViolation.count > 0)
                state.lineViolationMeasured = true;

            state.lineViolationTimer = state.lineViolation.currentDuration;
        }
    }

    private void FinalizeMeasuredSensorViolations(DrivingLogManager logger, LocalState state)
    {
        FinalizeViolationCounterOccurrence(state.offRoadViolation, offRoadSettings.offRoadViolationDuration);
        FinalizeViolationCounterOccurrence(state.wrongLaneViolation, wrongLaneSettings.wrongLaneViolationDuration);
        FinalizeViolationCounterOccurrence(state.lineViolation, lineViolationSettings.lineViolationDuration);

        if (offRoadSettings.measureOffRoad && state.offRoadViolation.count > 0 && !state.offRoadReportedToActiveEvent)
        {
            state.offRoadMeasured = true;
            string message = BuildViolationMessageWithStats(offRoadSettings.offRoadErrorMessage, "Yol dışı", "Yol dışında kalma süresi", state.offRoadViolation, true);
            AddReason(state, message);

            bool accepted = NotifyActiveEvents(logger, ViolationType.YolDisi, message);

            if (!accepted && generalSettings.logSelfWhenNoActiveEvent)
                state.selfLogBecauseNoActiveEvent = true;

            state.offRoadReportedToActiveEvent = true;
        }

        if (wrongLaneSettings.measureWrongLane && state.wrongLaneViolation.count > 0 && !state.wrongLaneReportedToActiveEvent)
        {
            state.wrongLaneMeasured = true;
            string message = BuildViolationMessageWithStats(wrongLaneSettings.wrongLaneErrorMessage, "Karşı şerit", "Karşı şeritte kalma süresi", state.wrongLaneViolation, true);
            AddReason(state, message);

            bool accepted = NotifyActiveEvents(logger, ViolationType.KarsiSerit, message);

            if (!accepted && generalSettings.logSelfWhenNoActiveEvent)
                state.selfLogBecauseNoActiveEvent = true;

            state.wrongLaneReportedToActiveEvent = true;
        }

        if (lineViolationSettings.measureLineViolation && state.lineViolation.count > 0 && !state.lineViolationReportedToActiveEvent)
        {
            state.lineViolationMeasured = true;
            string message = BuildViolationMessageWithStats(lineViolationSettings.lineViolationErrorMessage, "Çizgi ihlali", "Çizgi ihlali süresi", state.lineViolation, true);
            AddReason(state, message);

            bool accepted = NotifyActiveEvents(logger, ViolationType.CizgiIhlali, message);

            if (!accepted && generalSettings.logSelfWhenNoActiveEvent)
                state.selfLogBecauseNoActiveEvent = true;

            state.lineViolationReportedToActiveEvent = true;
        }
    }

    private void RegisterCorridorEnter(DrivingLogManager logger)
    {
        if (!corridorSettings.measureCorridorTransition)
            return;

        string key = GetCorridorKey(logger);

        if (!corridorStates.ContainsKey(key))
        {
            CorridorState newState = new CorridorState();
            newState.startCorridorNo = corridorSettings.corridorNo;
            newState.otherCorridorTimer = 0f;
            newState.violationSent = false;
            newState.activeContactCount = 0;

            corridorStates.Add(key, newState);
        }

        corridorStates[key].activeContactCount++;
    }

    private void RegisterCorridorExit(DrivingLogManager logger)
    {
        if (!corridorSettings.measureCorridorTransition)
            return;

        string key = GetCorridorKey(logger);

        if (!corridorStates.ContainsKey(key))
            return;

        corridorStates[key].activeContactCount--;

        if (corridorStates[key].activeContactCount < 0)
            corridorStates[key].activeContactCount = 0;

        if (corridorSettings.resetCorridorStartOnExit)
            StartCoroutine(ClearCorridorStateAfterDelay(key, corridorSettings.corridorResetDelay));
    }

    private IEnumerator ClearCorridorStateAfterDelay(string key, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!corridorStates.ContainsKey(key))
            yield break;

        if (corridorStates[key].activeContactCount <= 0)
            corridorStates.Remove(key);
    }

    private void UpdateCorridorMeasurement(DrivingLogManager logger, LocalState state)
    {
        if (!corridorSettings.measureCorridorTransition)
            return;

        string key = GetCorridorKey(logger);

        if (!corridorStates.ContainsKey(key))
            return;

        CorridorState corridorState = corridorStates[key];

        if (corridorState.startCorridorNo != corridorSettings.corridorNo)
        {
            corridorState.otherCorridorTimer += Time.deltaTime;

            if (corridorState.otherCorridorTimer >= corridorSettings.corridorTransitionDuration && !corridorState.violationSent)
            {
                corridorState.violationSent = true;
                state.corridorViolationCount++;
                AddReason(state, corridorSettings.corridorTransitionErrorMessage);

                bool accepted = NotifyActiveEvents(logger, ViolationType.KoridorGecisi, corridorSettings.corridorTransitionErrorMessage);

                if (!accepted && generalSettings.logSelfWhenNoActiveEvent)
                    state.selfLogBecauseNoActiveEvent = true;
            }
        }
    }

    private void UpdateViolationCounter(ViolationCounter counter, bool conditionActive, float minimumDuration)
    {
        if (counter == null)
            return;

        if (conditionActive)
        {
            counter.currentDuration += Time.deltaTime;

            if (!counter.currentCounted && counter.currentDuration >= minimumDuration)
            {
                counter.count++;
                counter.currentCounted = true;
            }
        }
        else
        {
            FinalizeViolationCounterOccurrence(counter, minimumDuration);
        }
    }

    private void FinalizeViolationCounterOccurrence(ViolationCounter counter, float minimumDuration)
    {
        if (counter == null)
            return;

        if (counter.currentDuration > 0f)
        {
            if (counter.currentDuration >= minimumDuration)
                counter.totalDuration += counter.currentDuration;

            counter.currentDuration = 0f;
            counter.currentCounted = false;
        }
    }

    private void FinalizeAllViolationCounters(LocalState state)
    {
        FinalizeViolationCounterOccurrence(state.speedViolation, speedSettings.speedViolationDuration);
        FinalizeViolationCounterOccurrence(state.brakeViolation, brakeSettings.requiredBrakeDuration);
        FinalizeViolationCounterOccurrence(state.gasViolation, gasSettings.requiredGasDuration);
        FinalizeViolationCounterOccurrence(state.gasBrakeTogetherViolation, gasBrakeTogetherSettings.gasBrakeTogetherDuration);
        FinalizeViolationCounterOccurrence(state.steeringViolation, steeringSettings.requiredSteeringDuration);
        FinalizeViolationCounterOccurrence(state.signalViolation, signalSettings.requiredSignalDuration);
        FinalizeViolationCounterOccurrence(state.offRoadViolation, offRoadSettings.offRoadViolationDuration);
        FinalizeViolationCounterOccurrence(state.wrongLaneViolation, wrongLaneSettings.wrongLaneViolationDuration);
        FinalizeViolationCounterOccurrence(state.lineViolation, lineViolationSettings.lineViolationDuration);
    }

    private string BuildViolationMessageWithStats(string baseMessage, string countLabel, string durationLabel, ViolationCounter counter, bool includeDuration)
    {
        string result = baseMessage;

        if (!result.EndsWith("."))
            result += ".";

        if (violationSummarySettings.appendViolationSummary && violationSummarySettings.appendViolationCount)
            result += " " + countLabel + " ihlal sayısı: " + counter.count + ".";

        if (violationSummarySettings.appendViolationSummary && violationSummarySettings.appendViolationDuration && includeDuration)
            result += " " + durationLabel + ": " + counter.totalDuration.ToString("F2") + " sn.";

        return result;
    }

    private string AppendViolationSummaryExplanation(string explanation, LocalState state)
    {
        if (!violationSummarySettings.appendViolationSummary)
            return explanation;

        explanation = AppendCounterSummary(explanation, "Hız", "Toplam hız ihlal süresi", state.speedViolation, state.speedViolation.count > 0, true);

        if (brakeSettings.checkBrake && brakeSettings.reverseBrakeCondition)
            explanation = AppendCounterSummary(explanation, "Fren", "Toplam yersiz fren süresi", state.brakeViolation, state.brakeViolation.count > 0, true);

        if (gasSettings.checkGas && gasSettings.reverseGasCondition)
            explanation = AppendCounterSummary(explanation, "Gaz", "Toplam yersiz gaz süresi", state.gasViolation, state.gasViolation.count > 0, true);

        explanation = AppendCounterSummary(explanation, "Gaz+fren", "Toplam gaz+fren süresi", state.gasBrakeTogetherViolation, state.gasBrakeTogetherViolation.count > 0, true);

        if (steeringSettings.checkSteeringDirection && steeringSettings.reverseSteeringCondition)
            explanation = AppendCounterSummary(explanation, "Direksiyon", "Toplam yersiz direksiyon süresi", state.steeringViolation, state.steeringViolation.count > 0, true);

        if (signalSettings.requiredSignal == SignalRequirement.Yok)
            explanation = AppendCounterSummary(explanation, "Sinyal", "Toplam yersiz sinyal süresi", state.signalViolation, state.signalViolation.count > 0, true);

        explanation = AppendCounterSummary(explanation, "Yol dışı", "Yol dışında kalma süresi", state.offRoadViolation, state.offRoadViolation.count > 0 && !offRoadSettings.measureOffRoad, true);
        explanation = AppendCounterSummary(explanation, "Karşı şerit", "Karşı şeritte kalma süresi", state.wrongLaneViolation, state.wrongLaneViolation.count > 0 && !wrongLaneSettings.measureWrongLane, true);
        explanation = AppendCounterSummary(explanation, "Çizgi ihlali", "Çizgi ihlali süresi", state.lineViolation, state.lineViolation.count > 0 && !lineViolationSettings.measureLineViolation, true);

        if (state.collisionCount > 0)
            explanation += " Çarpışma sayısı: " + state.collisionCount + ".";

        if (state.corridorViolationCount > 0)
            explanation += " Koridor ihlal sayısı: " + state.corridorViolationCount + ".";

        if (violationSummarySettings.appendStopDuration && state.longestStopDuration > 0f)
            explanation += " Durma süresi: " + state.longestStopDuration.ToString("F2") + " sn.";

        return explanation;
    }

    private string AppendCounterSummary(string explanation, string countLabel, string durationLabel, ViolationCounter counter, bool shouldWrite, bool includeDuration)
    {
        if (!shouldWrite || counter == null || counter.count <= 0)
            return explanation;

        if (violationSummarySettings.appendViolationCount)
            explanation += " " + countLabel + " ihlal sayısı: " + counter.count + ".";

        if (includeDuration && violationSummarySettings.appendViolationDuration)
            explanation += " " + durationLabel + ": " + counter.totalDuration.ToString("F2") + " sn.";

        return explanation;
    }

    private void EvaluateAndLog(DrivingLogManager logger, LocalState state)
    {
        if (generalSettings.logOnlyOnce && hasLogged)
            return;

        FinalizeRequiredChecks(state);

        bool hasError =
            state.errorReasons.Count > 0 ||
            state.speedError ||
            state.brakeError ||
            state.gasError ||
            state.gasBrakeTogetherError ||
            state.steeringError ||
            state.signalError ||
            state.offRoadMeasured ||
            state.wrongLaneMeasured ||
            state.lineViolationMeasured ||
            state.externalOffRoad ||
            state.externalWrongLane ||
            state.externalLineViolation ||
            state.externalCorridorTransition ||
            state.externalCollision;

        if (!hasError && generalSettings.logOnlyIfError)
            return;

        string explanation = hasError ? BuildErrorExplanation(state) : generalSettings.successMessage;
        explanation = AppendDistanceExplanation(explanation, state);
        explanation = AppendViolationSummaryExplanation(explanation, state);

        logger.WriteEventLog(GetActionText(), hasError, explanation);

        hasLogged = true;
    }

    private void FinalizeRequiredChecks(LocalState state)
    {
        if (stopSettings.requireStop)
        {
            bool shouldRequireStop = ShouldRequireStopAccordingToTrafficLight();

            if (shouldRequireStop && !state.stopSuccess)
                AddReason(state, stopSettings.stopErrorMessage);

            if (ShouldCountGreenStopAsError(state))
                AddReason(state, trafficLightStopSettings.greenStopErrorMessage);
        }

        if (stopSettings.requireStop &&trafficLightStopSettings.useTrafficLightForStop &&trafficLightStopSettings.errorIfPassedBeforeGreenAfterStop &&state.stopSuccess &&state.hasTrafficLightColor &&state.lastTrafficLightColor != TrafficLight.CurrentLightColor.Green)
        {
            AddReason(state, trafficLightStopSettings.passedBeforeGreenErrorMessage);
        }

        if (brakeSettings.checkBrake && !brakeSettings.reverseBrakeCondition)
        {
            bool shouldRequireBrake = true;

            if (trafficLightStopSettings.useTrafficLightForStop)
                shouldRequireBrake = ShouldRequireStopAccordingToTrafficLight();

            if (shouldRequireBrake && !state.brakeSuccess)
                AddReason(state, brakeSettings.brakeErrorMessage);
        }

        if (gasSettings.checkGas && !gasSettings.reverseGasCondition && !state.gasSuccess)
            AddReason(state, gasSettings.gasErrorMessage);

        if (steeringSettings.checkSteeringDirection && !steeringSettings.reverseSteeringCondition && !state.steeringSuccess)
            AddReason(state, steeringSettings.steeringErrorMessage);

        if (signalSettings.requiredSignal == SignalRequirement.Sol || signalSettings.requiredSignal == SignalRequirement.Sag)
        {
            if (!state.signalSuccess)
                AddReason(state, signalSettings.signalErrorMessage);
        }
    }


    private bool ShouldCountGreenStopAsError(LocalState state)
    {
        if (!trafficLightStopSettings.useTrafficLightForStop)
            return false;

        if (!trafficLightStopSettings.errorIfStoppedOnGreen)
            return false;

        if (trafficLightStopSettings.targetTrafficLight == null)
            return false;

        TrafficLight.CurrentLightColor currentColor =
            trafficLightStopSettings.targetTrafficLight.GetCurrentColor();

        if (currentColor != TrafficLight.CurrentLightColor.Green)
            return false;

        return state.stopSuccess;
    }

    private string BuildErrorExplanation(LocalState state)
    {
        if (!generalSettings.appendErrorReasons || state.errorReasons.Count == 0)
            return generalSettings.generalErrorMessage;

        string result = generalSettings.generalErrorMessage + " Sebepler: ";

        for (int i = 0; i < state.errorReasons.Count; i++)
        {
            result += state.errorReasons[i];

            if (!result.EndsWith("."))
                result += ".";

            if (i < state.errorReasons.Count - 1)
                result += " ";
        }

        return result;
    }

    private bool ReceiveExternalViolation(DrivingLogManager logger, ViolationType type, string customMessage = "")
    {
        LocalState state = GetOrCreateState(logger);
        bool accepted = false;

        if (type == ViolationType.YolDisi && offRoadSettings.listenOffRoadViolation)
        {
            state.externalOffRoad = true;
            AddReason(state, IsBlank(customMessage) ? offRoadSettings.offRoadErrorMessage : customMessage);
            accepted = true;
        }

        if (type == ViolationType.KarsiSerit && wrongLaneSettings.listenWrongLaneViolation)
        {
            state.externalWrongLane = true;
            AddReason(state, IsBlank(customMessage) ? wrongLaneSettings.wrongLaneErrorMessage : customMessage);
            accepted = true;
        }

        if (type == ViolationType.CizgiIhlali && lineViolationSettings.listenLineViolation)
        {
            state.externalLineViolation = true;
            AddReason(state, IsBlank(customMessage) ? lineViolationSettings.lineViolationErrorMessage : customMessage);
            accepted = true;
        }

        if (type == ViolationType.KoridorGecisi && corridorSettings.listenCorridorTransition)
        {
            state.externalCorridorTransition = true;
            state.corridorViolationCount++;
            AddReason(state, IsBlank(customMessage) ? corridorSettings.corridorTransitionErrorMessage : customMessage);
            accepted = true;
        }

        if (type == ViolationType.Carpisma && collisionSettings.listenCollisionViolation)
        {
            state.externalCollision = true;
            state.collisionCount++;

            if (IsBlank(customMessage))
                AddReason(state, collisionSettings.collisionErrorMessage);
            else
                AddReason(state, customMessage);

            accepted = true;
        }

        return accepted;
    }

    public static bool ReportCollisionViolation(DrivingLogManager logger, string customMessage)
    {
        if (logger == null)
            return false;

        if (!activeEventTriggers.ContainsKey(logger))
            return false;

        bool acceptedByAnyTrigger = false;
        List<ScenarioEventTrigger> activeList = activeEventTriggers[logger];

        for (int i = 0; i < activeList.Count; i++)
        {
            if (activeList[i] != null)
            {
                bool accepted = activeList[i].ReceiveExternalViolation(logger, ViolationType.Carpisma, customMessage);

                if (accepted)
                    acceptedByAnyTrigger = true;
            }
        }

        return acceptedByAnyTrigger;
    }

    public void RegisterCollisionViolation(DrivingLogManager logger)
    {
        NotifyActiveEvents(logger, ViolationType.Carpisma);
    }

    private bool NotifyActiveEvents(DrivingLogManager logger, ViolationType type, string customMessage = "")
    {
        if (logger == null)
            return false;

        if (!activeEventTriggers.ContainsKey(logger))
            return false;

        bool acceptedByAnyTrigger = false;
        List<ScenarioEventTrigger> activeList = activeEventTriggers[logger];

        for (int i = 0; i < activeList.Count; i++)
        {
            if (activeList[i] != null)
            {
                bool accepted = activeList[i].ReceiveExternalViolation(logger, type, customMessage);

                if (accepted)
                    acceptedByAnyTrigger = true;
            }
        }

        return acceptedByAnyTrigger;
    }

    private bool ShouldRegisterAsActiveEvent()
    {
        if (!generalSettings.writeResultLog)
            return false;

        return offRoadSettings.listenOffRoadViolation ||
               wrongLaneSettings.listenWrongLaneViolation ||
               lineViolationSettings.listenLineViolation ||
               corridorSettings.listenCorridorTransition ||
               collisionSettings.listenCollisionViolation ||
               speedSettings.checkSpeed ||
               stopSettings.requireStop ||
               brakeSettings.checkBrake ||
               gasSettings.checkGas ||
               gasBrakeTogetherSettings.checkGasBrakeTogether ||
               steeringSettings.checkSteeringDirection ||
               signalSettings.requiredSignal != SignalRequirement.Farketmez;
    }

    private void RegisterActiveEvent(DrivingLogManager logger, ScenarioEventTrigger trigger)
    {
        if (!activeEventTriggers.ContainsKey(logger))
            activeEventTriggers.Add(logger, new List<ScenarioEventTrigger>());

        if (!activeEventTriggers[logger].Contains(trigger))
            activeEventTriggers[logger].Add(trigger);
    }

    private void UnregisterActiveEvent(DrivingLogManager logger, ScenarioEventTrigger trigger)
    {
        if (!activeEventTriggers.ContainsKey(logger))
            return;

        if (activeEventTriggers[logger].Contains(trigger))
            activeEventTriggers[logger].Remove(trigger);
    }

    private LocalState GetOrCreateState(DrivingLogManager logger)
    {
        if (!localStates.ContainsKey(logger))
            localStates.Add(logger, new LocalState());

        return localStates[logger];
    }

    private DrivingLogManager GetLogger(Collider other)
    {
        DrivingLogManager logger = other.GetComponentInParent<DrivingLogManager>();

        if (logger != null)
            return logger;

        return other.GetComponent<DrivingLogManager>();
    }

    private void AddReason(LocalState state, string reason)
    {
        if (IsBlank(reason))
            return;

        if (!state.errorReasons.Contains(reason))
            state.errorReasons.Add(reason);
    }

    private bool IsBlank(string value)
    {
        return string.IsNullOrEmpty(value) || value.Trim().Length == 0;
    }

    private string GetCorridorKey(DrivingLogManager logger)
    {
        string carName = logger.transform.root.name;
        return carName + "_" + corridorSettings.corridorGroupId;
    }

    private string GetActionText()
    {
        switch (generalSettings.actionType)
        {
            case EventActionType.SabitSurus:
                return "Sabit Sürüş";
            case EventActionType.Hizlanma:
                return "Hızlanma";
            case EventActionType.YavaslamaFrenleme:
                return "Yavaşlama / Frenleme";
            case EventActionType.SolSeritDegistirme:
                return "Sol Şerit Değiştirme";
            case EventActionType.SagSeritDegistirme:
                return "Sağ Şerit Değiştirme";
            case EventActionType.SolaDonus:
                return "Sola Dönüş";
            case EventActionType.SagaDonus:
                return "Sağa Dönüş";
            case EventActionType.Sollama:
                return "Sollama";
            case EventActionType.Durma:
                return "Durma";
            case EventActionType.YayayaYolVerme:
                return "Yayaya Yol Verme";
            case EventActionType.HizSiniriKontrolu:
                return "Hız Sınırı Kontrolü";
            case EventActionType.AniFrenTepkisi:
                return "Ani Fren Tepkisi";
            case EventActionType.NavigasyonTakibi:
                return "Navigasyon Takibi";
            case EventActionType.AmbulansaYolVerme:
                return "Ambulansa Yol Verme";
            case EventActionType.KarsiSeritIhlali:
                return "Karşı Şerit İhlali";
            case EventActionType.YolDisi:
                return "Yol Dışı";
            case EventActionType.CizgiIhlali:
                return "Çizgi İhlali";
            case EventActionType.KoridorGecisIhlali:
                return "Koridor Geçiş İhlali";
            case EventActionType.Carpisma:
                return "Çarpışma";
            default:
                return "Bilinmeyen Olay";
        }
    }
}