using System.IO;
using UnityEngine;

public class GenelSenaryoTrigger : MonoBehaviour
{
    public enum GerekliSinyal { Yok, Sol, Sag }
    public enum SeritYon { Yok, Sol, Sag }

    [Header("ARAÇ REFERANSLARI")]
    public GameObject aracObjesi;
    public Rigidbody aracRigidbody;
    public G29SteeringOverride g29GirisSistemi;

    [Header("GENEL TRIGGER AYARLARI")]
    public bool sadeceSecilenAraciKontrolEt = true;
    public bool triggerGirisiniKaydet = true;
    public bool triggerCikisiniKaydet = true;

    [Header("LOG SÝSTEMÝ")]
    public bool loglamaAktif = true;
    public bool dosyayaYaz = false;
    public bool consolaYaz = true;
    public bool oyunBasindaLoguTemizle = false;
    public string logDosyaAdi = "SenaryoLog.txt";

    [Header("HIZ KONTROLÜ")]
    public bool hizKontroluAktif = false;
    public float minimumHizKmh = 0f;
    public float maksimumHizKmh = 60f;
    public bool minimumHizAltiniUyar = true;
    public bool maksimumHizUstunuUyar = true;
    public bool surekliKontrolEt = true;
    public float logAraligiSaniye = 1f;
    public bool sadeceBirKezLogla = false;
    public bool normalHizaDonusuKaydet = true;

    [Header("SÝNYAL KONTROLÜ")]
    public bool sinyalKontroluAktif = false;
    public GerekliSinyal gerekliSinyal = GerekliSinyal.Yok;

    [Header("DURMA KONTROLÜ")]
    public bool durmaKontroluAktif = false;
    public float durmusSayilacakHizKmh = 1f;
    public float gerekliDurusSuresiSaniye = 2f;

    [Header("GAZ / FREN KONTROLÜ")]
    public bool gazFrenKontroluAktif = false;
    public bool freniKontrolEt = true;
    public bool gaziKontrolEt = true;
    [Range(0f, 1f)] public float frenAlgilamaEsigi = 0.2f;
    [Range(0f, 1f)] public float gazAlgilamaEsigi = 0.2f;
    public bool frenKuvvetiniOlc = true;
    public bool gazKuvvetiniOlc = true;
    public bool frenSuresiniOlc = true;
    public bool gazSuresiniOlc = true;
    public bool frenBasiminiKaydet = true;
    public bool gazBasiminiKaydet = true;
    public bool gazFrenAyniAndaKaydet = true;

    [Header("ÞERÝT DEÐÝÞTÝRME KONTROLÜ")]
    public bool seritDegistirmeKontroluAktif = false;
    public SeritYon seritDegistirmeYonu = SeritYon.Yok;
    [Range(0f, 1f)] public float direksiyonEsigi = 0.35f;
    public float gerekliTutmaSuresiSaniye = 1f;

    [Header("ÇARPIÞMA KONTROLÜ")]
    public bool carpismaKontroluAktif = false;
    public bool carpismaHiziniKaydet = true;
    public bool carpismayiSadeceBirKezKaydet = false;
    public float carpismaBeklemeSuresi = 1f;

    [Header("KARÞI ÞERÝT KONTROLÜ")]
    public bool karsiSeritKontroluAktif = false;

    [Header("YOL DIÞI KONTROLÜ")]
    public bool yolDisiKontroluAktif = false;

    private static bool sessionBasladi = false;
    private static float sessionBaslangicZamani;
    private static string logDosyaYolu;

    private bool aracTriggerIcinde = false;
    private int triggerIcindekiColliderSayisi = 0;

    private float sonBilinenHizKmh = 0f;

    private float sonHizLogZamani = -999f;
    private bool hizAltLoglandi = false;
    private bool hizUstLoglandi = false;
    private bool hizAnormaldi = false;

    private bool solSinyalOncekiDurum = false;
    private bool sagSinyalOncekiDurum = false;
    private bool solSinyalVerildi = false;
    private bool sagSinyalVerildi = false;
    private float solSinyalBaslangicZamani = 0f;
    private float sagSinyalBaslangicZamani = 0f;

    private float durusSayaci = 0f;
    private bool aracDuruyor = false;
    private float gozlenenMinimumHiz = float.MaxValue;
    private bool buTriggerdaDurdu = false;

    private bool frenBasiliydi = false;
    private bool gazBasiliydi = false;
    private float frenBaslangicZamani = 0f;
    private float gazBaslangicZamani = 0f;
    private float maksimumFrenKuvveti = 0f;
    private float maksimumGazKuvveti = 0f;
    private float frenKuvvetToplami = 0f;
    private float gazKuvvetToplami = 0f;
    private int frenOrnekSayisi = 0;
    private int gazOrnekSayisi = 0;

    private bool gazFrenAyniAndaAktif = false;
    private float gazFrenAyniAndaBaslangicZamani = 0f;

    private float seritTutmaSayaci = 0f;
    private bool seritDegisimiAlgilandi = false;
    private bool buTriggerdaSeritAlgilandi = false;

    private bool carpismaLoglandi = false;
    private float sonCarpismaZamani = -999f;

    private bool karsiSeritte = false;
    private float karsiSeritBaslangicZamani = 0f;

    private bool yolDisinda = false;
    private float yolDisiBaslangicZamani = 0f;

    private void Awake()
    {
        if (!sessionBasladi)
        {
            sessionBasladi = true;
            sessionBaslangicZamani = Time.time;
            logDosyaYolu = Path.Combine(Application.persistentDataPath, logDosyaAdi);

            if (oyunBasindaLoguTemizle && File.Exists(logDosyaYolu))
                File.Delete(logDosyaYolu);
        }

        if (aracObjesi != null && aracRigidbody == null)
            aracRigidbody = aracObjesi.GetComponentInChildren<Rigidbody>();

        if (aracObjesi != null && g29GirisSistemi == null)
            g29GirisSistemi = aracObjesi.GetComponentInChildren<G29SteeringOverride>();
    }

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void Update()
    {
        sonBilinenHizKmh = AnlikHizKmh();

        if (!aracTriggerIcinde)
            return;

        if (hizKontroluAktif) HizKontrolu();
        if (sinyalKontroluAktif) SinyalKontrolu();
        if (durmaKontroluAktif) DurmaKontrolu();
        if (gazFrenKontroluAktif) GazFrenKontrolu();
        if (seritDegistirmeKontroluAktif) DireksiyonlaSeritKontrolu();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!DogruAracMi(other))
            return;

        triggerIcindekiColliderSayisi++;

        if (aracTriggerIcinde)
            return;

        aracTriggerIcinde = true;
        DegerleriSifirla();

        if (triggerGirisiniKaydet)
            LogYaz($"{name} - TRIGGER GÝRÝÞÝ");

        if (karsiSeritKontroluAktif)
        {
            karsiSeritte = true;
            karsiSeritBaslangicZamani = Time.time;
            LogYaz($"{name} - KARÞI ÞERÝDE GÝRÝLDÝ");
        }

        if (yolDisiKontroluAktif)
        {
            yolDisinda = true;
            yolDisiBaslangicZamani = Time.time;
            LogYaz($"{name} - YOL DIÞINA ÇIKILDI");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Ölçümler Update içinde yapýlýyor.
    }

    private void OnTriggerExit(Collider other)
    {
        if (!DogruAracMi(other))
            return;

        triggerIcindekiColliderSayisi--;

        if (triggerIcindekiColliderSayisi > 0)
            return;

        triggerIcindekiColliderSayisi = 0;
        aracTriggerIcinde = false;

        if (sinyalKontroluAktif) SinyalSonucunuYaz();
        if (durmaKontroluAktif) DurmaSonucunuYaz();
        if (gazFrenKontroluAktif) GazFrenSonucunuBitir();
        if (seritDegistirmeKontroluAktif) SeritSonucunuYaz();

        if (karsiSeritKontroluAktif && karsiSeritte)
        {
            float sure = Time.time - karsiSeritBaslangicZamani;
            LogYaz($"{name} - KARÞI ÞERÝTTEN ÇIKILDI | Süre: {sure:F2} sn");
            karsiSeritte = false;
        }

        if (yolDisiKontroluAktif && yolDisinda)
        {
            float sure = Time.time - yolDisiBaslangicZamani;
            LogYaz($"{name} - YOL DIÞINDAN ÇIKILDI | Süre: {sure:F2} sn");
            yolDisinda = false;
        }

        if (triggerCikisiniKaydet)
            LogYaz($"{name} - TRIGGER ÇIKIÞI");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!carpismaKontroluAktif)
            return;

        if (carpismayiSadeceBirKezKaydet && carpismaLoglandi)
            return;

        if (Time.time - sonCarpismaZamani < carpismaBeklemeSuresi)
            return;

        sonCarpismaZamani = Time.time;
        carpismaLoglandi = true;

        float hiz = collision.relativeVelocity.magnitude * 3.6f;

        if (hiz <= 0.1f)
            hiz = sonBilinenHizKmh;

        if (carpismaHiziniKaydet)
            LogYaz($"{name} - ÇARPIÞMA | Hýz: {hiz:F1} km/h");
        else
            LogYaz($"{name} - ÇARPIÞMA");
    }

    private bool DogruAracMi(Collider other)
    {
        if (!sadeceSecilenAraciKontrolEt)
            return true;

        if (aracRigidbody == null)
            return false;

        return other.attachedRigidbody == aracRigidbody;
    }

    private void DegerleriSifirla()
    {
        sonHizLogZamani = -999f;
        hizAltLoglandi = false;
        hizUstLoglandi = false;
        hizAnormaldi = false;

        solSinyalOncekiDurum = SolSinyalAcik();
        sagSinyalOncekiDurum = SagSinyalAcik();
        solSinyalVerildi = false;
        sagSinyalVerildi = false;
        solSinyalBaslangicZamani = 0f;
        sagSinyalBaslangicZamani = 0f;

        durusSayaci = 0f;
        aracDuruyor = false;
        gozlenenMinimumHiz = float.MaxValue;
        buTriggerdaDurdu = false;

        frenBasiliydi = false;
        gazBasiliydi = false;
        frenBaslangicZamani = 0f;
        gazBaslangicZamani = 0f;
        maksimumFrenKuvveti = 0f;
        maksimumGazKuvveti = 0f;
        frenKuvvetToplami = 0f;
        gazKuvvetToplami = 0f;
        frenOrnekSayisi = 0;
        gazOrnekSayisi = 0;

        gazFrenAyniAndaAktif = false;
        gazFrenAyniAndaBaslangicZamani = 0f;

        seritTutmaSayaci = 0f;
        seritDegisimiAlgilandi = false;
        buTriggerdaSeritAlgilandi = false;

        karsiSeritte = false;
        karsiSeritBaslangicZamani = 0f;

        yolDisinda = false;
        yolDisiBaslangicZamani = 0f;
    }

    private float AnlikHizKmh()
    {
        if (aracRigidbody == null)
            return 0f;

        return aracRigidbody.linearVelocity.magnitude * 3.6f;
    }

    private float AnlikFren()
    {
        float fren = 0f;

        if (g29GirisSistemi != null)
            fren = Mathf.Clamp01(g29GirisSistemi.brakeOutput);

        if (Input.GetKey(KeyCode.S))
            fren = 1f;

        return fren;
    }

    private float AnlikGaz()
    {
        float gaz = 0f;

        if (g29GirisSistemi != null)
            gaz = Mathf.Clamp01(g29GirisSistemi.throttleOutput);

        if (Input.GetKey(KeyCode.W))
            gaz = 1f;

        return gaz;
    }

    private float AnlikDireksiyon()
    {
        float direksiyon = 0f;

        if (g29GirisSistemi != null)
            direksiyon = Mathf.Clamp(g29GirisSistemi.steerOutput, -1f, 1f);

        if (Input.GetKey(KeyCode.A))
            direksiyon = -1f;

        if (Input.GetKey(KeyCode.D))
            direksiyon = 1f;

        return direksiyon;
    }

    private bool SolSinyalAcik()
    {
        if (g29GirisSistemi == null || g29GirisSistemi.rccpInput == null)
            return false;

        var car = g29GirisSistemi.rccpInput.CarController;

        if (car == null || car.Lights == null)
            return false;

        return car.Lights.indicatorsLeft || car.Lights.indicatorsAll;
    }

    private bool SagSinyalAcik()
    {
        if (g29GirisSistemi == null || g29GirisSistemi.rccpInput == null)
            return false;

        var car = g29GirisSistemi.rccpInput.CarController;

        if (car == null || car.Lights == null)
            return false;

        return car.Lights.indicatorsRight || car.Lights.indicatorsAll;
    }

    private void HizKontrolu()
    {
        float hiz = AnlikHizKmh();

        bool minAlti = minimumHizAltiniUyar && hiz < minimumHizKmh;
        bool maxUstu = maksimumHizUstunuUyar && hiz > maksimumHizKmh;
        bool anormal = minAlti || maxUstu;

        if (!surekliKontrolEt)
        {
            if (minAlti && !hizAltLoglandi)
            {
                LogYaz($"{name} - HIZ MÝN ALTI | Mevcut: {hiz:F1} km/h | Min: {minimumHizKmh:F1} km/h");
                hizAltLoglandi = true;
            }

            if (maxUstu && !hizUstLoglandi)
            {
                LogYaz($"{name} - HIZ MAX ÜSTÜ | Mevcut: {hiz:F1} km/h | Max: {maksimumHizKmh:F1} km/h");
                hizUstLoglandi = true;
            }
        }
        else
        {
            if (Time.time - sonHizLogZamani >= logAraligiSaniye)
            {
                if (minAlti && (!sadeceBirKezLogla || !hizAltLoglandi))
                {
                    LogYaz($"{name} - HIZ MÝN ALTI | Mevcut: {hiz:F1} km/h | Min: {minimumHizKmh:F1} km/h");
                    hizAltLoglandi = true;
                    sonHizLogZamani = Time.time;
                }
                else if (maxUstu && (!sadeceBirKezLogla || !hizUstLoglandi))
                {
                    LogYaz($"{name} - HIZ MAX ÜSTÜ | Mevcut: {hiz:F1} km/h | Max: {maksimumHizKmh:F1} km/h");
                    hizUstLoglandi = true;
                    sonHizLogZamani = Time.time;
                }
            }
        }

        if (hizAnormaldi && !anormal)
        {
            if (normalHizaDonusuKaydet)
                LogYaz($"{name} - HIZ NORMAL ARALIKTA | Mevcut: {hiz:F1} km/h");

            hizAltLoglandi = false;
            hizUstLoglandi = false;
        }

        hizAnormaldi = anormal;
    }

    private void SinyalKontrolu()
    {
        bool sol = SolSinyalAcik();
        bool sag = SagSinyalAcik();

        if (sol && !solSinyalOncekiDurum)
        {
            solSinyalVerildi = true;
            solSinyalBaslangicZamani = Time.time;
            LogYaz($"{name} - SÝNYAL AÇILDI | Verilen: Sol | Beklenen: {gerekliSinyal}");
        }

        if (!sol && solSinyalOncekiDurum)
        {
            float sure = Time.time - solSinyalBaslangicZamani;
            LogYaz($"{name} - SÝNYAL KAPANDI | Verilen: Sol | Süre: {sure:F2} sn | Beklenen: {gerekliSinyal}");
        }

        if (sag && !sagSinyalOncekiDurum)
        {
            sagSinyalVerildi = true;
            sagSinyalBaslangicZamani = Time.time;
            LogYaz($"{name} - SÝNYAL AÇILDI | Verilen: Sað | Beklenen: {gerekliSinyal}");
        }

        if (!sag && sagSinyalOncekiDurum)
        {
            float sure = Time.time - sagSinyalBaslangicZamani;
            LogYaz($"{name} - SÝNYAL KAPANDI | Verilen: Sað | Süre: {sure:F2} sn | Beklenen: {gerekliSinyal}");
        }

        solSinyalOncekiDurum = sol;
        sagSinyalOncekiDurum = sag;
    }

    private void SinyalSonucunuYaz()
    {
        bool sol = SolSinyalAcik();
        bool sag = SagSinyalAcik();

        if (sol)
        {
            float sure = Time.time - solSinyalBaslangicZamani;
            LogYaz($"{name} - SOL SÝNYAL HALA AÇIKTI | Süre: {sure:F2} sn | Beklenen: {gerekliSinyal}");
        }

        if (sag)
        {
            float sure = Time.time - sagSinyalBaslangicZamani;
            LogYaz($"{name} - SAÐ SÝNYAL HALA AÇIKTI | Süre: {sure:F2} sn | Beklenen: {gerekliSinyal}");
        }

        if (!solSinyalVerildi && !sagSinyalVerildi)
        {
            LogYaz($"{name} - SÝNYAL VERÝLMEDÝ | Beklenen: {gerekliSinyal}");
        }
    }

    private void DurmaKontrolu()
    {
        float hiz = AnlikHizKmh();

        if (hiz < gozlenenMinimumHiz)
            gozlenenMinimumHiz = hiz;

        bool suAnDuruyor = hiz <= durmusSayilacakHizKmh;

        if (suAnDuruyor && !aracDuruyor)
        {
            aracDuruyor = true;
            durusSayaci = 0f;
            buTriggerdaDurdu = true;
            LogYaz($"{name} - ARAÇ DURDU | Hýz: {hiz:F1} km/h | Durma Eþiði: {durmusSayilacakHizKmh:F1} km/h");
        }

        if (suAnDuruyor && aracDuruyor)
            durusSayaci += Time.deltaTime;

        if (!suAnDuruyor && aracDuruyor)
        {
            LogYaz($"{name} - ARAÇ HAREKET ETTÝ | Durma Süresi: {durusSayaci:F2} sn | Beklenen: {gerekliDurusSuresiSaniye:F2} sn | En Düþük Hýz: {gozlenenMinimumHiz:F1} km/h");

            aracDuruyor = false;
            durusSayaci = 0f;
            gozlenenMinimumHiz = float.MaxValue;
        }
    }

    private void DurmaSonucunuYaz()
    {
        if (aracDuruyor)
        {
            LogYaz($"{name} - ARAÇ TRIGGER ÇIKIÞINDA HALA DURUYORDU | Durma Süresi: {durusSayaci:F2} sn | Beklenen: {gerekliDurusSuresiSaniye:F2} sn | En Düþük Hýz: {gozlenenMinimumHiz:F1} km/h");
        }

        if (!buTriggerdaDurdu)
        {
            LogYaz($"{name} - ARAÇ DURMADAN GEÇTÝ | En Düþük Hýz: {gozlenenMinimumHiz:F1} km/h | Durma Eþiði: {durmusSayilacakHizKmh:F1} km/h | Beklenen: {gerekliDurusSuresiSaniye:F2} sn");
        }
    }

    private void GazFrenKontrolu()
    {
        float fren = AnlikFren();
        float gaz = AnlikGaz();

        bool frenBasili = freniKontrolEt && fren >= frenAlgilamaEsigi;
        bool gazBasili = gaziKontrolEt && gaz >= gazAlgilamaEsigi;

        if (frenBasili && !frenBasiliydi)
        {
            frenBasiliydi = true;
            frenBaslangicZamani = Time.time;
            maksimumFrenKuvveti = 0f;
            frenKuvvetToplami = 0f;
            frenOrnekSayisi = 0;
            LogYaz($"{name} - FREN BAÞLADI | Ýlk Kuvvet: {fren:F2}");
        }

        if (frenBasili)
        {
            maksimumFrenKuvveti = Mathf.Max(maksimumFrenKuvveti, fren);
            frenKuvvetToplami += fren;
            frenOrnekSayisi++;
        }

        if (!frenBasili && frenBasiliydi)
        {
            FrenBasiminiYaz();
            frenBasiliydi = false;
        }

        if (gazBasili && !gazBasiliydi)
        {
            gazBasiliydi = true;
            gazBaslangicZamani = Time.time;
            maksimumGazKuvveti = 0f;
            gazKuvvetToplami = 0f;
            gazOrnekSayisi = 0;
            LogYaz($"{name} - GAZ BAÞLADI | Ýlk Kuvvet: {gaz:F2}");
        }

        if (gazBasili)
        {
            maksimumGazKuvveti = Mathf.Max(maksimumGazKuvveti, gaz);
            gazKuvvetToplami += gaz;
            gazOrnekSayisi++;
        }

        if (!gazBasili && gazBasiliydi)
        {
            GazBasiminiYaz();
            gazBasiliydi = false;
        }

        if (frenBasili && gazBasili && gazFrenAyniAndaKaydet && !gazFrenAyniAndaAktif)
        {
            gazFrenAyniAndaAktif = true;
            gazFrenAyniAndaBaslangicZamani = Time.time;
            LogYaz($"{name} - GAZ + FREN AYNI ANDA BAÞLADI | Fren: {fren:F2} | Gaz: {gaz:F2}");
        }

        if ((!frenBasili || !gazBasili) && gazFrenAyniAndaAktif)
        {
            float sure = Time.time - gazFrenAyniAndaBaslangicZamani;
            LogYaz($"{name} - GAZ + FREN AYNI ANDA BÝTTÝ | Süre: {sure:F2} sn");
            gazFrenAyniAndaAktif = false;
        }
    }

    private void FrenBasiminiYaz()
    {
        if (!frenBasiminiKaydet)
            return;

        float sure = Time.time - frenBaslangicZamani;
        float ortalama = frenOrnekSayisi > 0 ? frenKuvvetToplami / frenOrnekSayisi : 0f;

        string mesaj = $"{name} - FREN BIRAKILDI";

        if (frenSuresiniOlc)
            mesaj += $" | Süre: {sure:F2} sn";

        if (frenKuvvetiniOlc)
            mesaj += $" | Max Kuvvet: {maksimumFrenKuvveti:F2} | Ortalama Kuvvet: {ortalama:F2}";

        LogYaz(mesaj);
    }

    private void GazBasiminiYaz()
    {
        if (!gazBasiminiKaydet)
            return;

        float sure = Time.time - gazBaslangicZamani;
        float ortalama = gazOrnekSayisi > 0 ? gazKuvvetToplami / gazOrnekSayisi : 0f;

        string mesaj = $"{name} - GAZ BIRAKILDI";

        if (gazSuresiniOlc)
            mesaj += $" | Süre: {sure:F2} sn";

        if (gazKuvvetiniOlc)
            mesaj += $" | Max Kuvvet: {maksimumGazKuvveti:F2} | Ortalama Kuvvet: {ortalama:F2}";

        LogYaz(mesaj);
    }

    private void GazFrenSonucunuBitir()
    {
        if (gazFrenAyniAndaAktif)
        {
            float sure = Time.time - gazFrenAyniAndaBaslangicZamani;
            LogYaz($"{name} - GAZ + FREN AYNI ANDA TRIGGER ÇIKIÞINDA BÝTTÝ | Süre: {sure:F2} sn");
            gazFrenAyniAndaAktif = false;
        }

        if (frenBasiliydi)
        {
            FrenBasiminiYaz();
            frenBasiliydi = false;
        }

        if (gazBasiliydi)
        {
            GazBasiminiYaz();
            gazBasiliydi = false;
        }
    }

    private void DireksiyonlaSeritKontrolu()
    {
        float direksiyon = AnlikDireksiyon();

        bool solEsikAsildi = direksiyon <= -Mathf.Abs(direksiyonEsigi);
        bool sagEsikAsildi = direksiyon >= Mathf.Abs(direksiyonEsigi);
        bool esikAsildi = solEsikAsildi || sagEsikAsildi;

        string verilenYon = "Yok";

        if (solEsikAsildi)
            verilenYon = "Sol";
        else if (sagEsikAsildi)
            verilenYon = "Sað";

        if (esikAsildi)
        {
            seritTutmaSayaci += Time.deltaTime;

            if (!seritDegisimiAlgilandi && seritTutmaSayaci >= gerekliTutmaSuresiSaniye)
            {
                seritDegisimiAlgilandi = true;
                buTriggerdaSeritAlgilandi = true;

                LogYaz($"{name} - ÞERÝT DEÐÝÞTÝRME ALGILANDI | Verilen: {verilenYon} | Beklenen: {seritDegistirmeYonu} | Direksiyon: {direksiyon:F2} | Eþik Üstü Süre: {seritTutmaSayaci:F2} sn | Gerekli Süre: {gerekliTutmaSuresiSaniye:F2} sn");
            }
        }
        else
        {
            if (seritDegisimiAlgilandi)
            {
                LogYaz($"{name} - ÞERÝT DEÐÝÞTÝRME HAREKETÝ BÝTTÝ | Eþik Üstü Süre: {seritTutmaSayaci:F2} sn | Beklenen: {seritDegistirmeYonu}");
            }

            seritTutmaSayaci = 0f;
            seritDegisimiAlgilandi = false;
        }
    }

    private void SeritSonucunuYaz()
    {
        if (!buTriggerdaSeritAlgilandi)
        {
            LogYaz($"{name} - ÞERÝT DEÐÝÞTÝRME ALGILANMADI | Beklenen: {seritDegistirmeYonu} | Direksiyon Eþiði: {direksiyonEsigi:F2}");
        }
    }

    private void LogYaz(string mesaj)
    {
        if (!loglamaAktif)
            return;

        float oyunZamani = Time.time - sessionBaslangicZamani;
        string gercekZaman = System.DateTime.Now.ToString("HH:mm:ss.fff");

        string finalMesaj = $"[GERÇEK:{gercekZaman}] [OYUN:{oyunZamani:F3}] {mesaj}";

        if (consolaYaz)
            Debug.Log(finalMesaj);

        if (dosyayaYaz)
        {
            if (string.IsNullOrEmpty(logDosyaYolu))
                logDosyaYolu = Path.Combine(Application.persistentDataPath, logDosyaAdi);

            File.AppendAllText(logDosyaYolu, finalMesaj + "\n");
        }
    }
}